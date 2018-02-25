using PicTag.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicTag.UI
{
    class TreeModule
    {
        private FormState source;
        private TreeView tree;

        public TreeModule(FormState source)
        {
            this.source = source;
            this.source.PropertyChanged += Source_PropertyChanged;
        }

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(source.RootFolder) && tree != null)
            {
                Populate();
            }
        }

        internal void Bind(TreeView tree)
        {
            this.tree = tree;
            tree.AfterSelect += (o, e) => source.SelectedFolder = e.Node.FullPath;
        }

        internal void Populate()
        {
            TreeNode rootNode = NestedNodes();
            rootNode.Text = source.RootFolder;
            rootNode.Expand();

            tree.Nodes.Clear();
            tree.Nodes.Add(rootNode);
        }

        private TreeNode NestedNodes(object dir = null)
        {
            return new TreeNode(source.GetParentName(dir), source.GetChildren(dir, NestedNodes));
        }
    }
}
