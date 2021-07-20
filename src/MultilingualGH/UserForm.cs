using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultilingualGH
{
    public partial class UserForm : Form
    {
        public UserForm()
        {
            InitializeComponent();
            textBox1.Text = MGH.ExcludeUser;
            FormClosing += (s, e) => MGH.ExcludeUser = textBox1.Text;
        }

        private void UserForm_Load(object sender, EventArgs e)
        {

        }
    }
}
