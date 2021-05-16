using PicTag.Data;
using PicTag.Properties;
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
        private readonly FormState source;
        private readonly MenuModule menu;
        private readonly ListModule list;
        private readonly TreeModule tree;

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

        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            ValidateChildren();
            source.SaveSelected();
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (source.ImageInfo != null)
            {
                ValidateChildren();
                saveFileDialog1.InitialDirectory = source.SelectedFolder;
                saveFileDialog1.FileName = source.ImageInfo.Name;
                saveFileDialog1.DefaultExt = System.IO.Path.GetExtension(source.ImageInfo.Name);
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var newPath = saveFileDialog1.FileName;
                    source.SaveAs(newPath);
                }
                list.Populate();
            }
        }

        private void SaveToolStripButton_Click(object sender, EventArgs e)
        {
            ValidateChildren();
            list.SaveAll();
        }

        private void CopyToolStripButton_Click(object sender, EventArgs e)
        {
            source.CopyMetadata();
        }

        private void PasteToolStripButton_Click(object sender, EventArgs e)
        {
            source.PasteMetadata();
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Resources.DeletingText, Resources.DeletingCaption, MessageBoxButtons.YesNo);
            list.Delete();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ToolStripButton4_Click(object sender, EventArgs e)
        {
            if (menu.OrderBy != OrderByEnum.Date)
                return;

            list.MoveSelectedUp();
        }

        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            if (menu.OrderBy != OrderByEnum.Date)
                return;

            list.MoveSelectedDown();
        }
    }
}
