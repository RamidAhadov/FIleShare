using System.Runtime.InteropServices;
using FileShare.App.Icons;

namespace FileShare.App
{
    partial class Application
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnlMain = new Panel();
            pnlCommon = new Panel();
            btnSendFile = new Button();
            pnlDirectory = new Panel();
            tvDirectory = new TreeView();
            icons = new ImageList(components);
            btnSearchDevice = new Button();
            btnStartEngine = new Button();
            btnRestartEngine = new Button();
            pnlDevices = new Panel();
            pbarSearchDevices = new ProgressBar();
            pbarStartEngine = new ProgressBar();
            pbarSendFile = new ProgressBar();
            pnlMain.SuspendLayout();
            pnlCommon.SuspendLayout();
            pnlDirectory.SuspendLayout();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.Controls.Add(pnlCommon);
            pnlMain.Controls.Add(pnlDevices);
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(0, 0);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(1290, 605);
            pnlMain.TabIndex = 0;
            // 
            // pnlCommon
            // 
            pnlCommon.Controls.Add(btnSendFile);
            pnlCommon.Controls.Add(pnlDirectory);
            pnlCommon.Controls.Add(btnSearchDevice);
            pnlCommon.Controls.Add(btnStartEngine);
            pnlCommon.Controls.Add(btnRestartEngine);
            pnlCommon.Controls.Add(pbarSearchDevices);
            pnlCommon.Controls.Add(pbarStartEngine);
            pnlCommon.Controls.Add(pbarSendFile);
            pnlCommon.Dock = DockStyle.Fill;
            pnlCommon.Location = new Point(270, 0);
            pnlCommon.Name = "pnlCommon";
            pnlCommon.Size = new Size(1020, 605);
            pnlCommon.TabIndex = 3;
            // 
            // btnSendFile
            // 
            btnSendFile.Location = new Point(797, 12);
            btnSendFile.Name = "btnSendFile";
            btnSendFile.Size = new Size(220, 60);
            btnSendFile.TabIndex = 4;
            btnSendFile.Text = "Send file";
            btnSendFile.UseVisualStyleBackColor = true;
            btnSendFile.Click += btnSendFile_Click;
            // 
            // pnlDirectory
            // 
            pnlDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlDirectory.Controls.Add(tvDirectory);
            pnlDirectory.Location = new Point(6, 109);
            pnlDirectory.Name = "pnlDirectory";
            pnlDirectory.Size = new Size(1011, 493);
            pnlDirectory.TabIndex = 3;
            // 
            // tvDirectory
            // 
            tvDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tvDirectory.ImageIndex = 0;
            tvDirectory.ImageList = icons;
            tvDirectory.Location = new Point(3, 3);
            tvDirectory.Name = "tvDirectory";
            tvDirectory.SelectedImageIndex = 0;
            tvDirectory.Size = new Size(1008, 490);
            tvDirectory.TabIndex = 3;
            // 
            // icons
            // 
            icons.Images.Add("folder", DefaultIcons.FolderLarge);
            icons.ColorDepth = ColorDepth.Depth32Bit;
            icons.ImageSize = new Size(16, 16);
            icons.TransparentColor = Color.Transparent;
            // 
            // btnSearchDevice
            // 
            btnSearchDevice.Location = new Point(6, 12);
            btnSearchDevice.Name = "btnSearchDevice";
            btnSearchDevice.Size = new Size(220, 60);
            btnSearchDevice.TabIndex = 1;
            btnSearchDevice.Text = "Search for devices";
            btnSearchDevice.UseVisualStyleBackColor = true;
            btnSearchDevice.Click += btnSearchDevice_Click;
            // 
            // btnStartEngine
            // 
            btnStartEngine.Location = new Point(229, 12);
            btnStartEngine.Name = "btnStartEngine";
            btnStartEngine.Size = new Size(220, 60);
            btnStartEngine.TabIndex = 2;
            btnStartEngine.Text = "Start engine";
            btnStartEngine.UseVisualStyleBackColor = true;
            btnStartEngine.Click += btnStartEngine_Click;
            // 
            // btnStartEngine
            // 
            btnRestartEngine.Location = new Point(451, 12);
            btnRestartEngine.Name = "btnRestartEngine";
            btnRestartEngine.Size = new Size(220, 60);
            btnRestartEngine.TabIndex = 2;
            btnRestartEngine.Text = "Restart engine";
            btnRestartEngine.UseVisualStyleBackColor = true;
            btnRestartEngine.Click += btnRestartEngine_Click;
            //
            // pbarSearchDevices
            //
            pbarSearchDevices.Location = new Point(7, 75);
            pbarSearchDevices.Size = new Size(217, 33);
            pbarSearchDevices.Minimum = 0;
            pbarSearchDevices.Value = 0;
            pbarSearchDevices.Name = "pbarStartEngine";
            pbarSearchDevices.Visible = false;
            //
            // pbarStartEngine
            //
            pbarStartEngine.Location = new Point(230, 75);
            pbarStartEngine.Size = new Size(218, 33);
            pbarStartEngine.Minimum = 0;
            pbarStartEngine.Value = 0;
            pbarStartEngine.Name = "pbarStartEngine";
            pbarStartEngine.Visible = false;
            //
            // pbarSendFile
            //
            pbarSendFile.Location = new Point(798, 75);
            pbarSendFile.Size = new Size(218, 33);
            pbarSendFile.Minimum = 0;
            pbarSendFile.Value = 0;
            pbarSendFile.Name = "pbarSendFile";
            pbarSendFile.Visible = false;
            // 
            // pnlDevices
            // 
            pnlDevices.Dock = DockStyle.Left;
            pnlDevices.Location = new Point(0, 0);
            pnlDevices.Name = "pnlDevices";
            pnlDevices.Size = new Size(270, 605);
            pnlDevices.TabIndex = 0;
            // 
            // Application
            // 
            AutoScaleMode = AutoScaleMode.None;
            AutoSize = true;
            ClientSize = new Size(1290, 605);
            Controls.Add(pnlMain);
            Name = "Application";
            Text = "Application";
            Load += Application_Load;
            pnlMain.ResumeLayout(false);
            pnlCommon.ResumeLayout(false);
            pnlDirectory.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlMain;
        private Panel pnlDevices;
        private Button btnSearchDevice;
        private Button btnStartEngine;
        private Button btnRestartEngine;
        private Panel pnlCommon;
        private TreeView tvDirectory;
        private Panel pnlDirectory;
        private ImageList icons;
        private Button btnSendFile;
        private ProgressBar pbarSearchDevices;
        private ProgressBar pbarStartEngine;
        private ProgressBar pbarSendFile;
    }

    
}