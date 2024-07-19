using EtherCAT.NET;
using Microsoft.DotNet.PlatformAbstractions;
using ScannerGUI.Tree;
using System.Net.NetworkInformation;
using System.Reflection;

namespace ScannerGUI
{
    public partial class Form1 : Form
    {
        TreeNode root;
        Root OwnTree;
        NetworkInterface[] interfaces;
        string esi = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ESI");
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var codeBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Directory.EnumerateFiles(Path.Combine(codeBase, "runtimes"), "*soem_wrapper.*", SearchOption.AllDirectories).ToList().ForEach(filePath =>
            {
                if (filePath.Contains(RuntimeEnvironment.RuntimeArchitecture))
                    File.Copy(filePath, Path.Combine(codeBase, Path.GetFileName(filePath)), true);
            });
            OwnTree = new Root();
            OwnTree.SetTreeView(this.treeView1);
            if (this.treeView1.Nodes.Count > 0)
            {
                if (this.treeView1.Nodes[0].Name == "This PC")
                {
                    root = this.treeView1.Nodes[0];
                }
                else
                {
                    foreach (TreeNode node in this.treeView1.Nodes)
                    {
                        if(node.Name == "This PC")
                        {
                            this.root = node;
                        }
                    }
                }
            }
        }

        private void scanButton_Click(object sender, EventArgs e)
        {
            if(root == null) 
            {
                root = new("This PC");
                this.treeView1.Nodes.Add(root);
            }
            root.Nodes.Clear();
            interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nic in interfaces)
            {
                TreeNode node = new();
                node.Text = nic.Name;
                root.Nodes.Add(node);
                if(nic.OperationalStatus == OperationalStatus.Up)
                {
                    try
                    {
                        var rootSlave = EcUtilities.ScanDevices(nic.Name);
                        rootSlave.Descendants().ToList().ForEach(slave =>
                        {
                            EcUtilities.CreateDynamicData(esi, slave);
                        });
                    }
                    catch
                    {

                    }
                    
                }
                
            }
        }
    }
}
