using System;
using System.Diagnostics;
using System.IO.Compression;
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
            // 1. Pobierz lokaln� wersj� aplikacji
            currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Brak wersji";
            lblCurrentVersion.Text = $"Wersja: {currentVersion}";

            // 2. Pobierz zdaln� wersj�
            try
            {
                remoteVersion = await GetRemoteVersionAsync();
                lblUpdateInfo.Text = $"Nowa wersja: {remoteVersion}";

                // 3. Por�wnaj wersje
                if (remoteVersion != currentVersion)
                {
                    lblUpdateInfo.Text += " (Dost�pna nowa wersja!)";
                    updateBtn.Enabled = true; // np. w��cz przycisk aktualizacji
                }
                else
                {
                    lblUpdateInfo.Text += " (Aktualna)";
                }
            }
            catch (Exception ex)
            {
                lblUpdateInfo.Text = $"B��d sprawdzania wersji: {ex.Message}";
            }
        }
        // 4. Pobierz wersj� z pliku version.txt z Netlify
        private async Task<string> GetRemoteVersionAsync()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var url = "https://majestic-axolotl-224ec3.netlify.app/version.txt";

                var response = await httpClient.GetAsync(url);
                var stream = await response.Content.ReadAsStreamAsync();

                using (var reader = new StreamReader(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
                {
                    string version = await reader.ReadToEndAsync();

                    // Usu� niewidoczne znaki BOM + ko�c�wki linii
                    return version.Trim().Trim('\uFEFF');
                }
            }
        }


        // (Opcjonalnie) Obs�uga przycisku aktualizacji
        private async void updateBtn_Click(object sender, EventArgs e)
        {
            string url = "https://majestic-axolotl-224ec3.netlify.app/publish.zip"; // link do zipa
            string tempZipPath = Path.Combine(Path.GetTempPath(), "update.zip");
            string extractPath = Path.Combine(Path.GetTempPath(), "update");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var data = await client.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync(tempZipPath, data);
                }

                // Usu� folder z poprzedniej aktualizacji je�li istnieje
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);

                // Rozpakuj
                ZipFile.ExtractToDirectory(tempZipPath, extractPath);
                var files = Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories);
                string fileList = string.Join("\n", files);
                MessageBox.Show("Zawarto�� rozpakowanego folderu:\n" + fileList);

                var exeFiles = Directory.GetFiles(extractPath, "*.exe", SearchOption.AllDirectories);
                if (exeFiles.Length == 0)
                {
                    MessageBox.Show("Nie znaleziono pliku .exe w paczce aktualizacji.");
                    return;
                }
                string newExePath = exeFiles[0];

                // Uruchom now� wersj�
                Process.Start(newExePath);
                Application.Exit();

            }
            catch (Exception ex)
            {
                MessageBox.Show("B��d aktualizacji: " + ex.Message);
            }
        }

    }
}
