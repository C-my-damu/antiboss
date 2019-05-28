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
    public partial class Form2 : Form
    {
        static Bitmap now ;
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
        }
    }
}
