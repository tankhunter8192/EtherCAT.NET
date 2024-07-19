namespace ScannerGUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            treeView1 = new TreeView();
            scanButton = new Button();
            splitter1 = new Splitter();
            SuspendLayout();
            // 
            // treeView1
            // 
            treeView1.Location = new Point(12, 41);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(234, 397);
            treeView1.TabIndex = 0;
            // 
            // scanButton
            // 
            scanButton.Location = new Point(12, 12);
            scanButton.Name = "scanButton";
            scanButton.Size = new Size(75, 23);
            scanButton.TabIndex = 1;
            scanButton.Text = "scan";
            scanButton.UseVisualStyleBackColor = true;
            scanButton.Click += scanButton_Click;
            // 
            // splitter1
            // 
            splitter1.Location = new Point(0, 0);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(3, 450);
            splitter1.TabIndex = 2;
            splitter1.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(splitter1);
            Controls.Add(scanButton);
            Controls.Add(treeView1);
            Name = "Form1";
            Text = "Ethercat Scanner";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private TreeView treeView1;
        private Button scanButton;
        private Splitter splitter1;
    }
}
