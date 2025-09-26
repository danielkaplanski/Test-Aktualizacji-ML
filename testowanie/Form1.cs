using System;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testowanie
{
    public partial class Form1 : Form
    {
        private string remoteVersion;
        private string currentVersion;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            string localVersion = Application.ProductVersion;
            lblCurrentVersion.Text = $"Twoja wersja: {localVersion}";

            string remoteVersion = await GetLatestVersionAsync();

            if (remoteVersion != null)
            {
                lblUpdateInfo.Text = $"Dostêpna wersja: {remoteVersion}";

                if (IsNewerVersion(remoteVersion, localVersion))
                {
                    updateBtn.Visible = true;
                }
                else
                {
                    updateBtn.Visible = false;
                }
            }
            else
            {
                lblUpdateInfo.Text = "Nie uda³o siê sprawdziæ wersji.";
                updateBtn.Visible = false;
            }
        }

        //
        // 4. Pobierz wersjê z pliku version.txt z Netlify
        private async Task<string> GetRemoteVersionAsync()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var url = "https://gilded-sorbet-913b659.netlify.app/version.txt";

                var response = await httpClient.GetAsync(url);
                var stream = await response.Content.ReadAsStreamAsync();

                using (var reader = new StreamReader(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
                {
                    string version = await reader.ReadToEndAsync();

                    // Usuñ niewidoczne znaki BOM + koñcówki linii
                    return version.Trim().Trim('\uFEFF');
                }
            }
        }
        private async Task<string> GetLatestVersionAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var bytes = await client.GetByteArrayAsync("https://gilded-sorbet-913b659.netlify.app/version.txt");
                    string version = Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF').Trim();
                    return version;
                }
                catch
                {
                    return null;
                }
            }
        }
        private bool IsNewerVersion(string remoteVersion, string localVersion)
        {
            try
            {
                Version remote = new Version(remoteVersion);
                Version local = new Version(localVersion);
                return remote > local;
            }
            catch
            {
                return false;
            }
        }

        // (Opcjonalnie) Obs³uga przycisku aktualizacji
        private void updateBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Tu mo¿esz uruchomiæ aktualizator, pobraæ instalator, itd.");
            // Np. Process.Start("https://link-do-instalki.exe");
        }
    }
}
