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
            lblCurrentVersion.Text = $"Wersja: {Application.ProductVersion}";
            await CheckForUpdateAsync();
        }
        private void updateBtn_Click(object sender, EventArgs e)
        {
            string newAppPath = @"E:\github\testowanie\new_version\testowanie.exe"; // zmie� "App.exe" na swoj� nazw�

            if (File.Exists(newAppPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = newAppPath,
                    UseShellExecute = true
                });

                Application.Exit(); // zamknij star� wersj�
            }
            else
            {
                MessageBox.Show("Nie znaleziono nowej wersji.");
            }
        }

        private async Task CheckForUpdateAsync()
        {
            try
            {
                // Plik version.txt w tym samym folderze co EXE
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.txt");

                if (!File.Exists(path))
                {
                    lblUpdateInfo.Text = "Brak pliku version.txt.";
                    return;
                }

                string remoteVersionString = await File.ReadAllTextAsync(path);
                remoteVersionString = remoteVersionString.Trim();

                Version remoteVersion = new Version(remoteVersionString);
                Version localVersion = new Version(Application.ProductVersion);

                if (remoteVersion > localVersion)
                {
                    lblUpdateInfo.Text = $"Dost�pna nowa wersja: {remoteVersion}";
                    updateBtn.Visible = true;
                    updateBtn.Enabled = true;
                }
                else
                {
                    lblUpdateInfo.Text = "Masz najnowsz� wersj�.";
                    updateBtn.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblUpdateInfo.Text = "B��d sprawdzania wersji.";
            }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
