using System;
using System.Windows.Forms;

namespace MultilingualGH
{
    public partial class UserForm : Form
    {
        readonly MultilingualInstance mgh;
        public UserForm()
        {
            InitializeComponent();
            MultilingualInstance.documents.TryGetValue(Grasshopper.Instances.ActiveCanvas.Document.DocumentID, out mgh);
            textBox.Text = mgh.excludeUser;
            FormClosing += SubmitButton_Click;
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            FormClosing -= SubmitButton_Click;
            mgh.excludeUser = textBox.Text;
            MultilingualInstance.EventHandler(Grasshopper.Instances.ActiveCanvas, mgh);
            Close();
        }
    }
}
