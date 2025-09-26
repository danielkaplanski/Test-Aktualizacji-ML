using System;
using System.Diagnostics;
using System.IO;
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
            // Sprawd� czy to restart po aktualizacji
            CheckForUpdateRestart();

            currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Brak wersji";
            lblCurrentVersion.Text = $"Wersja: {currentVersion}";

            try
            {
                remoteVersion = await GetRemoteVersionAsync();
                lblUpdateInfo.Text = $"Nowa wersja: {remoteVersion}";

                if (remoteVersion != currentVersion)
                {
                    lblUpdateInfo.Text += " (Dost�pna nowa wersja!)";
                    updateBtn.Enabled = true;
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

        private void CheckForUpdateRestart()
        {
            // Sprawd� czy istnieje plik bat z aktualizacji
            string batFile = Path.Combine(Path.GetTempPath(), "update.bat");
            if (File.Exists(batFile))
            {
                // Poczekaj chwil� i usu� plik bat
                Task.Delay(1000).ContinueWith(t => {
                    try { File.Delete(batFile); } catch { }
                });

                MessageBox.Show("Aktualizacja zako�czona pomy�lnie! Obecna wersja: " +
                    Assembly.GetExecutingAssembly().GetName().Version?.ToString());
            }
        }

        private async Task<string> GetRemoteVersionAsync()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var url = "https://danielkaplanski.github.io/Test-Aktualizacji-ML/version.txt";
                var response = await httpClient.GetAsync(url);
                var stream = await response.Content.ReadAsStreamAsync();

                using (var reader = new StreamReader(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
                {
                    string version = await reader.ReadToEndAsync();
                    return version.Trim().Trim('\uFEFF');
                }
            }
        }

        private async void updateBtn_Click(object sender, EventArgs e)
        {
            string url = "https://danielkaplanski.github.io/Test-Aktualizacji-ML/publish.zip";
            string tempZipPath = Path.Combine(Path.GetTempPath(), "update.zip");
            string extractPath = Path.Combine(Path.GetTempPath(), "update");
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            try
            {
                updateBtn.Enabled = false;
                lblUpdateInfo.Text = "Pobieranie...";

                // Pobierz
                using (HttpClient client = new HttpClient())
                {
                    var data = await client.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync(tempZipPath, data);
                }

                lblUpdateInfo.Text = "Rozpakowywanie...";

                // Rozpakuj
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);

                ZipFile.ExtractToDirectory(tempZipPath, extractPath);

                // Znajd� nowy exe
                var exeFiles = Directory.GetFiles(extractPath, "*.exe", SearchOption.AllDirectories);
                if (exeFiles.Length == 0)
                {
                    MessageBox.Show("Brak pliku exe w aktualizacji.");
                    return;
                }

                string newExePath = exeFiles[0];
                string newExeName = Path.GetFileName(newExePath);
                string targetExePath = Path.Combine(currentDir, newExeName);

                lblUpdateInfo.Text = "Uruchamianie nowej wersji...";

                // Uruchom now� wersj� (bez zast�powania plik�w)
                Process.Start(newExePath);

                MessageBox.Show("Nowa wersja zosta�a uruchomiona. Mo�esz zamkn�� star� wersj�.");

                // Opcjonalnie: zamknij star� wersj�
                // Application.Exit();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"B��d: {ex.Message}");
                updateBtn.Enabled = true;
                lblUpdateInfo.Text = "B��d";
            }
        }
    }
}