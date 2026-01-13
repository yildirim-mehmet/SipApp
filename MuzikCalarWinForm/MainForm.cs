<<<<<<< HEAD
﻿using Newtonsoft.Json;
using NAudio.Wave;
=======
﻿using NAudio.Wave;
using Newtonsoft.Json;
>>>>>>> ac88e2837235ac296494cdec610e8b6a79add180
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net.Http;
using System.Text;


using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace MuzikCalarWinForm
{
    public partial class MainForm : Form
    {
        private HttpClient _httpClient;
        private System.Timers.Timer _sureTimer;
        private System.Timers.Timer _otomatikCalmaTimer;

        // NAudio (MP3/WAV)
        private WaveOutEvent _outputDevice;
        private AudioFileReader _audioFile;

        private volatile bool _stopRequestedByUser = false; // kullanıcı stop/next/prev yaptı mı?

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

<<<<<<< HEAD
=======
        private readonly HttpClient _http = new HttpClient();


>>>>>>> ac88e2837235ac296494cdec610e8b6a79add180
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
            // İSTENEN: timer ile değil, şarkı bitince yenilenecek => Start ETMİYORUZ.
            timerYenile.Tick += async (s, e) => await SiraListesiniYenile();
            timerYenile.Interval = (int)numYenilemeAraligi.Value * 1000;
            // timerYenile.Start();  // <-- KAPALI

            // Şarkı süresi timer'ı
            _sureTimer = new System.Timers.Timer(1000);
            _sureTimer.Elapsed += (s, e) => UpdateSureGosterge();

            // Otomatik çalma kontrol timer'ı (bunu koruyoruz)
            _otomatikCalmaTimer = new System.Timers.Timer(5000); // 5 saniyede bir kontrol
            _otomatikCalmaTimer.Elapsed += async (s, e) => await OtomatikCalmaKontrol();
            _otomatikCalmaTimer.AutoReset = true;
        }

        private void LoadSettings()
        {
            // Ses seviyesi ayarı (mevcut yapı korunuyor, istersen NAudio volume ile bağlarız)
            cbSesSeviyesi.SelectedIndexChanged += (s, e) =>
            {
                if (cbSesSeviyesi.SelectedItem != null)
                {
                    string sesSeviye = cbSesSeviyesi.SelectedItem.ToString().Replace("%", "");
                    if (int.TryParse(sesSeviye, out int seviye))
                    {
                        // NAudio volume istenirse:
                        // if (_audioFile != null) _audioFile.Volume = Math.Clamp(seviye / 100f, 0f, 1f);
                    }
                }
            };

            // Yenileme aralığı ayarı (timer start edilmiyor ama manuel yenilemede interval yine güncel kalsın)
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
                    var kuyruk = JsonConvert.DeserializeObject<List<CalmaListesiDto>>(json) ?? new List<CalmaListesiDto>();

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
            _mevcutListe = kuyruk ?? new List<CalmaListesiDto>();
            listViewSira.Items.Clear();

            int siraNo = 1;

            // İSTENEN ÖNCELİK: calindi=0, siraDegeri DESC, id ASC
            foreach (var item in _mevcutListe
                .Where(x => !x.calindi)
                .OrderByDescending(x => x.siraDegeri)
                .ThenBy(x => x.id))
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

        // -------------------------
        // NAudio yardımcıları
        // -------------------------
        private void StopPlayback(bool userRequested)
        {
            _stopRequestedByUser = userRequested;

            try { _outputDevice?.Stop(); } catch { /* ignore */ }

            try { _audioFile?.Dispose(); _audioFile = null; } catch { /* ignore */ }
            try { _outputDevice?.Dispose(); _outputDevice = null; } catch { /* ignore */ }

            _sureTimer.Stop();
        }

        private void StartPlayback(string filePath)
        {
            // önceki çalma varsa kapat (userRequested=false => doğal bitiş akışını bozmayalım)
            StopPlayback(userRequested: false);

            _audioFile = new AudioFileReader(filePath);
            _outputDevice = new WaveOutEvent();
            _outputDevice.Init(_audioFile);

            _outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;

            _stopRequestedByUser = false;
            _outputDevice.Play();
        }

        // Şarkı doğal bittiğinde çalışacak
        private async void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            // Hata ile durduysa
            if (e.Exception != null)
            {
                Invoke(new Action(() =>
                {
                    lblSarkiBilgisi.Text = $"Çalma hatası: {e.Exception.Message}";
                }));
                _caliyor = false;
                return;
            }

            // Kullanıcı stop/next/prev yaptıysa otomatik akışı tetikleme
            if (_stopRequestedByUser)
                return;

            // Doğal bitiş mi?
            bool naturalEnd = false;
            try
            {
                if (_audioFile != null && _audioFile.Position >= _audioFile.Length)
                    naturalEnd = true;
            }
            catch { /* ignore */ }

            if (!naturalEnd)
                return;

            // 1) BİTTİ -> DB’de calindi=1 yap
            if (_suAnCalanItem != null)
            {
                await MarkAsPlayed(_suAnCalanItem.id);
            }

            // 2) Listeyi şarkı bitince yenile (timer yok!)
            await SiraListesiniYenile();

            // 3) Otomatik mod açıksa sıradakini çal
            if (_otomatikCalmaAktif && chkOtomatikDevam.Checked)
            {
                await Task.Delay(300);
                await SiradakiSarkiyiCal();
            }
            else
            {
                _caliyor = false;
                Invoke(new Action(() =>
                {
                    btnBaslaDurdur.Text = "▶️ Çalmaya Başla";
                    btnBaslaDurdur.BackColor = Color.FromArgb(52, 152, 219);
                }));
            }
        }

        // -------------------------
        // UI Eventleri
        // -------------------------

        private async void btnBaslaDurdur_Click(object sender, EventArgs e)
        {
            if (!_caliyor)
            {
                // Eğer pause'dan dönüyorsak aynı şarkıyı devam ettir
                if (_outputDevice != null && _audioFile != null &&
                    _outputDevice.PlaybackState == PlaybackState.Paused)
                {
                    _stopRequestedByUser = false;
                    _outputDevice.Play();
                    _sureTimer.Start();

                    btnBaslaDurdur.Text = "⏸ Duraklat";
                    btnBaslaDurdur.BackColor = Color.FromArgb(46, 204, 113);
                    _caliyor = true;
                    return;
                }

                await SiradakiSarkiyiCal();
                btnBaslaDurdur.Text = "⏸ Duraklat";
                btnBaslaDurdur.BackColor = Color.FromArgb(46, 204, 113);
                _caliyor = true;
            }
            else
            {
                // Duraklat
                if (_outputDevice != null)
                    _outputDevice.Pause();

                _sureTimer.Stop();
                btnBaslaDurdur.Text = "▶️ Devam Et";
                btnBaslaDurdur.BackColor = Color.FromArgb(241, 196, 15);
                _caliyor = false;
            }
        }

        private async Task SiradakiSarkiyiCal()
        {
            try
            {
                // API'den sıradaki şarkıyı belirle (calindi burada set edilmeyecek!)
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/siradaki");
                if (!response.IsSuccessStatusCode)
                    return;

                var json = await response.Content.ReadAsStringAsync();
                _suAnCalanItem = JsonConvert.DeserializeObject<CalmaListesiDto>(json);

                if (_suAnCalanItem == null || string.IsNullOrEmpty(_suAnCalanItem.dosyaYolu))
                    return;

                if (!File.Exists(_suAnCalanItem.dosyaYolu))
                {
                    MessageBox.Show($"Dosya bulunamadı: {_suAnCalanItem.dosyaYolu}",
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ÇAL (MP3/WAV)
                StartPlayback(_suAnCalanItem.dosyaYolu);

                // Bilgileri güncelle
                Invoke(new Action(() =>
                {
                    lblSarkiBilgisi.Text = $"{_suAnCalanItem.sarkiAdi} - {_suAnCalanItem.masaAdi}";
                    progressSarki.Value = 0;
                    _sarkiBaslangic = DateTime.Now;
                    _sureTimer.Start();

                    // Listeyi burada timer ile yenilemiyoruz,
                    // ama istersen burada sadece UI highlight için yenileyebilirsin:
                    // _ = SiraListesiniYenile();
                }));

                // DİKKAT: İSTENEN DEĞİŞİKLİK -> calindi burada set edilmiyor!
                // MarkAsPlayed(_suAnCalanItem.id) => SADECE şarkı bitince (PlaybackStopped) çağrılacak.
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
                Debug.WriteLine($"Çalındı işaretleme hatası: {ex.Message}");
            }
        }

        private void UpdateSureGosterge()
        {
            if (_suAnCalanItem == null) return;

            // Timer thread’i UI thread değil -> Invoke
            try
            {
                Invoke(new Action(() =>
                {
                    if (!_caliyor) return;

                    var gecenSure = (DateTime.Now - _sarkiBaslangic).TotalSeconds;

                    // Toplam süre: API sure > 0 ise onu kullan, yoksa NAudio TotalTime’dan çek
                    int totalSeconds = _suAnCalanItem.sure > 0
                        ? _suAnCalanItem.sure
                        : (_audioFile != null ? (int)_audioFile.TotalTime.TotalSeconds : 0);

                    if (totalSeconds <= 0) totalSeconds = 1;

                    var yuzde = (int)((gecenSure / totalSeconds) * 100);
                    progressSarki.Value = Math.Min(100, Math.Max(0, yuzde));

                    lblSure.Text =
                        $"{TimeSpan.FromSeconds(gecenSure):mm\\:ss} / {TimeSpan.FromSeconds(totalSeconds):mm\\:ss}";
                }));
            }
            catch
            {
                // form kapanırken invoke patlayabilir, görmezden gel
            }
        }

        private async void btnSonraki_Click(object sender, EventArgs e)
        {
            // Kullanıcı komutu -> doğal bitiş akışı tetiklenmesin
            StopPlayback(userRequested: true);

            // İSTENEN & GEREKLİ: Sonrakiye basınca mevcut şarkı bitmiş sayılmalı
            // (yoksa API /siradaki aynı kaydı tekrar döndürür)
            if (_suAnCalanItem != null)
                await MarkAsPlayed(_suAnCalanItem.id);

            await SiraListesiniYenile();
            await SiradakiSarkiyiCal();

            _caliyor = true;
            btnBaslaDurdur.Text = "⏸ Duraklat";
            btnBaslaDurdur.BackColor = Color.FromArgb(46, 204, 113);
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
            if (_suAnCalanItem == null) return;

            // Kullanıcı komutu
            StopPlayback(userRequested: true);

            if (!File.Exists(_suAnCalanItem.dosyaYolu))
                return;

            StartPlayback(_suAnCalanItem.dosyaYolu);

            Invoke(new Action(() =>
            {
                lblSarkiBilgisi.Text = $"{_suAnCalanItem.sarkiAdi} - {_suAnCalanItem.masaAdi}";
                progressSarki.Value = 0;
                _sarkiBaslangic = DateTime.Now;
                _sureTimer.Start();
                _caliyor = true;
                btnBaslaDurdur.Text = "⏸ Duraklat";
                btnBaslaDurdur.BackColor = Color.FromArgb(46, 204, 113);
            }));

            // DİKKAT: calindi burada set edilmiyor (bitince set edilecek)
        }

        private async void btnYenile_Click(object sender, EventArgs e)
        {
            await SiraListesiniYenile();
        }

        private void btnDurdur_Click(object sender, EventArgs e)
        {
            // Kullanıcı stop -> şarkı bitmiş sayılmayacak (calindi set edilmeyecek)
            StopPlayback(userRequested: true);

            _caliyor = false;
            btnBaslaDurdur.Text = "▶️ Çalmaya Başla";
            btnBaslaDurdur.BackColor = Color.FromArgb(52, 152, 219);

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

        // -------------------------
        // YENİ EKLENEN METOTLAR (korundu)
        // -------------------------

        private async Task<List<SarkiDto>> ApiSarkilariCek()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/sarkilar");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<SarkiDto>>(json) ?? new List<SarkiDto>();
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

        private async Task<bool> ApiSarkilariniCalmaListesineEkle()
        {
            try
            {
                _apiSarkilari = await ApiSarkilariCek();

                if (!_apiSarkilari.Any())
                {
                    MessageBox.Show("API'den aktif şarkı bulunamadı.", "Bilgi",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                int adminMasaId = 1; // Varsayılan admin masa ID
                int eklenenSayi = 0;

                foreach (var sarki in _apiSarkilari)
                {
                    var zatenVar = await CalmaListesindeVarMi(sarki.id);

                    if (!zatenVar)
                    {
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

        private async Task OtomatikCalmaKontrol()
        {
            // Otomatik mod açık değilse çık
            if (!_otomatikCalmaAktif) return;

            // Eğer şu an çalıyorsa çık (bitişte zaten event sıradakini alacak)
            if (_caliyor) return;

            try
            {
                var kuyruk = await GetKuyrukAsync();

                if (!kuyruk.Any(k => !k.calindi))
                {
                    // Sıra boşsa, API şarkılarını ekle
                    if (_apiSarkilari.Any() || await ApiSarkilariniCalmaListesineEkle())
                    {
                        await Task.Delay(1000);
                        await SiradakiSarkiyiCal();
                        _caliyor = true;
                    }
                }
                else
                {
                    await SiradakiSarkiyiCal();
                    _caliyor = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Otomatik çalma hatası: {ex.Message}");
            }
        }

        private async Task<List<CalmaListesiDto>> GetKuyrukAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/kuyruk");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<CalmaListesiDto>>(json) ?? new List<CalmaListesiDto>();
                }
                return new List<CalmaListesiDto>();
            }
            catch
            {
                return new List<CalmaListesiDto>();
            }
        }

        private void BaslatOtomatikCalma()
        {
            _otomatikCalmaAktif = true;
            _otomatikCalmaTimer.Start();
            btnOtomatikBaslat.Text = "⏸️ Otomatik Durdur";
            btnOtomatikBaslat.BackColor = Color.FromArgb(231, 76, 60);

            MessageBox.Show("Otomatik çalma başlatıldı. Sistem otomatik olarak şarkıları çalacak.",
                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DurdurOtomatikCalma()
        {
            _otomatikCalmaAktif = false;
            _otomatikCalmaTimer.Stop();
            btnOtomatikBaslat.Text = "▶️ Otomatik Çalma";
            btnOtomatikBaslat.BackColor = Color.FromArgb(46, 204, 113);
        }

        private async void btnApiSarkilariCek_Click(object sender, EventArgs e)
        {
            try
            {
                btnApiSarkilariCek.Enabled = false;
                btnApiSarkilariCek.Text = "⏳ Çekiliyor...";

                var basarili = await ApiSarkilariniCalmaListesineEkle();

                if (basarili && !_caliyor)
                {
                    await SiradakiSarkiyiCal();
                    _caliyor = true;
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopPlayback(userRequested: true);

            try { _sureTimer?.Stop(); } catch { }
            try { _otomatikCalmaTimer?.Stop(); } catch { }

            try { timerYenile?.Stop(); } catch { } // zaten start etmiyoruz ama güvenli

            try { _httpClient?.Dispose(); } catch { }
        }

<<<<<<< HEAD
        // -------------------------
        // DTO'lar
        // -------------------------
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
=======

        private async Task<HttpResponseMessage> PostAsyncSimple(string url)
        {
            return await _http.PostAsync(url, null);
>>>>>>> ac88e2837235ac296494cdec610e8b6a79add180
        }

        private async void btnTumSarkilar_Click(object sender, EventArgs e)
        {
            try
            {
                var confirm = MessageBox.Show(
                    "Tüm aktif şarkıları (siraDegeri=0) kuyruklamak istiyor musunuz?",
                    "Tüm Şarkıları Ekle",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return;

                btnTumSarkilar.Enabled = false;

                // 1️⃣ API çağrısı
                var response = await PostAsyncSimple($"{_apiBaseUrl}/tum-sarkilari-ekle");

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(
                        $"API hata: {response.StatusCode}\n{errorText}",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // 2️⃣ Kuyruğu çek
                var kuyruk = await GetKuyrukAsync();

                // 3️⃣ ListView doldur
                BindKuyrukToListView(kuyruk);

                // 4️⃣ Player boşta ise başlat
                StartPlaybackIfIdle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTumSarkilar.Enabled = true;
            }
        }

        private void StartPlaybackIfIdle()
        {
            // 1) Kuyruk yoksa çık
            if (listViewSira.Items.Count == 0)
                return;

            // 2) Şu an çalıyorsa dokunma (mevcut flag’in ile değiştir)
            // ÖRN: if (_isPlaying) return;
            if (IsPlayerBusy())
                return;

            // 3) İlk sıradaki öğeyi al ve çal
            var firstItem = listViewSira.Items[0];
            if (firstItem?.Tag is CalmaListesiDto dto)
            {
                // sende muhtemelen zaten var: PlayFile(dto.dosyaYolu) / StartSong(dto)
                PlayFromPath(dto.dosyaYolu);
            }
        }
        private bool IsPlayerBusy()
        {
            // TODO: senin projede "çalıyor mu" nasıl tutuluyor?
            // örnek:
            // return _isPlaying || waveOutEvent?.PlaybackState == PlaybackState.Playing;
            return false;
        }

        private void PlayFromPath(string path)
        {
            // TODO: sende var olan WAV/NAudio çalma metodunu çağır
            // örnek:
            // PlaySong(path);
        }

        private void BindKuyrukToListView(List<CalmaListesiDto> kuyruk)
        {
            listViewSira.BeginUpdate();
            listViewSira.Items.Clear();

            foreach (var x in kuyruk)
            {
                // 1. kolon: queue id
                var lvi = new ListViewItem(x.id.ToString());

                // örnek kolonlar:
                lvi.SubItems.Add(x.sarkiAdi ?? "");
                lvi.SubItems.Add(x.masaAdi ?? "");
                lvi.SubItems.Add(x.siraDegeri.ToString());
                lvi.SubItems.Add(x.dosyaYolu ?? "");

                // player için lazım olacak her şeyi Tag'e koy
                lvi.Tag = x;

                listViewSira.Items.Add(lvi);
            }

            listViewSira.EndUpdate();
        }



        // -------------------------
        // DTO'lar
        // -------------------------
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
