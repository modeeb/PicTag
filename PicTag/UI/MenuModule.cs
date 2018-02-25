using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace PicTag.UI
{
    class MenuModule : INotifyPropertyChanged
    {
        private bool collapsed;
        private View listView;
        private ToolStripMenuItem[] menuItems;

        public MenuModule()
        {
            listView = View.LargeIcon;
        }

        internal void Bind(ToolStripMenuItem parent, ToolStripMenuItem collapseTree)
        {
            menuItems = PopulateMenuItems().ToArray();
            parent.DropDownItems.AddRange(menuItems);
            collapseTree.Click += (o, e) => Collapsed = !Collapsed;
        }

        internal IEnumerable<ToolStripMenuItem> PopulateMenuItems()
        {
            foreach (View view in Enum.GetValues(typeof(View)))
            {
                var menu = new ToolStripMenuItem()
                {
                    Text = view.ToString(),
                    Tag = view,
                    Checked = view == listView
                };
                menu.Click += (o, e) => ListViewStyle = view;
                yield return menu;
            }
        }

        public bool Collapsed
        {
            get => collapsed;
            internal set
            {
                if (value != collapsed)
                {
                    collapsed = value;
                    OnPropertyChanged();
                }
            }
        }

        public View ListViewStyle
        {
            get => listView;
            internal set
            {
                if (value != listView)
                {
                    listView = value;
                    Array.ForEach(menuItems, m => m.Checked = (View)m.Tag == listView);
                    OnPropertyChanged();
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}