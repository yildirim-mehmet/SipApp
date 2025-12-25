using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MuzikCalarWinForm
{
    public partial class MainForm : Form
    {
        private HttpClient _httpClient;
        private System.Timers.Timer _sureTimer;
        private System.Timers.Timer _otomatikCalmaTimer;
        private SoundPlayer _soundPlayer;
        private bool _caliyor = false;
        private bool _otomatikCalmaAktif = false;
        private DateTime _sarkiBaslangic;
        private int _toplamSure = 0;
        private string _apiBaseUrl = "https://localhost:7286/api/muzik"; // SipApp API URL
        private List<CalmaListesiDto> _mevcutListe = new List<CalmaListesiDto>();
        private CalmaListesiDto _suAnCalanItem = null;
        private List<SarkiDto> _apiSarkilari = new List<SarkiDto>();
        private AppSettings _settings;

        // Yeni butonlar için field'lar
        private Button btnApiSarkilariCek;
        private Button btnOtomatikBaslat;
        private CheckBox chkOtomatikDevam;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            InitializeHttpClient();
            InitializeTimers();
            LoadSettings();
            _settings = AppSettings.Load();
            this.FormClosing += MainForm_FormClosing;
        }

        private void InitializeCustomComponents()
        {
            // API'den Şarkıları Çek Butonu
            btnApiSarkilariCek = new Button
            {
                BackColor = Color.FromArgb(155, 89, 182),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 9F),
                ForeColor = Color.White,
                Location = new Point(280, 95),
                Size = new Size(140, 30),
                Text = "📡 API'den Şarkıları Çek",
                UseVisualStyleBackColor = false
            };
            btnApiSarkilariCek.Click += btnApiSarkilariCek_Click;
            panelControls.Controls.Add(btnApiSarkilariCek);

            // Otomatik Çalma Butonu
            btnOtomatikBaslat = new Button
            {
                BackColor = Color.FromArgb(46, 204, 113),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 9F),
                ForeColor = Color.White,
                Location = new Point(430, 95),
                Size = new Size(140, 30),
                Text = "▶️ Otomatik Çalma",
                UseVisualStyleBackColor = false
            };
            btnOtomatikBaslat.Click += btnOtomatikBaslat_Click;
            panelControls.Controls.Add(btnOtomatikBaslat);

            // Otomatik Devam CheckBox
            chkOtomatikDevam = new CheckBox
            {
                AutoSize = true,
                Checked = true,
                Font = new Font("Microsoft Sans Serif", 8F),
                Location = new Point(580, 102),
                Size = new Size(140, 17),
                Text = "Otomatik devam et (loop)"
            };
            panelControls.Controls.Add(chkOtomatikDevam);
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

            // Otomatik çalma kontrol timer'ı
            _otomatikCalmaTimer = new System.Timers.Timer(5000); // 5 saniyede bir kontrol
            _otomatikCalmaTimer.Elapsed += async (s, e) => await OtomatikCalmaKontrol();
            _otomatikCalmaTimer.AutoReset = true;
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
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/kuyruk");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var kuyruk = JsonConvert.DeserializeObject<List<CalmaListesiDto>>(json);

                    Invoke(new Action(() => UpdateListView(kuyruk)));
                }
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                    lblSarkiBilgisi.Text = $"Hata: {ex.Message.Substring(0, Math.Min(50, ex.Message.Length))}..."));
            }
        }

        private void UpdateListView(List<CalmaListesiDto> kuyruk)
        {
            _mevcutListe = kuyruk;
            listViewSira.Items.Clear();

            int siraNo = 1;
            foreach (var item in kuyruk.Where(x => !x.calindi).OrderByDescending(x => x.siraDegeri).ThenBy(x => x.id))
            {
                var listItem = new ListViewItem(siraNo.ToString());
                listItem.SubItems.Add(item.sarkiAdi);
                listItem.SubItems.Add(item.masaAdi);
                listItem.SubItems.Add(item.siraDegeri.ToString());
                listItem.SubItems.Add(item.calindi ? "Çalındı" : "Bekliyor");

                // Renk kodlama
                if (item.siraDegeri > 1)
                {
                    listItem.BackColor = Color.LightGreen; // Ödemeli şarkı
                }
                else if (item.id == _suAnCalanItem?.id)
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
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/siradaki");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _suAnCalanItem = JsonConvert.DeserializeObject<CalmaListesiDto>(json);

                    if (_suAnCalanItem != null && !string.IsNullOrEmpty(_suAnCalanItem.dosyaYolu))
                    {
                        // Şarkıyı çal
                        if (File.Exists(_suAnCalanItem.dosyaYolu))
                        {
                            _soundPlayer = new SoundPlayer(_suAnCalanItem.dosyaYolu);
                            _soundPlayer.Play();

                            // Bilgileri güncelle
                            Invoke(new Action(() =>
                            {
                                lblSarkiBilgisi.Text = $"{_suAnCalanItem.sarkiAdi} - {_suAnCalanItem.masaAdi}";
                                progressSarki.Value = 0;
                                _sarkiBaslangic = DateTime.Now;
                                _sureTimer.Start();
                            }));

                            // Çalındı olarak işaretle
                            await MarkAsPlayed(_suAnCalanItem.id);
                        }
                        else
                        {
                            MessageBox.Show($"Dosya bulunamadı: {_suAnCalanItem.dosyaYolu}",
                                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Çalma hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task MarkAsPlayed(int calmaListesiId)
        {
            try
            {
                await _httpClient.PostAsync($"{_apiBaseUrl}/{calmaListesiId}/calindi", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Çalındı işaretleme hatası: {ex.Message}");
            }
        }

        private void UpdateSureGosterge()
        {
            if (_caliyor && _suAnCalanItem != null)
            {
                Invoke(new Action(() =>
                {
                    var gecenSure = (DateTime.Now - _sarkiBaslangic).TotalSeconds;
                    var yuzde = (int)((gecenSure / _suAnCalanItem.sure) * 100);
                    progressSarki.Value = Math.Min(100, yuzde);

                    // Süre gösterimi
                    lblSure.Text = $"{TimeSpan.FromSeconds(gecenSure):mm\\:ss} / {TimeSpan.FromSeconds(_suAnCalanItem.sure):mm\\:ss}";
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
            if (_mevcutListe.Any() && _suAnCalanItem != null)
            {
                var index = _mevcutListe.FindIndex(x => x.id == _suAnCalanItem.id);
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

                if (File.Exists(_suAnCalanItem.dosyaYolu))
                {
                    _soundPlayer = new SoundPlayer(_suAnCalanItem.dosyaYolu);
                    _soundPlayer.Play();

                    Invoke(new Action(() =>
                    {
                        lblSarkiBilgisi.Text = $"{_suAnCalanItem.sarkiAdi} - {_suAnCalanItem.masaAdi}";
                        progressSarki.Value = 0;
                        _sarkiBaslangic = DateTime.Now;
                        _sureTimer.Start();
                        _caliyor = true;
                        btnBaslaDurdur.Text = "⏸ Duraklat";
                    }));

                    await MarkAsPlayed(_suAnCalanItem.id);
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
                    // Bu kısım admin yetkisi gerektirir
                    MessageBox.Show("Bu özellik için admin yetkisi gereklidir.",
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    // Yeni endpoint: Muzik/clear
                    var response = await _httpClient.PostAsync($"{_apiBaseUrl}/clear", null);
                    if (response.IsSuccessStatusCode)
                    {
                        await SiraListesiniYenile();
                        MessageBox.Show("Sıra temizlendi.", "Başarılı",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
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

        // YENİ EKLENEN METOTLAR

        // API'den şarkıları çek
        private async Task<List<SarkiDto>> ApiSarkilariCek()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/sarkilar");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<SarkiDto>>(json)
                        ?? new List<SarkiDto>();
                }
                return new List<SarkiDto>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API bağlantı hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<SarkiDto>();
            }
        }

        // API şarkılarını çalma listesine ekle
        private async Task<bool> ApiSarkilariniCalmaListesineEkle()
        {
            try
            {
                // 1. API'den şarkıları çek
                _apiSarkilari = await ApiSarkilariCek();

                if (!_apiSarkilari.Any())
                {
                    MessageBox.Show("API'den aktif şarkı bulunamadı.", "Bilgi",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                // 2. Her şarkı için kontrol et ve ekle
                int adminMasaId = 1; // Varsayılan admin masa ID
                int eklenenSayi = 0;

                foreach (var sarki in _apiSarkilari)
                {
                    // Şarkı zaten sırada mı kontrol et
                    var zatenVar = await CalmaListesindeVarMi(sarki.id);

                    if (!zatenVar)
                    {
                        // CalmaListesi'ne ekle
                        var request = new
                        {
                            sarkiId = sarki.id,
                            masaId = adminMasaId
                        };

                        var json = JsonConvert.SerializeObject(request);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await _httpClient.PostAsync($"{_apiBaseUrl}/ekle", content);

                        if (response.IsSuccessStatusCode)
                        {
                            eklenenSayi++;
                        }
                    }
                }

                // 3. Listeyi yenile
                await SiraListesiniYenile();

                MessageBox.Show($"{eklenenSayi} şarkı çalma listesine eklendi.",
                    "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return eklenenSayi > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Şarkı ekleme hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Şarkı çalma listesinde var mı kontrol
        private async Task<bool> CalmaListesindeVarMi(int sarkiId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/kuyruk");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var kuyruk = JsonConvert.DeserializeObject<List<CalmaListesiDto>>(json);
                    return kuyruk?.Any(k => k.sarkiId == sarkiId && !k.calindi) ?? false;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Otomatik çalma kontrolü
        private async Task OtomatikCalmaKontrol()
        {
            if (!_otomatikCalmaAktif || _caliyor) return;

            try
            {
                // Sırayı kontrol et
                var kuyruk = await GetKuyrukAsync();

                if (!kuyruk.Any(k => !k.calindi))
                {
                    // Sıra boşsa, API şarkılarını ekle
                    if (_apiSarkilari.Any() || await ApiSarkilariniCalmaListesineEkle())
                    {
                        await Task.Delay(1000);
                        await SiradakiSarkiyiCal();
                    }
                }
                else
                {
                    // Sırada şarkı varsa, çal
                    await SiradakiSarkiyiCal();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Otomatik çalma hatası: {ex.Message}");
            }
        }

        // Kuyruğu getir (yardımcı metot)
        private async Task<List<CalmaListesiDto>> GetKuyrukAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/kuyruk");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<CalmaListesiDto>>(json)
                        ?? new List<CalmaListesiDto>();
                }
                return new List<CalmaListesiDto>();
            }
            catch
            {
                return new List<CalmaListesiDto>();
            }
        }

        // Otomatik çalma başlat
        private void BaslatOtomatikCalma()
        {
            _otomatikCalmaAktif = true;
            _otomatikCalmaTimer.Start();
            btnOtomatikBaslat.Text = "⏸️ Otomatik Durdur";
            btnOtomatikBaslat.BackColor = Color.FromArgb(231, 76, 60);

            MessageBox.Show("Otomatik çalma başlatıldı. Sistem otomatik olarak şarkıları çalacak.",
                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Otomatik çalma durdur
        private void DurdurOtomatikCalma()
        {
            _otomatikCalmaAktif = false;
            _otomatikCalmaTimer.Stop();
            btnOtomatikBaslat.Text = "▶️ Otomatik Çalma";
            btnOtomatikBaslat.BackColor = Color.FromArgb(46, 204, 113);
        }

        // YENİ BUTON EVENT HANDLER'LARI

        private async void btnApiSarkilariCek_Click(object sender, EventArgs e)
        {
            try
            {
                btnApiSarkilariCek.Enabled = false;
                btnApiSarkilariCek.Text = "⏳ Çekiliyor...";

                var basarili = await ApiSarkilariniCalmaListesineEkle();

                if (basarili && !_caliyor)
                {
                    // Hemen çalmaya başla
                    await SiradakiSarkiyiCal();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İşlem hatası: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnApiSarkilariCek.Enabled = true;
                btnApiSarkilariCek.Text = "📡 API'den Şarkıları Çek";
            }
        }

        private void btnOtomatikBaslat_Click(object sender, EventArgs e)
        {
            if (!_otomatikCalmaAktif)
            {
                var result = MessageBox.Show("Otomatik çalma modu başlatılsın mı?\n\n" +
                                           "✓ API'den şarkıları otomatik çekecek\n" +
                                           "✓ Boş kaldığında yeni şarkılar ekleyecek\n" +
                                           "✓ Sürekli olarak şarkıları çalacak",
                                           "Otomatik Çalma",
                                           MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    BaslatOtomatikCalma();
                }
            }
            else
            {
                DurdurOtomatikCalma();
            }
        }

        // Media player şarkı bittiğinde
        private async void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            // Otomatik çalma aktifse, sıradakini çal
            if (_otomatikCalmaAktif && chkOtomatikDevam.Checked)
            {
                await Task.Delay(1000); // 1 saniye bekle
                await SiradakiSarkiyiCal();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _soundPlayer?.Stop();
            _sureTimer?.Stop();
            _otomatikCalmaTimer?.Stop();
            timerYenile.Stop();
            _httpClient?.Dispose();
        }

        // Data modelleri - YENİ DTO'LAR
        public class CalmaListesiDto
        {
            public int id { get; set; }
            public int sarkiId { get; set; }
            public string sarkiAdi { get; set; }
            public string dosyaYolu { get; set; }
            public int sure { get; set; }
            public int masaId { get; set; }
            public string masaAdi { get; set; }
            public int siraDegeri { get; set; }
            public bool calindi { get; set; }
            public decimal odemeMiktari { get; set; }
            public DateTime eklenmeZamani { get; set; }
        }

        public class SarkiDto
        {
            public int id { get; set; }
            public string ad { get; set; }
            public string dosyaYolu { get; set; }
            public int sure { get; set; }
            public bool aktif { get; set; }
            public DateTime eklenmeTarihi { get; set; }
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