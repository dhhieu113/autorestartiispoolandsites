using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace IISAutoRestartApp
{
    public partial class Form1 : Form
    {
        private int checkInterval = 5000;
        private List<string> notActivePools = new List<string>();
        private List<string> notActiveWebsites = new List<string>();
        ServerManager server = new ServerManager();
        private string poolLogFiles = "pools-check.txt";
        private string websiteLogFiles = "websites-check.txt";
        public Form1()
        {
            InitializeComponent();
            timer1.Enabled = true;
            timer1.Start();
            var intervalStr = (checkInterval / 1000).ToString();
            textBox1.Text = intervalStr;
            lblcurrent.Text = "Hiện tại: " + intervalStr + "s";
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
            timer1.Stop();
            timer1.Enabled = false;
            timer1.Dispose();
        }
    }
}
