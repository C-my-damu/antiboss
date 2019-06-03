using AForge.Video.DirectShow;
using Baidu.Aip.Face;
using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace WindowsFormsApp2
{

    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        string filep = "";
        private int Indexof = 0;
        bool flag = false;
        bool drawable = false;
        bool isrun = false;
        int t = 0;
        double zoom = 1;//摄像头分辨率/窗口控件分辨率
        int selectface = 0;
        bool flag_cam = false;//相机标签
        string selectedprocess = "";
        int ttimer2 = 0;//timer2已过秒数
        int ttime2 = 0;//timer2设定描述
        Rectangle[] rectangles = new Rectangle[10];
        String[] whitelist = new string[20];//白名单
        bool inWhitelist = false;
        Dictionary<int,string> face_id=new Dictionary<int, string>() ;
        Dictionary<string, int> processlist = new Dictionary<string, int>();

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]        
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;

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
                button2.Enabled = true;
                button3.Enabled = true;
                startCam();
            }

            else
            {
                button1.BackColor = Color.Red;
                button2.Enabled = false;
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
            //设置下像素，自动寻找分辨率最高的选项并且按比例设置预览框大小：
            int maxres = 0;
            int maxindex = 0;
            for(int i=0;i< videoSource.VideoCapabilities.Count(); i++)
            {
                if (videoSource.VideoCapabilities[i].FrameSize.Width > maxres)
                {
                    maxres = videoSource.VideoCapabilities[i].FrameSize.Width;
                    maxindex = i;
                }
            }

            videoSource.VideoResolution = videoSource.VideoCapabilities[maxindex];
            //在videoSourcePlayer1里显示
            zoom = (double)(((double)videoSourcePlayer1.Height) / videoSource.VideoCapabilities[maxindex].FrameSize.Height);
            videoSourcePlayer1.Width =(int)(videoSource.VideoCapabilities[maxindex].FrameSize.Width * zoom);
            videoSourcePlayer1.VideoSource = videoSource;
            try
            {
                videoSourcePlayer1.Start();
            }
            catch (Exception e)
            {

                System.Console.WriteLine(e.ToString());
            }
           
        }
        private void closeCam()//关闭摄像头
        {
            Indexof = Cameralist.SelectedIndex;
            videoSourcePlayer1.Visible = false;
            videoSource.Stop();
            videoSourcePlayer1.Stop();

        }

        private void Form1_Shown(object sender, EventArgs e)//载入时初始化
        {
            Camlist();
            comboBox3.SelectedIndex = 0;
            comboBox2.SelectedIndex = 4;

            comboBox1.Items.Clear();
            processlist.Clear();
            Process[] list = Process.GetProcesses();
            foreach (var item in list)
            {
                if (item.MainWindowTitle != ""&&!comboBox1.Items.Contains(item.MainWindowTitle))
                {
                    try
                    {
                        comboBox1.Items.Add(item.MainWindowTitle);
                        processlist.Add(item.MainWindowTitle, item.Id);
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex.ToString());
                    }
                }                
            }
            comboBox1.SelectedIndex = 0;
            //载入时清空白名单
            whitelist[0] = "1";
            for(int k = 1; k < 20; k++)
            {
                whitelist[k] = "-1";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
            if (timer1.Enabled) timer1.Start();
            notifyIcon1.Text = "当前状态：" + timer1.Enabled.ToString()+"\n自动切换：" + checkBox1.Checked.ToString();
            flag = false;
            if (button2.Text == "开始检测(&x)")
                button2.Text = ("停止检测(&x)");
            else
                button2.Text = ("开始检测(&x)");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(null!= pictureBox1.Image)
            using (Form form = new Form2(new Bitmap(pictureBox1.Image)))
            {
                form.ShowDialog();
              
            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(videoSourcePlayer1.IsRunning)
            videoSourcePlayer1.Stop();
        }
        void flashimage(Bitmap bitmap,Rectangle temp)
        {
            try
            {
                pictureBox1.Image = null;
                Bitmap unknowbit = new Bitmap(temp.Width, temp.Height);
                Graphics graphics = Graphics.FromImage(unknowbit);
                graphics.DrawImage(bitmap, 0, 0, temp, GraphicsUnit.Pixel);
                pictureBox1.Image = (Bitmap)unknowbit.Clone();
            }
            catch (Exception)
            {

                
            }
           
               
            
        }
         void  run()
         {
            isrun = true;
            for (int n = 0; t < 10; t++)
            {
                rectangles[n].X = 0;
                rectangles[n].Y = 0;
                rectangles[n].Width = 0;
                rectangles[n].Height = 0;
            }


            Bitmap bitmap = videoSourcePlayer1.GetCurrentVideoFrame();
            var client = new Face("IUZlio98xUOVPcoLmHYQEhGI", "66u5WWXcYF0WwIbFlCKGwfaUTAqiHF7E");
            int face_num = 0;
            var useful_num = 0;
            string bit64 = ImgToBase64String(bitmap);
            var options = new Dictionary<string, object> {
                { "max_face_num",10},
            //    { "quality_control","NORMAL"},
            //    { "action_type","replace"}
            };
            try
            {
                var result = client.Detect(bit64, "BASE64", options);

                if (result["error_msg"].ToString().Equals("SUCCESS"))
                {
                    string num = "";
                    num = result["result"]["face_num"].ToString();
                    //label1.Text = num;
                    face_num = Convert.ToInt32(num);
                    useful_num = 0;
                    if (Convert.ToInt32(num) >= 1)
                    {
                        for (int i = 0; i < face_num; i++)
                        {
                            var face1 = result["result"]["face_list"][i];
                            if (Convert.ToInt32(face1["face_probability"]) > 0.9 && Convert.ToInt32(face1["location"]["width"]) > 30 
                                && Convert.ToInt32(face1["location"]["height"]) > 30)
                            {
                                double x = Convert.ToInt32(face1["location"]["left"]);
                                double y = Convert.ToInt32(face1["location"]["top"]);
                                double w = Convert.ToInt32(face1["location"]["width"]);
                                double h = Convert.ToInt32(face1["location"]["height"]);
                                rectangles[i].X = (int)((Convert.ToInt32(face1["location"]["left"])) *zoom);
                                rectangles[i].Y = (int)((Convert.ToInt32(face1["location"]["top"])) *zoom);
                                rectangles[i].Width =(int)((Convert.ToInt32(face1["location"]["width"])) *zoom);
                                rectangles[i].Height = (int)((Convert.ToInt32(face1["location"]["height"])) *zoom);
                                Rectangle temp = new Rectangle(0, 0, 0, 0);
                                temp.X = (int)(x - w / 1.75);
                                temp.Y = (int)(y - h / 1.75);
                                temp.Width = (int)(w * 2);
                                temp.Height = (int)(h * 2);
                                useful_num++;
                                drawable = true;
                                Bitmap unknowbit = new Bitmap(temp.Width, temp.Height);
                                Graphics graphics = Graphics.FromImage(unknowbit);
                                graphics.DrawImage(bitmap, 0, 0, temp, GraphicsUnit.Pixel);
                               
                                   
                                try
                                {
                                    var face_result = client.Search(ImgToBase64String(unknowbit), "BASE64", "1");
                                    unknowbit.Dispose();
                                    label2.BeginInvoke(new Action(() => { label2.Text = face_result["error_msg"].ToString(); }));
                                    if (Convert.ToString(face_result["error_msg"]) == "SUCCESS")
                                    {
                                        face_id.Remove(i);
                                        face_id.Add(i, Convert.ToString(face_result["result"]["user_list"][0]["user_id"]) + "-"
                                            + Convert.ToString(face_result["result"]["user_list"][0]["user_info"]) + "-"
                                            + Convert.ToString(face_result["result"]["user_list"][0]["score"]));
                                        if (Convert.ToInt32(face_result["result"]["user_list"][0]["score"]) <= 90)
                                        {
                                            face_id.Remove(i);
                                            face_id.Add(i,"unknown-" + Convert.ToString(face_result["result"]["user_list"][0]["score"]));
                                        }
                                        
                                    }
                                    else
                                    {
                                        if (Convert.ToString(face_result["error_msg"]) == "match user is not found")
                                        {
                                            face_id.Remove(i);
                                            face_id.Add(i, "unknown");
                                        }
                                    }
                                }
                                catch (Exception)
                                {

                                    //throw;
                                }
                              
                                if (selectface == i)
                                    try
                                    {
                                        pictureBox1.BeginInvoke(new Action(() => { flashimage(bitmap,temp); }));
                                    }
                                    catch (Exception)
                                    {
                                        //do nothing
                                    }
                            }

                        }
                        for (int i = face_num; i < 10; i++)
                        {
                            rectangles[i].X = 0;
                            rectangles[i].Y = 0;
                            rectangles[i].Width = 0;
                            rectangles[i].Height = 0;
                        }
                    }
                    try
                    {
                        label1.BeginInvoke(new Action(() => { label1.Text = useful_num.ToString(); }));
                        t++;
                    }
                    catch (Exception)
                    {

                        
                    }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加失败：" + ex.ToString());
                throw;
            }
            if (!flag && checkBox1.Checked)
            {
                if (inWhitelist)//如果白名单模式启动，则启动筛选
                {
                    int i = 0;
                    for(; i < useful_num; i++)
                    {
                        int j = 0;
                        for(; j < 20; j++)
                        {
                            if (face_id[i].StartsWith(whitelist[j]))
                            {
                                break;
                            }
                        }
                        if (j == 20)
                        {
                            //白名单外
                            break;
                        }
                        
                    }
                    if (i < useful_num)
                    {
                        //白名单检测失败
                        RunProcess();
                       
                    }
                }
                else//一般模式
                {       if(useful_num > 1)
                        
                        RunProcess();
                }
            }
            
            isrun = false;
         }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!isrun)
            {
                //if(pictureBox1.Image!=null)
                //pictureBox1.Image.Dispose();
                Thread t0 = new Thread(run);
                t0.Start();
            }         
        }
        public static string ImgToBase64String(Bitmap bmp)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //threeebase64编码的字符串转为图片
        public static Bitmap Base64StringToImage(string strbase64)
        {
            try
            {
                byte[] arr = Convert.FromBase64String(strbase64);
                MemoryStream ms = new MemoryStream(arr);
                Bitmap bmp = new Bitmap(ms);
                ms.Close();
                return bmp;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            processlist.Clear();
            Process[] list = Process.GetProcesses();
            foreach(var item in list)
            {
                if (item.MainWindowTitle != "") {
                    try
                    {
                        comboBox1.Items.Add(item.MainWindowTitle);
                        processlist.Add(item.MainWindowTitle, item.Id);
                    }
                    catch (Exception)
                    {

                       
                    }
                   
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        //private void button4_Click_1(object sender, EventArgs e)
        //{
        //    OpenFileDialog dialog = new OpenFileDialog();

        //    dialog.Filter = "程序(*.exe)|*.exe";
        //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        filep = dialog.FileName;
        //    }

        //    comboBox1.Items.Clear();
        //    Process[] list = Process.GetProcesses();
        //    foreach (var item in list)
        //    {
        //        comboBox1.Items.Add(item.ProcessName);
        //    }
        //}

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (ttimer2 < ttime2)
            {
                ttimer2 += 1000;
                System.Console.WriteLine("2tick");
            }
            else
            {
                ttimer2 = 0;
                timer1.Enabled = true;
                timer1.Start();
                System.Console.WriteLine("1:" + timer1.Enabled.ToString());
                timer2.Enabled = false;
                notifyIcon1.BalloonTipText = "重启人脸检测";
                notifyIcon1.ShowBalloonTip(2000);
                notifyIcon1.Text = "当前状态：" + timer1.Enabled.ToString() + "\n自动切换：" + checkBox1.Checked.ToString();
            }
        }

        private void comboBox2_DropDownClosed(object sender, EventArgs e)
        {
            switch (comboBox2.SelectedIndex)
            {
                case 0: {
                        ttime2 = 30000;
                        break; }
                case 1: {
                        ttime2 = 60000;
                        break; }
                case 2: {
                        ttime2 = 300000;
                        break; }
                case 3: {
                        ttime2 = 600000;
                        break; }
                case 4: {
                        ttime2 = 10;
                        break; }

            }
        }

        private void videoSourcePlayer1_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.Graphics g = e.Graphics;
            System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Brushes.OrangeRed, 1); // 创建一个红色，宽度为1的画笔
            var c = 3;
            if (drawable)
            {               
                for(int i=0; i<10;i++)
                    if (!rectangles[i].IsEmpty&&rectangles[i].Width>0&&rectangles[i].Height>0)
                    {
                        Font font = new Font("微软雅黑", 10);
                        if(face_id.Count>=i+1)
                        g.DrawString (face_id[i],font, new SolidBrush(Color.Red), rectangles[i].X,rectangles[i].Y);
                        g.DrawRectangle(pen, rectangles[i]);
                    }               
            }           
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            notifyIcon1.Text = "当前状态：" + timer1.Enabled.ToString() + "\n自动切换：" + checkBox1.Checked.ToString();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            button2.PerformClick();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void comboBox3_DropDownClosed(object sender, EventArgs e)
        {
            selectface = comboBox3.SelectedIndex;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            inWhitelist = !inWhitelist;
        }
        private void RunProcess()//拉起选中的窗口
        {
            try
            {
                if (selectedprocess != "" && !selectedprocess.Equals(null))
                {
                    bool t = false;

                    Process[] temp = Process.GetProcessesByName(selectedprocess);//在所有已启动的进程中查找需要的进程；
                    if (processlist.ContainsKey(selectedprocess))
                        foreach (var pro in Process.GetProcesses())
                        {
                            if (pro.Id == processlist[selectedprocess])//如果查找到
                            {
                                IntPtr handle = pro.MainWindowHandle;
                                if (pro.MainWindowTitle != "")
                                {
                                    try
                                    {
                                        SwitchToThisWindow(handle, true);    // 激活，显示在最前
                                        t = true;
                                        button2.BeginInvoke(new Action(() => { button2.Text = ("开始检测(&x)"); }));
                                        notifyIcon1.Text = "当前状态：" + timer1.Enabled.ToString() + "\n自动切换：" + checkBox1.Checked.ToString();
                                    }
                                    catch (Exception)
                                    {

                                        throw;
                                    }
                                    
                                }
                            }
                        }
                    if (!t)
                    {
                        MessageBox.Show("未选择有效程序或进程！" );
                    }
                }
                else
                {
                    MessageBox.Show("未选择有效程序或进程！" );
                }


                flag = true;
                //timer2.Enabled = true;
                //timer2.Start();
                this.Invoke(new Action(() => { timer1.Stop(); timer1.Enabled = false; }));
                //timer1.Stop();
                //timer1.Enabled = false;
                System.Console.WriteLine("1:" + timer1.Enabled.ToString());
                notifyIcon1.Text = "当前状态：" + timer1.Enabled.ToString() + "\n自动切换：" + checkBox1.Checked.ToString();
            }
            catch (Exception e0)
            {
                flag = true;
                timer1.Enabled = false;
                notifyIcon1.Text = "当前状态：" + timer1.Enabled.ToString() + "\n自动切换：" + checkBox1.Checked.ToString();
                //                   throw e0;
                MessageBox.Show("未选择有效程序或进程！" + e0.Message);

            }
            label1.BeginInvoke(new Action(() => { label1.Text = "1"; }));
            if (ttime2 > 100)
            {
                this.Invoke(new Action(() => { timer2.Enabled = true; timer2.Start(); }));
                timer2.Enabled = true;
                System.Console.WriteLine("2:" + timer2.Enabled.ToString());
            }
        
        }

        private void button4_Click_2(object sender, EventArgs e)
        {
            Form form = new Form3(whitelist);
            
            form.ShowDialog(this);            

        }

        private void label7_TextChanged(object sender, EventArgs e)
        {
            string[] temp = label7.Text.Split('$');
            MessageBox.Show("白名单更新！");
            for(int i = 0; i < 20; i++)
            {
                whitelist[i] = temp[i];
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() != "" && !comboBox1.SelectedItem.Equals(null))
                selectedprocess = comboBox1.SelectedItem.ToString();
            else
                selectedprocess = "";
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }
    }
}
