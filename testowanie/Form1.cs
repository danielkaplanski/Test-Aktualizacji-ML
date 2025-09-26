using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
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

            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

            // Podziel wersjê na czêœci u¿ywaj¹c '+' jako separatora i weŸ tylko pierwsz¹ czêœæ
            string displayVersion = version.Split('+')[0];
            currentVersion = displayVersion;
            lblCurrentVersion.Text = $"Wersja: {displayVersion}";

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
            string batFile = Path.Combine(Path.GetTempPath(), "update.bat");
            if (File.Exists(batFile))
            {
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

                using (var reader = new StreamReader(stream, new UTF8Encoding(false)))
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
            string exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

            try
            {
                updateBtn.Enabled = false;
                lblUpdateInfo.Text = "Pobieranie...";

                using (HttpClient client = new HttpClient())
                {
                    var data = await client.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync(tempZipPath, data);
                }

                lblUpdateInfo.Text = "Rozpakowywanie...";

                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);

                ZipFile.ExtractToDirectory(tempZipPath, extractPath);

                // ZnajdŸ nowy exe
                var exeFiles = Directory.GetFiles(extractPath, "*.exe", SearchOption.AllDirectories);
                if (exeFiles.Length == 0)
                {
                    MessageBox.Show("Brak pliku exe w aktualizacji.");
                    updateBtn.Enabled = true;
                    return;
                }

                string newExePath = exeFiles[0];
                string targetExePath = Path.Combine(currentDir, exeName);

                // Tworzymy plik BAT
                string batFile = Path.Combine(Path.GetTempPath(), "update.bat");
                string batContent = $@"
@echo off
timeout /t 1 /nobreak >nul
xcopy /Y /E ""{extractPath}\*"" ""{currentDir}\""
start """" ""{targetExePath}""
del ""%~f0""
";
                await File.WriteAllTextAsync(batFile, batContent, Encoding.Default);

                lblUpdateInfo.Text = "Uruchamianie nowej wersji...";

                Process.Start(new ProcessStartInfo
                {
                    FileName = batFile,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"B³¹d: {ex.Message}");
                updateBtn.Enabled = true;
                lblUpdateInfo.Text = "B³¹d";
            }
        }
    }
}
