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
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            mgh.excludeUser = textBox.Text;
            Close();
        }
    }
}
