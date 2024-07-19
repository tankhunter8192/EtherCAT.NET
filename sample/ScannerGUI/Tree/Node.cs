using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerGUI.Tree
{
    internal class Node
    {
        public List<Node> Nodes {  get; set; } = new List<Node>();
        public TreeNode? ViewNode { get; private set; }
        public object? obj { get; set; }
        public void SetViewNode(TreeNode view)
        {
            ViewNode = view;
            foreach (TreeNode node in ViewNode.Nodes) 
            {
                Node mnode = new();
                mnode.SetViewNode(node);
                Nodes.Add(mnode);
            }
        }
    }
}
