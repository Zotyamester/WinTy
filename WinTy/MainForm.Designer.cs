﻿
namespace WinTy
{
    partial class MainForm
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
            this.chklbComponents = new System.Windows.Forms.CheckedListBox();
            this.bStart = new System.Windows.Forms.Button();
            this.pbCompletionStatus = new System.Windows.Forms.ProgressBar();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.lStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chklbComponents
            // 
            this.chklbComponents.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chklbComponents.CheckOnClick = true;
            this.chklbComponents.FormattingEnabled = true;
            this.chklbComponents.Items.AddRange(new object[] {
            "Registry",
            "Hosts",
            "WinGet and Windows Terminal",
            "WSL2",
            "Tracking",
            "OneDrive",
            "Windows Photo Viewer"});
            this.chklbComponents.Location = new System.Drawing.Point(12, 12);
            this.chklbComponents.Name = "chklbComponents";
            this.chklbComponents.Size = new System.Drawing.Size(280, 130);
            this.chklbComponents.TabIndex = 0;
            // 
            // bStart
            // 
            this.bStart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bStart.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.bStart.Location = new System.Drawing.Point(12, 214);
            this.bStart.Name = "bStart";
            this.bStart.Size = new System.Drawing.Size(280, 59);
            this.bStart.TabIndex = 3;
            this.bStart.Text = "Start";
            this.bStart.UseVisualStyleBackColor = true;
            this.bStart.Click += new System.EventHandler(this.bStart_Click);
            // 
            // pbCompletionStatus
            // 
            this.pbCompletionStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbCompletionStatus.Location = new System.Drawing.Point(12, 153);
            this.pbCompletionStatus.Name = "pbCompletionStatus";
            this.pbCompletionStatus.Size = new System.Drawing.Size(280, 23);
            this.pbCompletionStatus.TabIndex = 1;
            // 
            // worker
            // 
            this.worker.WorkerReportsProgress = true;
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.worker_DoWork);
            this.worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.worker_ProgressChanged);
            this.worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
            // 
            // lStatus
            // 
            this.lStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lStatus.Location = new System.Drawing.Point(0, 188);
            this.lStatus.Name = "lStatus";
            this.lStatus.Size = new System.Drawing.Size(304, 15);
            this.lStatus.TabIndex = 2;
            this.lStatus.Text = "Select the desired components";
            this.lStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 281);
            this.Controls.Add(this.lStatus);
            this.Controls.Add(this.pbCompletionStatus);
            this.Controls.Add(this.bStart);
            this.Controls.Add(this.chklbComponents);
            this.Name = "MainForm";
            this.Text = "WinTy";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox chklbComponents;
        private System.Windows.Forms.Button bStart;
        private System.Windows.Forms.ProgressBar pbCompletionStatus;
        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.Label lStatus;
    }
}
