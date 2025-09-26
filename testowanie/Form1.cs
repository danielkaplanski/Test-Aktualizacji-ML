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
            // SprawdŸ czy to restart po aktualizacji
            CheckForUpdateRestart();

            currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Brak wersji";
            lblCurrentVersion.Text = $"Wersja: {currentVersion}";

            try
            {
                remoteVersion = await GetRemoteVersionAsync();
                lblUpdateInfo.Text = $"Nowa wersja: {remoteVersion}";

                if (remoteVersion != currentVersion)
                {
                    lblUpdateInfo.Text += " (Dostêpna nowa wersja!)";
                    updateBtn.Enabled = true;
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

        private void CheckForUpdateRestart()
        {
            // SprawdŸ czy istnieje plik bat z aktualizacji
            string batFile = Path.Combine(Path.GetTempPath(), "update.bat");
            if (File.Exists(batFile))
            {
                // Poczekaj chwilê i usuñ plik bat
                Task.Delay(1000).ContinueWith(t => {
                    try { File.Delete(batFile); } catch { }
                });

                MessageBox.Show("Aktualizacja zakoñczona pomyœlnie! Obecna wersja: " +
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
            if (MessageBox.Show("Czy na pewno chcesz zaktualizowaæ aplikacjê?", "Potwierdzenie",
                MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            string url = "https://danielkaplanski.github.io/Test-Aktualizacji-ML/publish.zip";
            string tempZipPath = Path.Combine(Path.GetTempPath(), "update.zip");
            string extractPath = Path.Combine(Path.GetTempPath(), "update");
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string currentExe = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

            try
            {
                updateBtn.Enabled = false;
                lblUpdateInfo.Text = "Pobieranie aktualizacji...";

                // 1. Pobierz now¹ wersjê
                using (HttpClient client = new HttpClient())
                {
                    var data = await client.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync(tempZipPath, data);
                }

                lblUpdateInfo.Text = "Rozpakowywanie...";

                // 2. Rozpakuj do folderu tymczasowego
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);

                ZipFile.ExtractToDirectory(tempZipPath, extractPath);

                // 3. SprawdŸ czy s¹ pliki do aktualizacji
                var newFiles = Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories);
                if (newFiles.Length == 0)
                {
                    MessageBox.Show("Brak plików w paczce aktualizacji.");
                    return;
                }

                lblUpdateInfo.Text = "Przygotowywanie aktualizacji...";

                // 4. Stwórz batch file który wykona aktualizacjê po zamkniêciu aplikacji
                string batPath = Path.Combine(Path.GetTempPath(), "update.bat");
                string batContent = $@"
@echo off
chcp 65001 > nul
echo Czekam na zamkniêcie aplikacji...
timeout /t 2 /nobreak > nul

echo Kopiowanie nowych plików...
xcopy /y /s ""{extractPath}"" ""{currentDir}""

echo Uruchamianie nowej wersji...
cd /d ""{currentDir}""
start """" ""{currentExe}""

echo Usuwanie plików tymczasowych...
rd /s /q ""{extractPath}""
del ""{tempZipPath}""
del ""%~f0""

exit
";

                await File.WriteAllTextAsync(batPath, batContent, Encoding.GetEncoding(852));

                // 5. Uruchom batch file
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{batPath}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                });

                // 6. Zamknij aplikacjê
                Application.Exit();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"B³¹d aktualizacji: {ex.Message}");
                updateBtn.Enabled = true;
                lblUpdateInfo.Text = "B³¹d aktualizacji";

                // Sprz¹tanie w przypadku b³êdu
                try
                {
                    if (File.Exists(tempZipPath)) File.Delete(tempZipPath);
                    if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
                }
                catch { }
            }
        }
    }
}