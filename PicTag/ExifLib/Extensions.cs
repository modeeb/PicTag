using ExifLib;
using PicTag.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicTag.ExifLib
{
    internal static class Extensions
    {
        internal static Metadata UseExifReader(this Metadata meta)
        {
            using (ExifReader reader = new ExifReader(meta.FullName))
            {
                if (reader.GetTagValue(ExifTags.DateTimeOriginal, out DateTime dateTimeOriginal))
                {
                    meta.DateTimeOriginal = dateTimeOriginal;
                }
                else if (reader.GetTagValue(ExifTags.DateTime, out dateTimeOriginal))
                {
                    meta.DateTimeOriginal = dateTimeOriginal;
                }

                if (reader.GetTagValue(ExifTags.GPSLatitude, out double[] latitude))
                {
                    meta.LatitudeStr = string.Format("{0}° {1}' {2}\"", latitude[0], latitude[1], latitude[2]);
                    meta.Latitude = latitude[0] + latitude[1] / 60.0 + latitude[2] / 3600.0;
                }
                if (reader.GetTagValue(ExifTags.GPSLongitude, out double[] longitude))
                {
                    meta.LongitudeStr = string.Format("{0}° {1}' {2}\"", longitude[0], longitude[1], longitude[2]);
                    meta.Longitude = longitude[0] + longitude[1] / 60.0 + longitude[2] / 3600.0;
                }
            }
            return meta;
        }
    }
}
