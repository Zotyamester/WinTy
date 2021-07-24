using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinTy
{

    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();
        }

        private void bStart_Click(object sender, EventArgs e)
        {
            bStart.Enabled = false;
            chklbComponents.Enabled = false;
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Dictionary<string, Method> AssociatedMethods = new Dictionary<string, Method>
            {
                { "Registry", UtilityMethods.EditRegistry },
                { "Hosts", UtilityMethods.EditHosts },
                { "WinGet and Windows Terminal", UtilityMethods.InstallCliTools },
                { "WSL2", UtilityMethods.ActivateWsl2 },
                { "Tracking", UtilityMethods.RemoveTracking },
                { "OneDrive", UtilityMethods.RemoveOneDrive },
                { "Windows Photo Viewer", UtilityMethods.ActivateWindowsPhotoViewer }
            };

            int total = chklbComponents.CheckedItems.Count;
            for (int i = 0; i < total; i++)
            {
                var item = chklbComponents.CheckedItems[i].ToString();
                worker.ReportProgress(i * 100 / total, item);
                Debug.WriteLine($"{ i + 1 } / { total } - { item }");
                AssociatedMethods[item]();
            }

            worker.ReportProgress(100);
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbCompletionStatus.Value = e.ProgressPercentage;
            lStatus.Text = (string)e.UserState;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            chklbComponents.Enabled = true;
            bStart.Enabled = true;
            lStatus.Text = "Done";
        }

        private void SelectAll(bool value)
        {
            for (int i = 0; i < chklbComponents.Items.Count; i++)
            {
                chklbComponents.SetItemChecked(i, value);
            }
        }

        private void rbAll_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAll.Checked)
            {
                SelectAll(true);
            }
        }

        private void rbNone_CheckedChanged(object sender, EventArgs e)
        {
            if (rbNone.Checked)
            {
                SelectAll(false);
            }
        }

        private void chklbComponents_SelectedIndexChanged(object sender, EventArgs e)
        {
            rbAll.Checked = false;
            rbNone.Checked = false;
            rbCustom.Checked = true;
        }
    }
}
