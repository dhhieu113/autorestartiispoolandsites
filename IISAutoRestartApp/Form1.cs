using Microsoft.Web.Administration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Windows.Forms;

namespace IISAutoRestartApp
{
    public partial class Form1 : Form
    {
        private bool IsStart = false;
        private int checkInterval = 5000;
        private List<string> notActivePools = new List<string>();
        private List<string> notActiveWebsites = new List<string>();
        ServerManager server = new ServerManager();
        private string poolLogFiles = "pools-check.txt";
        private string websiteLogFiles = "websites-check.txt";
        public Form1()
        {
            InitializeComponent();
            if(IsAdministrator())
            {
                StartApp();
            }
            else
            {
                MessageBox.Show("Please run as Administrator for IIS permission");
            }
        }

        private void StartApp()
        {
            IsStart = true;
            timer1.Enabled = true;
            timer1.Start();
            var intervalStr = (checkInterval / 1000).ToString();
            textBox1.Text = intervalStr;
            lblcurrent.Text = "Hiện tại: " + intervalStr + "s";
            AddStartUp();
        }

        private bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }


        private void AddStartUp(bool enabled = true)
        {
            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (enabled)
                {
                    key.SetValue("auto_restart_iis", System.Windows.Forms.Application.ExecutablePath);
                }
                else
                {
                    key.DeleteValue("auto_restart_iis", false);
                }
            }
            catch { }
        }

        private void LoadStopedPools()
        {
            // notActivePools = new List<string>();

            foreach (var pool in server.ApplicationPools)
            {
                try
                {
                    if (pool.State == ObjectState.Stopped)
                    {
                        var msg = pool.Name + $" was stopped at {DateTime.Now}";
                        notActivePools.Add(msg);
                        LogPool(msg);
                        pool.Start();
                        var startMsg = $"{pool.Name} is started at {DateTime.Now}";
                        LogPool(startMsg);
                        notActivePools.Add(startMsg);
                    }
                }
                catch (Exception ex)
                {
                    LogPool(ex.Message);
                }
            }

            richTextBox1.Text = string.Join("\n", notActivePools);
        }

        private void LoadStoppedWebsites()
        {
            //notActiveWebsites = new List<string>();
            foreach (var web in server.Sites)
            {
                try
                {
                    if (web.State == ObjectState.Stopped)
                    {
                        var msg = web.Name + $" was stopped at {DateTime.Now}";
                        notActiveWebsites.Add(msg);
                        LogWebsite(msg);
                        web.Start();
                        var startMsg = $"{web.Name} is started at {DateTime.Now}";
                        LogWebsite(startMsg);
                        notActiveWebsites.Add(startMsg);
                    }
                }
                catch (Exception ex)
                {
                    LogWebsite(ex.Message);
                }
            }

            richTextBox2.Text = string.Join("\n", notActiveWebsites);
        }

        private void LogPool(string msg)
        {
            System.IO.File.AppendAllText(poolLogFiles, "\n" + msg);
        }

        private void LogWebsite(string msg)
        {
            System.IO.File.AppendAllText(websiteLogFiles, "\n" + msg);
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            LoadStopedPools();
            LoadStoppedWebsites();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!IsStart) return;
            try
            {
                checkInterval = int.Parse(textBox1.Text) * 1000;
                var intervalStr = (checkInterval / 1000).ToString();
                lblcurrent.Text = "Hiện tại: " + intervalStr + "s";

                timer1.Stop();
                timer1.Interval = checkInterval;
                timer1.Start();
            }
            catch
            {
                MessageBox.Show("Interval là giá trị từ 1 - 99, đơn vị: s (giây)");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                timer1.Stop();
                timer1.Enabled = false;
                timer1.Dispose();
            }
            catch { }
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsStart) return;
            AddStartUp(checkBox1.Checked);
        }
    }
}
