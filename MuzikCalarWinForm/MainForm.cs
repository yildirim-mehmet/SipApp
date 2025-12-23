using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Runtime;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MuzikCalarWinForm
{




    public partial class MainForm : Form
    {
        private HttpClient _httpClient;
        private System.Timers.Timer _sureTimer;
        private SoundPlayer _soundPlayer;
        private bool _caliyor = false;
        private DateTime _sarkiBaslangic;
        private int _toplamSure = 0;
        private string _apiBaseUrl = "https://localhost:7231/api"; // SipApp API URL
        private List<CalmaListesiItem> _mevcutListe = new List<CalmaListesiItem>();
        private CalmaListesiItem _suAnCalanItem = null;
        private AppSettings _settings;

        public MainForm()
        {
            InitializeComponent();
            InitializeHttpClient();
            InitializeTimers();
            LoadSettings();
            _settings = AppSettings.Load();
            this.FormClosing += MainForm_FormClosing;
        }

        private void InitializeHttpClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MuzikCalarWinForm");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        private void InitializeTimers()
        {
            // Listeyi yenileme timer'ı
            timerYenile.Tick += async (s, e) => await SiraListesiniYenile();
            timerYenile.Interval = (int)numYenilemeAraligi.Value * 1000;
            timerYenile.Start();

            // Şarkı süresi timer'ı
            _sureTimer = new System.Timers.Timer(1000);
            _sureTimer.Elapsed += (s, e) => UpdateSureGosterge();
        }

        private void LoadSettings()
        {
            // Ses seviyesi ayarı
            cbSesSeviyesi.SelectedIndexChanged += (s, e) =>
            {
                if (cbSesSeviyesi.SelectedItem != null)
                {
                    string sesSeviye = cbSesSeviyesi.SelectedItem.ToString().Replace("%", "");
                    if (int.TryParse(sesSeviye, out int seviye))
                    {
                        // Ses seviyesi ayarı burada yapılabilir
                        // Windows ses API'si kullanılabilir
                    }
                }
            };

            // Yenileme aralığı ayarı
            numYenilemeAraligi.ValueChanged += (s, e) =>
            {
                timerYenile.Interval = (int)numYenilemeAraligi.Value * 1000;
            };
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            // İlk yüklemede listeyi getir
            await SiraListesiniYenile();

            // Otomatik başlatma opsiyonu
            if (_settings.OtoBaslat)
            {
                btnBaslaDurdur_Click(null, null);
            }
        }

        private async Task SiraListesiniYenile()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/CalmaListesi/kuyruk");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var kuyruk = JsonConvert.DeserializeObject<List<CalmaListesiItem>>(json);

                    Invoke(new Action(() => UpdateListView(kuyruk)));
                }
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                    lblSarkiBilgisi.Text = $"Hata: {ex.Message.Substring(0, Math.Min(50, ex.Message.Length))}..."));
            }
        }

        private void UpdateListView(List<CalmaListesiItem> kuyruk)
        {
            _mevcutListe = kuyruk;
            listViewSira.Items.Clear();

            int siraNo = 1;
            foreach (var item in kuyruk.Where(x => !x.Calindi).OrderByDescending(x => x.SiraDegeri).ThenBy(x => x.Id))
            {
                var listItem = new ListViewItem(siraNo.ToString());
                listItem.SubItems.Add(item.SarkiAdi);
                listItem.SubItems.Add(item.MasaAdi);
                listItem.SubItems.Add(item.SiraDegeri.ToString());
                listItem.SubItems.Add(item.Calindi ? "Çalındı" : "Bekliyor");

                // Renk kodlama
                if (item.SiraDegeri > 1)
                {
                    listItem.BackColor = Color.LightGreen; // Ödemeli şarkı
                }
                else if (item.Id == _suAnCalanItem?.Id)
                {
                    listItem.BackColor = Color.LightBlue; // Şu an çalan
                }

                listViewSira.Items.Add(listItem);
                siraNo++;
            }
        }

        private async void btnBaslaDurdur_Click(object sender, EventArgs e)
        {
            if (!_caliyor)
            {
                await SiradakiSarkiyiCal();
                btnBaslaDurdur.Text = "⏸ Duraklat";
                btnBaslaDurdur.BackColor = Color.FromArgb(46, 204, 113);
            }
            else
            {
                // Duraklat
                _soundPlayer?.Stop();
                _sureTimer.Stop();
                btnBaslaDurdur.Text = "▶️ Devam Et";
                btnBaslaDurdur.BackColor = Color.FromArgb(241, 196, 15);
            }
            _caliyor = !_caliyor;
        }

        private async Task SiradakiSarkiyiCal()
        {
            try
            {
                // API'den sıradaki şarkıyı belirle
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/CalmaListesi/siradaki");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _suAnCalanItem = JsonConvert.DeserializeObject<CalmaListesiItem>(json);

                    if (_suAnCalanItem != null && !string.IsNullOrEmpty(_suAnCalanItem.DosyaYolu))
                    {
                        // Şarkıyı çal
                        if (File.Exists(_suAnCalanItem.DosyaYolu))
                        {
                            _soundPlayer = new SoundPlayer(_suAnCalanItem.DosyaYolu);
                            _soundPlayer.Play();

                            // Bilgileri güncelle
                            Invoke(new Action(() =>
                            {
                                lblSarkiBilgisi.Text = $"{_suAnCalanItem.SarkiAdi} - {_suAnCalanItem.MasaAdi}";
                                progressSarki.Value = 0;
                                _sarkiBaslangic = DateTime.Now;
                                _sureTimer.Start();
                            }));

                            // Çalındı olarak işaretle
                            await MarkAsPlayed(_suAnCalanItem.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Çalma hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task MarkAsPlayed(int calmaListesiId)
        {
            try
            {
                await _httpClient.PostAsync($"{_apiBaseUrl}/CalmaListesi/{calmaListesiId}/calindi", null);
            }
            catch { /* Logla */ }
        }

        private void UpdateSureGosterge()
        {
            if (_caliyor && _suAnCalanItem != null)
            {
                Invoke(new Action(() =>
                {
                    var gecenSure = (DateTime.Now - _sarkiBaslangic).TotalSeconds;
                    var yuzde = (int)((gecenSure / _suAnCalanItem.Sure) * 100);
                    progressSarki.Value = Math.Min(100, yuzde);

                    // Süre gösterimi
                    lblSure.Text = $"{TimeSpan.FromSeconds(gecenSure):mm\\:ss} / {TimeSpan.FromSeconds(_suAnCalanItem.Sure):mm\\:ss}";
                }));
            }
        }

        private async void btnSonraki_Click(object sender, EventArgs e)
        {
            _soundPlayer?.Stop();
            _sureTimer.Stop();
            await SiradakiSarkiyiCal();
        }

        private async void btnOnceki_Click(object sender, EventArgs e)
        {
            // Önceki şarkıyı bul (basit implementasyon)
            if (_mevcutListe.Any() && _suAnCalanItem != null)
            {
                var index = _mevcutListe.FindIndex(x => x.Id == _suAnCalanItem.Id);
                if (index > 0)
                {
                    _suAnCalanItem = _mevcutListe[index - 1];
                    await PlaySelectedItem();
                }
            }
        }

        private async Task PlaySelectedItem()
        {
            if (_suAnCalanItem != null)
            {
                _soundPlayer?.Stop();
                _sureTimer.Stop();

                if (File.Exists(_suAnCalanItem.DosyaYolu))
                {
                    _soundPlayer = new SoundPlayer(_suAnCalanItem.DosyaYolu);
                    _soundPlayer.Play();

                    Invoke(new Action(() =>
                    {
                        lblSarkiBilgisi.Text = $"{_suAnCalanItem.SarkiAdi} - {_suAnCalanItem.MasaAdi}";
                        progressSarki.Value = 0;
                        _sarkiBaslangic = DateTime.Now;
                        _sureTimer.Start();
                        _caliyor = true;
                        btnBaslaDurdur.Text = "⏸ Duraklat";
                    }));

                    await MarkAsPlayed(_suAnCalanItem.Id);
                }
            }
        }

        private async void btnYenile_Click(object sender, EventArgs e)
        {
            await SiraListesiniYenile();
        }

        private void btnDurdur_Click(object sender, EventArgs e)
        {
            _soundPlayer?.Stop();
            _sureTimer.Stop();
            _caliyor = false;
            btnBaslaDurdur.Text = "▶️ Çalmaya Başla";
            progressSarki.Value = 0;
            lblSure.Text = "00:00 / 00:00";
        }

        private void btnSarkiEkle_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Müzik Dosyaları|*.mp3;*.wav;*.wma;*.m4a|Tüm Dosyalar|*.*";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // Seçilen şarkıları API'ye ekle
                    foreach (var file in ofd.FileNames)
                    {
                        // API'ye ekleme işlemi
                        // Bu kısım admin yetkisi gerektirir
                    }
                }
            }
        }

        private async void btnListeTemizle_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Tüm sırayı temizlemek istediğinize emin misiniz?",
                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    await _httpClient.DeleteAsync($"{_apiBaseUrl}/CalmaListesi/temizle");
                    await SiraListesiniYenile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Temizleme hatası: {ex.Message}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnKapat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _soundPlayer?.Stop();
            _sureTimer?.Stop();
            timerYenile.Stop();
            _httpClient?.Dispose();
        }

        // Data modelleri
        public class CalmaListesiItem
        {
            public int Id { get; set; }
            public int SarkiId { get; set; }
            public string SarkiAdi { get; set; }
            public string DosyaYolu { get; set; }
            public int Sure { get; set; }
            public int MasaId { get; set; }
            public string MasaAdi { get; set; }
            public int SiraDegeri { get; set; }
            public bool Calindi { get; set; }
            public decimal OdemeMiktari { get; set; }
            public DateTime EklemeZamani { get; set; }
        }
    }



    public class AppSettings
    {
        public bool OtoBaslat { get; set; } = false;
        public int YenilemeAraligi { get; set; } = 10;
        public int SesSeviyesi { get; set; } = 100;
        public string SonCalinanSarki { get; set; } = "";
        public string APIBaseUrl { get; set; } = "https://localhost:7231/api";

        public static AppSettings Load()
        {
            return new AppSettings();
        }
    }
}