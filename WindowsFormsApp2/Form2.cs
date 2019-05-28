using Baidu.Aip.Face;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form2 : Form//添加删除人脸数据窗口
    {
        static Bitmap now ;

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;

        Dictionary<string, string> userlist = new Dictionary<string, string>();

        public Form2(Bitmap bitmap)
        {
            InitializeComponent();
            now =(Bitmap) bitmap.Clone();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            pictureBox1.Image = now;
            var client = new Face("IUZlio98xUOVPcoLmHYQEhGI", "66u5WWXcYF0WwIbFlCKGwfaUTAqiHF7E");

            try
            {

                var result = client.GroupGetusers("1");

                Thread.Sleep(600);
                //var result = client.UserAdd(bit64, "BASE64", "1", textBox1.Text, options);
                if (result.HasValues && result["error_msg"].ToString().Contains("SUCCESS"))
                {
                    int num = (int)result["result"]["user_id_list"].LongCount();
                    int i = 0;
                    for (; i < num; i++)
                    {
                        var re = client.UserGet(result["result"]["user_id_list"][i].ToString(), "1");
                        userlist.Add(re["result"]["user_list"][0]["user_info"].ToString(),result["result"]["user_id_list"][i].ToString());
                        comboBox1.Items.Add(re["result"]["user_list"][0]["user_info"].ToString());
                        
                    }
                    //MessageBox.Show(result["result"]["user_id_list"].LongCount().ToString());
                    comboBox1.Items.Add("新增");

                }
                else { MessageBox.Show("查询失败：" + result["error_msg"]); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("查询失败：" + ex.ToString());
                //throw;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var client = new Face("IUZlio98xUOVPcoLmHYQEhGI", "66u5WWXcYF0WwIbFlCKGwfaUTAqiHF7E");
            now.Save("temp.jpg");
            //textBox2.Text = comboBox1.SelectedItem.ToString().Split('-')[1];
            string bit64 = ImgToBase64String(now);
            var options = new Dictionary<string, object> {
                { "user_info",textBox2.Text},
                { "quality_control","NORMAL"}//,
               // { "action_type","replace"}
            };
            string user_id = "";
            if (comboBox1.SelectedItem.Equals("新增"))
            {
                //do something
                int i = 0;
                for (; i < 20; i++)
                {
                    if (!userlist.ContainsValue((i+1).ToString()))
                    {
                        break;
                    }
                }
                user_id = (i+1).ToString();
            }
            else
            {
                user_id = userlist[comboBox1.SelectedItem.ToString()];
            }
            try
            {
                var result = client.UserAdd(bit64, "BASE64", "1", user_id, options);

                //Thread.Sleep(600);
                //var result = client.UserAdd(bit64, "BASE64", "1", textBox1.Text, options);
                if (result.HasValues&& result["error_msg"].ToString().Contains("SUCCESS")) { MessageBox.Show("添加成功："+result["error_msg"]); }
                else { MessageBox.Show("添加失败：" + result["error_msg"]); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加失败："+ex.ToString());
                throw;
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

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!comboBox1.SelectedItem.ToString().Equals("新增"))
            {
                var client = new Face("IUZlio98xUOVPcoLmHYQEhGI", "66u5WWXcYF0WwIbFlCKGwfaUTAqiHF7E");
                var result = client.UserDelete("1",userlist[comboBox1.SelectedItem.ToString()]);
            }
            else
            {
                MessageBox.Show("请从列表中选择！");
            }

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox2.Text = comboBox1.SelectedItem.ToString();
            if (comboBox1.SelectedItem.ToString().Equals("新增"))
            {
                textBox2.ReadOnly = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }
    }
}
