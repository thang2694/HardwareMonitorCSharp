using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace HardwareMonitor
{
    public partial class frmabout : Form
    {
        public frmabout()
        {
            InitializeComponent();
            llFB.Links.Remove(llFB.Links[0]);
            llFB.Links.Add(0, llFB.Text.Length, "https://www.facebook.com/nvt0206");

            llSource.Links.Remove(llSource.Links[0]);
            llSource.Links.Add(0, llSource.Text.Length, "https://drive.google.com/open?id=0B1YV3b4FnlvMSmtIYVVUUzJpZUE");

            llOHM.Links.Remove(llOHM.Links[0]);
            llOHM.Links.Add(0, llOHM.Text.Length, "http://openhardwaremonitor.org/");
            label3.Text = "Version " + Application.ProductVersion;
        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Link.LinkData.ToString()));
            }
            catch { }
        }
    }
}
