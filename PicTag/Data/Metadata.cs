using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using PicTag.Properties;

namespace PicTag.Data
{
    internal class Metadata : IDisposable
    {
        private ExifSubIfdDirectory subIfdDirectory;
        private GpsDirectory gpsDirectory;

        public Image Source { get; set; }
        public DateTime DateTimeOriginal { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string LatitudeStr
        {
            get => GeoLocation.DecimalToDegreesMinutesSecondsString(Latitude);
            set => Latitude = GeotaggingExt.DegreesMinutesSecondsStringToDecimal(value);
        }
        public string LongitudeStr
        {
            get => GeoLocation.DecimalToDegreesMinutesSecondsString(Longitude);
            set => Longitude = GeotaggingExt.DegreesMinutesSecondsStringToDecimal(value);
        }

        public Metadata(Stream stream)
        {
            MetaExtractor(stream);
            Source = Image.FromStream(stream);
        }

        internal Metadata AppendFileInfo(string name, string fullName, DateTime lastWriteTime)
        {
            Name = name;
            FullName = fullName;
            if (DateTimeOriginal == DateTime.MinValue)
                DateTimeOriginal = lastWriteTime;
            return this;
        }

        private void MetaExtractor(Stream stream)
        {
            var directories = ImageMetadataReader.ReadMetadata(stream);
            subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            var dateTime = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
            if (dateTime.HasValue)
            {
                DateTimeOriginal = dateTime.Value;
            }

            gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault();
            if (gpsDirectory != null)
            {
                var geolocation = gpsDirectory?.GetGeoLocation();
                Latitude = geolocation.Latitude;
                Longitude = geolocation.Longitude;
            }
        }

        public void UpdateImageMetadata(Metadata meta = null)
        {
            if (meta != null)
            {
                DateTimeOriginal = meta.DateTimeOriginal;
                Latitude = meta.Latitude;
                Longitude = meta.Longitude;
            }

            LatitudeStr = GeoLocation.DecimalToDegreesMinutesSecondsString(Latitude);
            LongitudeStr = GeoLocation.DecimalToDegreesMinutesSecondsString(Longitude);

            Source.SetPropertyItemString(ExifDirectoryBase.TagDateTimeOriginal, DateTimeOriginal.ToExifString());
            Source.Geotag(Latitude, Longitude);
        }

        private static PropertyItem GetNewPropertyItem()
        {
            return (PropertyItem)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(PropertyItem));
        }

        static void WriteCoordinatesToImage(string filename, double latitude, double longitude)
        {
            // copy longitude into byte array
            byte[] bLat = BitConverter.GetBytes(latitude);
            byte[] bLong = BitConverter.GetBytes(longitude);

            //load the image to change
            Image Pic = Image.FromFile(filename);

            //put longitude into the right property item
            PropertyItem[] PropertyItems = Pic.PropertyItems;
            PropertyItems[0].Id = 0x0002;
            PropertyItems[0].Type = 5;
            PropertyItems[0].Len = bLong.Length;
            PropertyItems[0].Value = bLong;
            Pic.SetPropertyItem(PropertyItems[0]);

            //put latitude into the right property item
            PropertyItems = Pic.PropertyItems;
            PropertyItems[0].Id = 0x0004;
            PropertyItems[0].Type = 5;
            PropertyItems[0].Len = bLat.Length;
            PropertyItems[0].Value = bLat;
            Pic.SetPropertyItem(PropertyItems[0]);

            // we cannot store in the same image, so use a temporary image instead
            string FilenameTemp = filename + ".temp";

            // for lossless rewriting must rotate the image by 90 degrees!
            EncoderParameters EncParms = new EncoderParameters(1);
            EncParms.Param[0] = new EncoderParameter(Encoder.Transformation, (long)EncoderValue.TransformRotate90); ;

            ImageCodecInfo CodecInfo = GetEncoderInfo("image/jpeg");
            // now write the rotated image with new description
            Pic.Save(FilenameTemp, CodecInfo, EncParms);

            // for computers with low memory and large pictures: release memory now
            Pic.Dispose();
            Pic = null;
            GC.Collect();

            // delete the original file, will be replaced later
            File.Delete(filename);

            // now must rotate back the written picture
            Pic = Image.FromFile(FilenameTemp);
            EncParms.Param[0] = new EncoderParameter(Encoder.Transformation, (long)EncoderValue.TransformRotate270); ;
            Pic.Save(filename, CodecInfo, EncParms);

            // release memory now
            Pic.Dispose();
            Pic = null;
            GC.Collect();

            // delete the temporary picture
            File.Delete(FilenameTemp);
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        public void Dispose()
        {
            Source?.Dispose();
            Source = null;
        }
    }
}
