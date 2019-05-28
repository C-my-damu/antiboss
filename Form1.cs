using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private int Indexof = 0;
        bool flag_cam = false;//相机标签

        public Form1()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            flag_cam = !flag_cam;
         
            if (flag_cam)
            {
                button1.BackColor = Color.Blue;
                button3.Enabled = true;
                startCam();
            }

            else
            {
                button1.BackColor = Color.Red;
                button3.Enabled = false;
                closeCam();
            }
        }


        private void Camlist()//获取摄像头列表
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("未找到摄像头设备");
            }
            foreach (FilterInfo device in videoDevices)
            {
                Cameralist.Items.Add(device.Name);
                Cameralist.SelectedIndex = 0;
            }
        }
        private void startCam()
        {//开启摄像头
            Indexof = Cameralist.SelectedIndex;
            if (Indexof < 0)
            {
                MessageBox.Show("请选择一个摄像头");
                return;
            }
            //pictureBox1.Visible = false;
            videoSourcePlayer1.Visible = true;
            //videoDevices[Indexof]确定出用哪个摄像头了。
            videoSource = new VideoCaptureDevice(videoDevices[Indexof].MonikerString);
            //设置下像素，这句话不写也可以正常运行：
            videoSource.VideoResolution = videoSource.VideoCapabilities[Indexof];
            //在videoSourcePlayer1里显示
            videoSourcePlayer1.VideoSource = videoSource;
            videoSourcePlayer1.Start();

        }
        private void closeCam()//关闭摄像头
        {
            Indexof = Cameralist.SelectedIndex;
            videoSourcePlayer1.Visible = false;
            videoSource.Stop();
            videoSourcePlayer1.Stop();

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Camlist();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form form = new Form2(videoSourcePlayer1.GetCurrentVideoFrame());
            form.Show();
        }
    }
}
