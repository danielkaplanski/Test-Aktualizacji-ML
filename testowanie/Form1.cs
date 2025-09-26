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
            string backupDir = Path.Combine(Path.GetTempPath(), "backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            try
            {
                updateBtn.Enabled = false;
                lblUpdateInfo.Text = "Pobieranie aktualizacji...";

                // 1. Pobierz now� wersj�
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

                // 3. Znajd� plik wykonywalny w nowej wersji
                var exeFiles = Directory.GetFiles(extractPath, "*.exe", SearchOption.AllDirectories);
                if (exeFiles.Length == 0)
                {
                    MessageBox.Show("Nie znaleziono pliku .exe w paczce aktualizacji.");
                    return;
                }

                lblUpdateInfo.Text = "Tworzenie kopii zapasowej...";

                // 4. Utw�rz kopi� zapasow� obecnej wersji
                if (Directory.Exists(backupDir))
                    Directory.Delete(backupDir, true);

                Directory.CreateDirectory(backupDir);

                // Skopiuj wszystkie pliki obecnej wersji do backupu
                foreach (string file in Directory.GetFiles(currentDir))
                {
                    string fileName = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(backupDir, fileName), true);
                }

                lblUpdateInfo.Text = "Zast�powanie plik�w...";

                // 5. Zast�p pliki now� wersj�
                foreach (string file in Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories))
                {
                    string relativePath = file.Substring(extractPath.Length + 1);
                    string targetPath = Path.Combine(currentDir, relativePath);

                    string targetDir = Path.GetDirectoryName(targetPath);
                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);

                    File.Copy(file, targetPath, true);
                }

                lblUpdateInfo.Text = "Uruchamianie nowej wersji...";

                // 6. Uruchom now� wersj� z ORYGINALNEJ lokalizacji
                string newExePath = Path.Combine(currentDir, Path.GetFileName(exeFiles[0]));

                Process.Start(newExePath);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("B��d aktualizacji: " + ex.Message);

                // Przywr�� backup w przypadku b��du
                try
                {
                    if (Directory.Exists(backupDir))
                    {
                        foreach (string file in Directory.GetFiles(backupDir))
                        {
                            string fileName = Path.GetFileName(file);
                            string targetPath = Path.Combine(currentDir, fileName);
                            File.Copy(file, targetPath, true);
                        }
                    }
                }
                catch (Exception restoreEx)
                {
                    MessageBox.Show("B��d przywracania kopii zapasowej: " + restoreEx.Message);
                }

                updateBtn.Enabled = true;
                lblUpdateInfo.Text = "B��d aktualizacji";
            }
            finally
            {
                // Sprz�tanie
                try
                {
                    if (File.Exists(tempZipPath))
                        File.Delete(tempZipPath);
                    if (Directory.Exists(extractPath))
                        Directory.Delete(extractPath, true);
                }
                catch { }
            }
        }
    }
}