using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PicTag.Data;

namespace PicTag.UI
{
    class ListModule
    {
        private FormState source;
        private ListView list;
        private Panel panel;

        public ListModule(FormState source)
        {
            this.source = source;
            this.source.PropertyChanged += Source_PropertyChanged;
        }

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(source.SelectedFolder) && list != null)
            {
                Populate();
            }
            if (e.PropertyName == nameof(source.ImageInfo) && panel != null)
            {
                panel.Visible = source.ImageInfo != null;
            }
        }

        internal void Bind(ListView list, Panel panel)
        {
            this.list = list;
            this.panel = panel;
            list.ItemSelectionChanged += (o, e) => source.SelectedImages = from ListViewItem item 
                                                                           in list.SelectedItems
                                                                           select item.Tag;
        }

        internal void Populate()
        {
            ClearAll();

            foreach (var image in source.Images)
            {
                list.LargeImageList.Images.Add(image.Name, image.Source);
                list.SmallImageList.Images.Add(image.Name, image.Source);
                var item = list.Items.Add(image.Name, image.Name);
                item.Tag = image;
                item.UseItemStyleForSubItems = false;
                item.SubItems.AddRange(new string[] {
                    image.DateTimeOriginal.ToLongDateString(),
                    image.DateTimeOriginal.ToLongTimeString(),
                    "Latitude " + image.LatitudeStr, "Longitude " + image.LongitudeStr }
                    , Color.Gray, item.BackColor, item.Font);
            }

            list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void ClearAll()
        {
            foreach (var image in source.Images)
            {
                image.Dispose();
            }
            source.SelectedImages = null;
            list.Items.Clear();
            list.LargeImageList.Images.Clear();
            list.SmallImageList.Images.Clear();
            GC.Collect();
        }

        internal void Delete()
        {
            source.DeleteSelected();
            var items = from ListViewItem item
                        in list.SelectedItems
                        select item;
            foreach (var item in items)
            {
                list.Items.Remove(item);
            }            
        }

        internal void SaveAll()
        {
            var items = from ListViewItem item
                        in list.Items
                        select item.Tag;
            source.Save(items);
        }
    }
}
