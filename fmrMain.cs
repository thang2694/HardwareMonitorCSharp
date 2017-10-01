using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.IO.Ports;
using System.Configuration;
using System.Net.NetworkInformation;
using OpenHardwareMonitor.Hardware;



namespace HardwareMonitor
{
    public partial class fmrMain : Form
    {
        Computer c = new Computer()
        {
            GPUEnabled = true,
            CPUEnabled = true,
            RAMEnabled = true

        };
        string a;
        float CPUTemp, CPULoad, GPULoad, GPURam, GPUTemp, RAMUsed, x, y;
        long byteRec, byteSent, oldRec, oldSent;
        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

        public fmrMain()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            serialPort1.BaudRate = 9600;
            serialPort1.Parity = Parity.None;
            serialPort1.StopBits = StopBits.One;
            serialPort1.DataBits = 8;
            serialPort1.Handshake = Handshake.None;
            serialPort1.RtsEnable = true;
            //cbbCom.DataSource = SerialPort.GetPortNames();
        }
        
        public void _showBalloon(string text, int time)
        {
            notifyIcon1.BalloonTipText = text;
            notifyIcon1.ShowBalloonTip(time);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtTimer.Text = ConfigurationManager.AppSettings["timer"].ToString();
            cbbCom.Text = ConfigurationManager.AppSettings["com"].ToString();
            timer1.Interval = (int)(double.Parse(txtTimer.Text) * 1000);
            c.Open();
            if (cbbCom.Text != "")
            {
                try
                {
                    serialPort1.PortName = cbbCom.Text;
                    if (!serialPort1.IsOpen)
                    {
                        serialPort1.Open();
                        serialPort1.Write("?");
                        timer1.Start();
                        btnConn.Text = "Disconnect";
                        cbbCom.BackColor = Color.Blue;
                        _showBalloon("LCD module connected!", 300);
                    }
                }
                catch
                {
                    _showBalloon("LCD module not found!", 300);
                }
            }
            timer1.Start();
        }

        private void thoátToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmabout frm = new frmabout();
            frm.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                foreach (var hardware in c.Hardware)
                {
                    if (hardware.HardwareType == HardwareType.CPU)
                    {
                        hardware.Update();
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature && sensor.Name == "CPU Package")
                            {
                                CPUTemp = sensor.Value.GetValueOrDefault();
                            }
                            if (sensor.SensorType == SensorType.Load && sensor.Name == "CPU Total")
                            {
                                CPULoad = sensor.Value.GetValueOrDefault();
                            }
                        }
                    }

                    if (hardware.HardwareType == HardwareType.GpuNvidia)
                    {
                        hardware.Update();
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Load)
                            {
                                GPULoad = sensor.Value.GetValueOrDefault();
                            }
                            if (sensor.SensorType == SensorType.SmallData && sensor.Name == "GPU Memory Used")
                            {
                                GPURam = sensor.Value.GetValueOrDefault();
                            }
                            if (sensor.SensorType == SensorType.Temperature)
                            {
                                GPUTemp = sensor.Value.GetValueOrDefault();
                            }
                        }
                    }

                    if (hardware.HardwareType == HardwareType.GpuAti)
                    {
                        hardware.Update();
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Load)
                            {
                                GPULoad = sensor.Value.GetValueOrDefault();
                            }
                            if (sensor.SensorType == SensorType.SmallData && sensor.Name == "GPU Memory Used")
                            {
                                GPURam = sensor.Value.GetValueOrDefault();
                            }
                            if (sensor.SensorType == SensorType.Temperature)
                            {
                                GPUTemp = sensor.Value.GetValueOrDefault();
                            }
                        }
                    }

                    if (hardware.HardwareType == HardwareType.RAM)
                    {
                        hardware.Update();
                        foreach (var sensor in hardware.Sensors)
                            if (sensor.SensorType == SensorType.Data && sensor.Name == "Used Memory")
                            {
                                RAMUsed = sensor.Value.GetValueOrDefault();
                            }
                    }
                }

                foreach (NetworkInterface inf in interfaces)
                {
                    if (inf.OperationalStatus == OperationalStatus.Up &&
                        inf.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        inf.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                        inf.NetworkInterfaceType != NetworkInterfaceType.Unknown && !inf.IsReceiveOnly)
                    {
                        byteRec = inf.GetIPv4Statistics().BytesReceived;
                        byteSent = inf.GetIPv4Statistics().BytesSent;
                    }
                }
                x = byteRec - oldRec;
                y = byteSent - oldSent;
                oldRec = byteRec;
                oldSent = byteSent;
                string strNw = (x * 8 / 1048576).ToString("F2") + "/" + (y * 8 / 1048576).ToString("F2");
                if(strNw.Length > 10)
                {
                    strNw = "Connecting...";
                }

                txtNw.Text = strNw;
                txtCPULoad.Text = ((int)CPULoad).ToString();
                txtCPUTemp.Text = ((int)CPUTemp).ToString();
                txtGPULoad.Text = ((int)GPULoad).ToString();
                txtGPURam.Text = ((int)GPURam).ToString();
                txtRAM.Text = ((int)(RAMUsed * 1024)).ToString();
                txtGPUTemp.Text = ((int)GPUTemp).ToString();
                a = txtCPUTemp.Text + "," + txtCPULoad.Text + "," + txtGPULoad.Text + "," + txtGPURam.Text + "," + txtRAM.Text + "," + txtNw.Text + "*";
                serialPort1.Write(a);

                //MessageBox.Show(oldRec.ToString() + " - " + oldSent.ToString());
                
            }
            catch(Exception ex)
            {
                //Error("Lỗi");
                //MessageBox.Show(ex.ToString());
            }
        }

        private void btnConn_Click(object sender, EventArgs e)
        {
            try
            {
                txtCmd.Text = "";
                if (!serialPort1.IsOpen)
                {
                    serialPort1.PortName = cbbCom.Text;
                    serialPort1.Open();
                    btnConn.Text = "Disconnect";
                    cbbCom.BackColor = Color.Blue;
                }
                else
                {
                    btnConn.Text = "Connect";
                    cbbCom.BackColor = Color.Red;
                    serialPort1.Close();
                    Error("LCD module stopped!");
                }
            }
            catch
            {
                Error("Error!");
            }
        }

        void Error(string str)
        {
            txtCmd.Text = str;
            btnConn.Text = "Connect";
            cbbCom.BackColor = Color.Red;
            serialPort1.Close();
        }

        private void btlHelp_Click(object sender, EventArgs e)
        {
            frmabout frm = new frmabout();
            frm.ShowDialog();
        }

        private void btnComReLoad_Click(object sender, EventArgs e)
        {
            cbbCom.DataSource = SerialPort.GetPortNames();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                _config.AppSettings.Settings["timer"].Value = txtTimer.Text;
                _config.AppSettings.Settings["com"].Value = cbbCom.Text;
                _config.Save();
                ConfigurationManager.RefreshSection("appSettings");
                MessageBox.Show("Saved!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Can not save config!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            _show();
        }

        private void txtCmd_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                try
                {
                    timer1.Stop();
                    serialPort1.Write(txtCmd.Text);
                    txtCmd.Text = "";
                    timer1.Start();
                }
                catch
                {
                    Error("Unknown command!");
                }
            }
        }

        private void fmrMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Normal)
            {
                this.Hide();
                _showBalloon("Minimize to tray!",2000);
            }
            else
            {
                //this.Visible = true;
                _show();
            }
        }
        public void _show()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
            this.Focus();
            this.ShowInTaskbar = true;
        }
    }
}
