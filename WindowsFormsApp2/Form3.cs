using Baidu.Aip.Face;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form3 : Form//白名单设置窗口
    {
        string[] whitelist = new string[20];
        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;

        public Form3(string[] input)
        {
            InitializeComponent();
            whitelist = input;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 f1 = (Form1)this.Owner;
            int k = 0;
            for(int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    whitelist[k] = (i+1).ToString();
                    k++;
                }
            }
            if (k < 20)
            {
                for (; k < 20; k++)
                {
                    whitelist[k] = "-1";
                }
            }
            string ans = "";
            for(int j = 0; j < 20; j++)
            {
                ans = ans + whitelist[j] + "$";
            }
            f1.Controls["label7"].Text = ans;

        }

        private void Form3_Shown(object sender, EventArgs e)
        {
            var client = new Face("IUZlio98xUOVPcoLmHYQEhGI", "66u5WWXcYF0WwIbFlCKGwfaUTAqiHF7E");         
            
            try
            {
              
                    var result = client.GroupGetusers("1");

                   // Thread.Sleep(600);
                    //var result = client.UserAdd(bit64, "BASE64", "1", textBox1.Text, options);
                    if (result.HasValues && result["error_msg"].ToString().Contains("SUCCESS")) {
                    int num =(int) result["result"]["user_id_list"].LongCount();
                    int i = 0;
                    for (; i < num; i++) {
                        var re = client.UserGet(result["result"]["user_id_list"][i].ToString(),"1");
                        checkedListBox1.Items.Add(re["result"]["user_list"][0]["user_info"].ToString());
                    }
                    //MessageBox.Show(result["result"]["user_id_list"].LongCount().ToString());
                    for (int k = 0; k < checkedListBox1.Items.Count; k++)//载入当前白名单
                    {
                        if(whitelist.Contains((k+1).ToString()))
                            checkedListBox1.SetItemChecked(k,true);
                        else
                            checkedListBox1.SetItemChecked(k, false);
                    }

                }
                    else { MessageBox.Show("查询失败：" + result["error_msg"]); }
            }
            catch (Exception ex)
            {
                    MessageBox.Show("查询失败：" + ex.ToString());
                    throw;
            }          
                          
        }

        private void button1_Click(object sender, EventArgs e)//全选
        {
            for(int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void button2_Click(object sender, EventArgs e)//全不选
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
        }

        private void button3_Click(object sender, EventArgs e)//反选
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, !checkedListBox1.GetItemChecked(i));
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form3_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }
    }
}
  