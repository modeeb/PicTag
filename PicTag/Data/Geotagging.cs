using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicTag.Data
{
    static class GeotaggingExt
    {
        // These constants come from the CIPA DC-008 standard for EXIF 2.3
        const short ExifTypeByte = 1;
        const short ExifTypeAscii = 2;
        const short ExifTypeRational = 5;

        const int ExifTagGPSVersionID = 0x0000;
        const int ExifTagGPSLatitudeRef = 0x0001;
        const int ExifTagGPSLatitude = 0x0002;
        const int ExifTagGPSLongitudeRef = 0x0003;
        const int ExifTagGPSLongitude = 0x0004;

        internal static Image Geotag(this Image image, double lat, double lng)
        {
            char latHemisphere = 'N';
            if (lat < 0)
            {
                latHemisphere = 'S';
                lat = -lat;
            }
            char lngHemisphere = 'E';
            if (lng < 0)
            {
                lngHemisphere = 'W';
                lng = -lng;
            }

            AddProperty(image, ExifTagGPSVersionID, ExifTypeByte, new byte[] { 2, 3, 0, 0 });
            AddProperty(image, ExifTagGPSLatitudeRef, ExifTypeAscii, new byte[] { (byte)latHemisphere, 0 });
            AddProperty(image, ExifTagGPSLatitude, ExifTypeRational, ConvertToRationalTriplet(lat));
            AddProperty(image, ExifTagGPSLongitudeRef, ExifTypeAscii, new byte[] { (byte)lngHemisphere, 0 });
            AddProperty(image, ExifTagGPSLongitude, ExifTypeRational, ConvertToRationalTriplet(lng));

            return image;
        }

        internal static double DegreesMinutesSecondsStringToDecimal(string value)
        {
            if (value != null)
            {
                double[] parts = value.Split(new char[] { '°', ' ', '\'', '\"' }, StringSplitOptions.RemoveEmptyEntries).Select(p => double.Parse(p)).ToArray();
                return parts[0] + parts[1] / 60.0 + parts[2] / 3600;
            }
            return 0;
        }

        static byte[] ConvertToRationalTriplet(double value)
        {
            int degrees = (int)Math.Floor(value);
            value = (value - degrees) * 60;
            int minutes = (int)Math.Floor(value);
            value = (value - minutes) * 60 * 100;
            int seconds = (int)Math.Round(value);
            byte[] bytes = new byte[3 * 2 * 4]; // Degrees, minutes, and seconds, each with a numerator and a denominator, each composed of 4 bytes
            int i = 0;
            Array.Copy(BitConverter.GetBytes(degrees), 0, bytes, i, 4); i += 4;
            Array.Copy(BitConverter.GetBytes(1), 0, bytes, i, 4); i += 4;
            Array.Copy(BitConverter.GetBytes(minutes), 0, bytes, i, 4); i += 4;
            Array.Copy(BitConverter.GetBytes(1), 0, bytes, i, 4); i += 4;
            Array.Copy(BitConverter.GetBytes(seconds), 0, bytes, i, 4); i += 4;
            Array.Copy(BitConverter.GetBytes(100), 0, bytes, i, 4);
            return bytes;
        }

        static void AddProperty(this Image img, int id, short type, byte[] value)
        {
            PropertyItem pi = img.PropertyItems[0];
            pi.Id = id;
            pi.Type = type;
            pi.Len = value.Length;
            pi.Value = value;
            img.SetPropertyItem(pi);
        }
    }
}
