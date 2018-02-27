using PicTag.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace PicTag.UI
{
    class ToolStripRadioMenuItem<TValue> : ToolStripMenuItem where TValue : struct
    {
        private static ToolStripRadioMenuItem<TValue> oldChecked;
        protected static string propertyName;
        protected static IPropertyOwner propertyOwner;
        public static TValue Value { get; set; }

        protected ToolStripRadioMenuItem() : this(default(TValue)) { }        

        protected ToolStripRadioMenuItem(TValue value)
        {
            Text = value.ToString();
            Tag = value;
            Checked = value.Equals(Value);

            if (Checked)
                oldChecked = this;

            Click += delegate (object o, EventArgs e)
            {
                oldChecked.Checked = false;
                Checked = true;
                SetOrder(value);
                oldChecked = (ToolStripRadioMenuItem<TValue>)o;
                propertyOwner.InvokePropertyChange(propertyName);
            };
        }

        protected virtual void SetOrder(TValue value)
        {
            Value = value;
        }

        protected static IEnumerable<TEnum> EnumList<TEnum>()
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
        }

        public static ToolStripRadioMenuItem<TValue>[] CreateForProperty(string name, IPropertyOwner owner, TValue selected)
        {
            propertyName = name;
            propertyOwner = owner;
            Value = selected;
            return EnumList<TValue>().Select(v => new ToolStripRadioMenuItem<TValue>(v) ).ToArray();
        }
    }

    class OrderByToolStripItem : ToolStripRadioMenuItem<OrderByEnum>
    {
        public static bool Ascending { get; set; }

        public OrderByToolStripItem(OrderByEnum value) : base(value) { }

        private void ReverseOrder()
        {
            Ascending = !Ascending;
        }

        protected override void SetOrder(OrderByEnum value)
        {
            if (Value == value)
                ReverseOrder();
            else
                base.SetOrder(value);
        }
 
        public static OrderByToolStripItem[] CreateForOwner(MenuModule owner, OrderByEnum selected, bool ascending)
        {
            propertyName = nameof(owner.OrderBy);
            propertyOwner = owner;
            Value = selected;
            Ascending = ascending;
            return EnumList<OrderByEnum>().Select(v => new OrderByToolStripItem(v)).ToArray();
        }
    }

    interface IPropertyOwner : INotifyPropertyChanged
    {
        void InvokePropertyChange([CallerMemberName] string propertyName = "");
    }

    class MenuModule : IPropertyOwner
    {
        private ToolStripMenuItem collapseTree;
        private FormState source;

        public MenuModule(FormState source)
        {
            this.source = source;
        }

        internal void Bind(ToolStripMenuItem parent, ToolStripMenuItem collapseTree)
        {
            this.collapseTree = collapseTree;
            var listStyleMenuItems = ToolStripRadioMenuItem<View>.CreateForProperty(nameof(ListStyle), this, View.Tile);
            var orderByMenuItems = OrderByToolStripItem.CreateForOwner(this, OrderByEnum.Date, ascending: false);
            parent.DropDownItems.AddRange(listStyleMenuItems);
            parent.DropDownItems.Add(new ToolStripSeparator());
            parent.DropDownItems.AddRange(orderByMenuItems);
            collapseTree.Click += (o, e) => OnPropertyChanged(nameof(Collapsed));
        }

        public bool Collapsed => !collapseTree.Checked;

        public View ListStyle => ToolStripRadioMenuItem<View>.Value;

        public OrderByEnum OrderBy => OrderByToolStripItem.Value;

        public bool Ascending => OrderByToolStripItem.Ascending;

        public void InvokePropertyChange([CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}