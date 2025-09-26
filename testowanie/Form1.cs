using System.Diagnostics;
using System.Reflection;

namespace testowanie
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            lblCurrentVersion.Text = $"Wersja:";
        }
        private void updateBtn_Click(object sender, EventArgs e)
        {
            
        }

        


    }
}
