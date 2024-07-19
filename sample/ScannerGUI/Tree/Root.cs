using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerGUI.Tree
{
    internal class Root
    {
        public List<Node> Nodes { get; set; } = new();
        //public TreeNode ViewNode { get; set; }
        public TreeView TreeView { get; private set; }
        public void SetTreeView (TreeView treeView)
        {
            this.TreeView = treeView;
            foreach (TreeNode node in TreeView.Nodes) 
            {
                Node mnode = new();
                mnode.SetViewNode(node);
                Nodes.Add(mnode);
            }
        }
    }
}
