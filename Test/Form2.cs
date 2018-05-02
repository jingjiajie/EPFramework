using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FrontWork;

namespace Test
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public string[] RoleValues()
        {
            return new string[] { "管理员", "VIP", "普通用户" };
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.basicView1.GetViewComponent("name").Color = Color.Red;
        }
    }
}
