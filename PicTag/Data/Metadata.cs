using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace PicTag.Data
{
    internal enum OrderByEnum
    {
        Name,
        Date,
        Location
    }

    internal class Metadata : IDisposable
    {
        private ExifSubIfdDirectory subIfdDirectory;
        private GpsDirectory gpsDirectory;

        private double Distance => Math.Sqrt(Math.Pow(Latitude, 2) + Math.Pow(Longitude, 2));
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

        public void UpdateImageMetadata(Metadata fromMeta = null)
        {
            if (fromMeta != null)
            {
                DateTimeOriginal = fromMeta.DateTimeOriginal;
                Latitude = fromMeta.Latitude;
                Longitude = fromMeta.Longitude;
            }

            LatitudeStr = GeoLocation.DecimalToDegreesMinutesSecondsString(Latitude);
            LongitudeStr = GeoLocation.DecimalToDegreesMinutesSecondsString(Longitude);

            Source.SetPropertyItemString(ExifDirectoryBase.TagDateTimeOriginal, DateTimeOriginal.ToExifString());
            Source.Geotag(Latitude, Longitude);
        }

        internal static int Compare(Metadata i1, Metadata i2, OrderByEnum orderBy, bool ascending)
        {
            int factor = ascending ? 1 : -1;
            switch (orderBy)
            {
                case OrderByEnum.Name:
                    return i1.Name.CompareTo(i2.Name) * factor;
                case OrderByEnum.Location:
                    return i1.Distance.CompareTo(i2.Distance) * factor;
                default:
                    return i1.DateTimeOriginal.CompareTo(i2.DateTimeOriginal) * factor;
            }
        }

        public void Dispose()
        {
            Source?.Dispose();
            Source = null;
        }
    }
}
