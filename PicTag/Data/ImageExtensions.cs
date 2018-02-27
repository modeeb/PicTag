using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using PicTag.Properties;
using System.Text;
using System.IO;

namespace PicTag.Data
{
    internal static class ImageExtensions
    {
        internal static Metadata GetMetadata(this Image image)
        {
            var stream = new MemoryStream();
            image.Save(stream, image.RawFormat);
            stream.Position = 0;
            return new Metadata(stream);
        }

        internal static void SetPropertyItemString(this Image image, int property, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                byte[] buffer = Encoding.ASCII.GetBytes(value);
                PropertyItem propItem = Resources.Template.GetPropertyItem(property);
                propItem.Len = buffer.Length;
                propItem.Value = buffer;
                image.SetPropertyItem(propItem);
            }
        }

        /// <summary>  
        /// Sets the meta value of the bitmap. Complete list of EXIF values http://www.exiv2.org/tags.html  
        /// </summary>  
        /// <param name="image">Bitmap to which changes will be applied to</param>  
        /// <param name="property">Property enum value containing the id of the property to be changed</param>         
        /// <param name="value">Value of the proeprty to be set</param>  
        /// <returns>Returns updated bitmap</returns>  
        public static Image SetMetaValue(this Image image, int property, string value)
        {
            PropertyItem prop = image.PropertyItems[0];

            int iLen = value.Length + 1;
            byte[] bTxt = new Byte[iLen];
            for (int i = 0; i < iLen - 1; i++)
                bTxt[i] = (byte)value[i];
            bTxt[iLen - 1] = 0x00;
            prop.Id = property;
            prop.Type = 2;
            prop.Value = bTxt;
            prop.Len = iLen;
            image.SetPropertyItem(prop);
            return image;
        }

        /// <summary>  
        /// Returns meta value from the bitmap  
        /// </summary>  
        /// <param name="image">Bitmap to which changes will be applied to</param>  
        /// <param name="property">Property enum value containing the id of the property to be changed</param>  
        /// <returns>Returns value of the bitmap meta property</returns>  
        public static string GetMetaValue(this Image image, int property)
        {
            PropertyItem[] propItems = image.PropertyItems;
            var prop = propItems.FirstOrDefault(p => p.Id == property);
            if (prop != null)
            {
                return Encoding.UTF8.GetString(prop.Value);
            }
            else
            {
                return null;
            }
        }

        internal static Bitmap ResizeToBounds(this Image image, Size bounds)
        {
            return new Bitmap(image);

            int maxDimension = Math.Max(image.Height, image.Width);
            int minDimension = Math.Min(bounds.Height, bounds.Width);
            double Ratio = maxDimension / minDimension;
            int height = (int)(image.Height / Ratio);
            int width = (int)(image.Width / Ratio);
            Size newSize = new Size(height, width);


            var brush = new SolidBrush(Color.Black);

            //var graph = Graphics.FromImage(image);
            //graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
            //graph.DrawImage(image, new Rectangle(((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight));
            return new Bitmap(image, newSize);
        }

        internal static DateTime? GetMetaDate(this Image image, int property)
        {
            var dateTime = image.GetMetaValue(property);
            return dateTime.ToDateTime();
        }

        internal static DateTime? ToDateTime(this string dateTimeStr)
        {
            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;
            try
            {
                return DateTime.ParseExact(dateTimeStr.Trim('\0'), "yyyy:MM:d H:m:s", provider);
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static string ToExifString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy:MM:dd HH:mm:ss") + '\0';
        }
    }
}
