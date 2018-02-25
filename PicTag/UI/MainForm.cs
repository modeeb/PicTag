using PicTag.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicTag.UI
{
    public partial class MainForm : Form
    {
        private FormState source;
        private MenuModule menu;
        private ListModule list;
        private TreeModule tree;

        public MainForm()
        {
            InitializeComponent();
        }

        internal MainForm(MenuModule menu, ListModule list, TreeModule tree, FormState source) : this()
        {
            this.menu = menu;
            this.list = list;
            this.tree = tree;
            this.source = source;

            menu.Bind(viewToolStripMenuItem, foldersTreeToolStripMenuItem);
            list.Bind(listView1, panel1);
            tree.Bind(treeView1);
            tree.Populate();

            menuModuleBindingSource.DataSource = menu;
            formStateBindingSource.DataSource = source;
        }

        private void OpenToolStripButton_Click(object sender, EventArgs e)
        {
            //openFileDialog1.InitialDirectory = state.CurrentDirectory.FullName;
            //openFileDialog1.FileName = "test";
            //if (openFileDialog1.ShowDialog() == DialogResult.OK)
            //{
            //    state.CurrentDirectory = new DirectoryInfo(openFileDialog1.FileName);
            //}
            folderBrowserDialog1.SelectedPath = source.RootFolder;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                source.RootFolder = folderBrowserDialog1.SelectedPath;
            }
        }

        private void SaveToolStripButton_Click(object sender, EventArgs e)
        {
            ValidateChildren();
            source.Save();
        }

        private void CopyToolStripButton_Click(object sender, EventArgs e)
        {
            source.CopyMetadata();
        }

        private void PasteToolStripButton_Click(object sender, EventArgs e)
        {
            source.PasteMetadata();
        }
    }
}
