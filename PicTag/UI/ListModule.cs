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
    class ListViewItem<T> : ListViewItem
    {
        public new T Tag { get; set; }
    }

    class ListModule
    {
        private FormState source;
        private MenuModule menu;
        private ListView list;
        private Panel panel;

        public ListModule(FormState source, MenuModule menu)
        {
            this.source = source;
            this.menu = menu;
            this.source.PropertyChanged += Source_PropertyChanged;
            this.menu.PropertyChanged += Source_PropertyChanged;
        }

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case (nameof(source.ImageInfo)):
                    panel.Visible = source.ImageInfo != null;
                    break;
                case (nameof(source.SelectedFolder)):
                    Populate();
                    break;
                case (nameof(menu.OrderBy)):
                case (nameof(menu.Ascending)):
                    Sort();
                    break;
                default:
                    break;
            }
        }

        internal void Bind(ListView list, Panel panel)
        {
            this.list = list;
            this.panel = panel;
            list.ItemSelectionChanged += (o, e) => source.SelectedImages = from ListViewItem<Metadata> item 
                                                                           in list.SelectedItems
                                                                           select item.Tag;
        }

        internal void Populate()
        {
            ClearAll();
            Sort();

            foreach (var image in source.Images)
            {
                list.LargeImageList.Images.Add(image.Name, image.Source.ResizeToBounds(list.LargeImageList.ImageSize));
                list.SmallImageList.Images.Add(image.Name, image.Source.ResizeToBounds(list.SmallImageList.ImageSize));
                ListViewItem<Metadata> item = new ListViewItem<Metadata>
                {
                    Text = image.Name,
                    ImageKey = image.Name,
                    Tag = image,
                    UseItemStyleForSubItems = false
                };
                item.SubItems.AddRange(new string[] {
                    image.DateTimeOriginal.ToLongDateString(),
                    image.DateTimeOriginal.ToLongTimeString(),
                    image.LatitudeStr + ", " + image.LongitudeStr },
                    Color.Gray, item.BackColor, item.Font);
                list.Items.Add(item);
            }
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

        private void Sort()
        {
            list.ListViewItemSorter = Comparer<ListViewItem<Metadata>>.Create((i1, i2) => Metadata.Compare(i1.Tag, i2.Tag, menu.OrderBy, menu.Ascending));
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
            var items = from ListViewItem<Metadata> item
                        in list.Items
                        select item.Tag;
            source.Save(items);
        }

        internal void MoveSelectedUp()
        {
            var items = from ListViewItem<Metadata> item
                        in list.SelectedItems
                        select item;

            double value = menu.Ascending ? -1 : 1;
            foreach (var item in items)
            {
                list.Items.Remove(item);
                item.Tag.DateTimeOriginal = item.Tag.DateTimeOriginal.AddMilliseconds(value);
                list.Items.Add(item);
                list.EnsureVisible(item.Index);
            }
        }

        internal void MoveSelectedDown()
        {
            var items = from ListViewItem<Metadata> item
                        in list.SelectedItems
                        select item;

            double value = menu.Ascending ? 1 : -1;
            foreach (var item in items)
            {
                list.Items.Remove(item);
                item.Tag.DateTimeOriginal = item.Tag.DateTimeOriginal.AddMilliseconds(value);
                list.Items.Add(item);
                list.EnsureVisible(item.Index);
            }
        }
    }
}
