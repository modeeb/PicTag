using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PicTag.Data;

namespace PicTag.UI
{
    class ListModule
    {
        private MenuModule menu;
        private FormState source;
        private ListView list;
        private Panel panel;

        public ListModule(MenuModule menu, FormState source)
        {
            this.menu = menu;
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
            source.SelectedImages = null;
            list.Items.Clear();
            list.LargeImageList.Images.Clear();
            list.SmallImageList.Images.Clear();

            foreach (var image in source.Images)
            {
                list.LargeImageList.Images.Add(image.Name, image.Source);
                list.SmallImageList.Images.Add(image.Name, image.Source);
                var item = list.Items.Add(image.Name, image.Name);
                item.Tag = image;
            }

            list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
    }
}
