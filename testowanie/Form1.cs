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
            // 1. Pobierz lokaln¹ wersjê aplikacji
            currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Brak wersji";
            lblCurrentVersion.Text = $"Wersja: {currentVersion}";

            // 2. Pobierz zdaln¹ wersjê
            try
            {
                remoteVersion = await GetRemoteVersionAsync();
                lblUpdateInfo.Text = $"Nowa wersja: {remoteVersion}";

                // 3. Porównaj wersje
                if (remoteVersion != currentVersion)
                {
                    lblUpdateInfo.Text += " (Dostêpna nowa wersja!)";
                    updateBtn.Enabled = true; // np. w³¹cz przycisk aktualizacji
                }
                else
                {
                    lblUpdateInfo.Text += " (Aktualna)";
                }
            }
            catch (Exception ex)
            {
                lblUpdateInfo.Text = $"B³¹d sprawdzania wersji: {ex.Message}";
            }
        }

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

        // (Opcjonalnie) Obs³uga przycisku aktualizacji
        private void updateBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Tu mo¿esz uruchomiæ aktualizator, pobraæ instalator, itd.");
            // Np. Process.Start("https://link-do-instalki.exe");
        }
    }
}
