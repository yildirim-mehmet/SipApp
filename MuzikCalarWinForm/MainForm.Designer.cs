namespace MuzikCalarWinForm
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblSuAnCalan;
        private System.Windows.Forms.Label lblSarkiBilgisi;
        private System.Windows.Forms.ListView listViewSira;
        private System.Windows.Forms.ColumnHeader colSira;
        private System.Windows.Forms.ColumnHeader colSarkiAdi;
        private System.Windows.Forms.ColumnHeader colMasa;
        private System.Windows.Forms.ColumnHeader colOncelik;
        private System.Windows.Forms.ColumnHeader colDurum;
        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.Button btnOnceki;
        private System.Windows.Forms.Button btnBaslaDurdur;
        private System.Windows.Forms.Button btnSonraki;
        private System.Windows.Forms.Button btnYenile;
        private System.Windows.Forms.Button btnDurdur;
        private System.Windows.Forms.Button btnKapat;
        private System.Windows.Forms.Timer timerYenile;
        private System.Windows.Forms.ProgressBar progressSarki;
        private System.Windows.Forms.Label lblSure;
        private System.Windows.Forms.NumericUpDown numYenilemeAraligi;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbSesSeviyesi;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSarkiEkle;
        private System.Windows.Forms.Button btnListeTemizle;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            panelTop = new Panel();
            lblSarkiBilgisi = new Label();
            lblSuAnCalan = new Label();
            listViewSira = new ListView();
            colSira = new ColumnHeader();
            colSarkiAdi = new ColumnHeader();
            colMasa = new ColumnHeader();
            colOncelik = new ColumnHeader();
            colDurum = new ColumnHeader();
            panelControls = new Panel();
            btnListeTemizle = new Button();
            btnSarkiEkle = new Button();
            label2 = new Label();
            cbSesSeviyesi = new ComboBox();
            label1 = new Label();
            numYenilemeAraligi = new NumericUpDown();
            btnKapat = new Button();
            btnDurdur = new Button();
            btnYenile = new Button();
            btnSonraki = new Button();
            btnBaslaDurdur = new Button();
            btnOnceki = new Button();
            progressSarki = new ProgressBar();
            lblSure = new Label();
            timerYenile = new System.Windows.Forms.Timer(components);
            btnTumSarkilar = new Button();
            panelTop.SuspendLayout();
            panelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numYenilemeAraligi).BeginInit();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(52, 73, 94);
            panelTop.Controls.Add(lblSarkiBilgisi);
            panelTop.Controls.Add(lblSuAnCalan);
            panelTop.Dock = DockStyle.Top;
            panelTop.ForeColor = Color.White;
            panelTop.Location = new Point(0, 0);
            panelTop.Margin = new Padding(4, 3, 4, 3);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1190, 92);
            panelTop.TabIndex = 0;
            // 
            // lblSarkiBilgisi
            // 
            lblSarkiBilgisi.AutoSize = true;
            lblSarkiBilgisi.Font = new Font("Microsoft Sans Serif", 14F, FontStyle.Bold);
            lblSarkiBilgisi.Location = new Point(23, 46);
            lblSarkiBilgisi.Margin = new Padding(4, 0, 4, 0);
            lblSarkiBilgisi.Name = "lblSarkiBilgisi";
            lblSarkiBilgisi.Size = new Size(232, 24);
            lblSarkiBilgisi.TabIndex = 1;
            lblSarkiBilgisi.Text = "- Sistem Hazır Bekliyor -";
            // 
            // lblSuAnCalan
            // 
            lblSuAnCalan.AutoSize = true;
            lblSuAnCalan.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            lblSuAnCalan.Location = new Point(23, 17);
            lblSuAnCalan.Margin = new Padding(4, 0, 4, 0);
            lblSuAnCalan.Name = "lblSuAnCalan";
            lblSuAnCalan.Size = new Size(115, 17);
            lblSuAnCalan.TabIndex = 0;
            lblSuAnCalan.Text = "ŞU AN ÇALAN:";
            // 
            // listViewSira
            // 
            listViewSira.Columns.AddRange(new ColumnHeader[] { colSira, colSarkiAdi, colMasa, colOncelik, colDurum });
            listViewSira.Dock = DockStyle.Fill;
            listViewSira.FullRowSelect = true;
            listViewSira.GridLines = true;
            listViewSira.Location = new Point(0, 92);
            listViewSira.Margin = new Padding(4, 3, 4, 3);
            listViewSira.Name = "listViewSira";
            listViewSira.Size = new Size(1190, 290);
            listViewSira.TabIndex = 1;
            listViewSira.UseCompatibleStateImageBehavior = false;
            listViewSira.View = View.Details;
            // 
            // colSira
            // 
            colSira.Text = "Sıra";
            colSira.Width = 50;
            // 
            // colSarkiAdi
            // 
            colSarkiAdi.Text = "Şarkı Adı";
            colSarkiAdi.Width = 400;
            // 
            // colMasa
            // 
            colMasa.Text = "Ekleyen Masa";
            colMasa.Width = 150;
            // 
            // colOncelik
            // 
            colOncelik.Text = "Öncelik";
            colOncelik.Width = 80;
            // 
            // colDurum
            // 
            colDurum.Text = "Durum";
            colDurum.Width = 100;
            // 
            // panelControls
            // 
            panelControls.BackColor = Color.FromArgb(236, 240, 241);
            panelControls.Controls.Add(btnTumSarkilar);
            panelControls.Controls.Add(btnListeTemizle);
            panelControls.Controls.Add(btnSarkiEkle);
            panelControls.Controls.Add(label2);
            panelControls.Controls.Add(cbSesSeviyesi);
            panelControls.Controls.Add(label1);
            panelControls.Controls.Add(numYenilemeAraligi);
            panelControls.Controls.Add(btnKapat);
            panelControls.Controls.Add(btnDurdur);
            panelControls.Controls.Add(btnYenile);
            panelControls.Controls.Add(btnSonraki);
            panelControls.Controls.Add(btnBaslaDurdur);
            panelControls.Controls.Add(btnOnceki);
            panelControls.Controls.Add(progressSarki);
            panelControls.Controls.Add(lblSure);
            panelControls.Dock = DockStyle.Bottom;
            panelControls.Location = new Point(0, 382);
            panelControls.Margin = new Padding(4, 3, 4, 3);
            panelControls.Name = "panelControls";
            panelControls.Size = new Size(1190, 162);
            panelControls.TabIndex = 2;
            // 
            // btnListeTemizle
            // 
            btnListeTemizle.BackColor = Color.FromArgb(230, 126, 34);
            btnListeTemizle.FlatStyle = FlatStyle.Flat;
            btnListeTemizle.Font = new Font("Microsoft Sans Serif", 9F);
            btnListeTemizle.ForeColor = Color.White;
            btnListeTemizle.Location = new Point(175, 110);
            btnListeTemizle.Margin = new Padding(4, 3, 4, 3);
            btnListeTemizle.Name = "btnListeTemizle";
            btnListeTemizle.Size = new Size(140, 35);
            btnListeTemizle.TabIndex = 13;
            btnListeTemizle.Text = "🗑️ Liste Temizle";
            btnListeTemizle.UseVisualStyleBackColor = false;
            btnListeTemizle.Click += btnListeTemizle_Click;
            // 
            // btnSarkiEkle
            // 
            btnSarkiEkle.BackColor = Color.FromArgb(241, 196, 15);
            btnSarkiEkle.FlatStyle = FlatStyle.Flat;
            btnSarkiEkle.Font = new Font("Microsoft Sans Serif", 9F);
            btnSarkiEkle.ForeColor = Color.White;
            btnSarkiEkle.Location = new Point(23, 110);
            btnSarkiEkle.Margin = new Padding(4, 3, 4, 3);
            btnSarkiEkle.Name = "btnSarkiEkle";
            btnSarkiEkle.Size = new Size(140, 35);
            btnSarkiEkle.TabIndex = 12;
            btnSarkiEkle.Text = "📁 Şarkı Ekle";
            btnSarkiEkle.UseVisualStyleBackColor = false;
            btnSarkiEkle.Click += btnSarkiEkle_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(863, 12);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(63, 15);
            label2.TabIndex = 11;
            label2.Text = "Ses Seviye:";
            // 
            // cbSesSeviyesi
            // 
            cbSesSeviyesi.DropDownStyle = ComboBoxStyle.DropDownList;
            cbSesSeviyesi.FormattingEnabled = true;
            cbSesSeviyesi.Items.AddRange(new object[] { "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" });
            cbSesSeviyesi.Location = new Point(863, 29);
            cbSesSeviyesi.Margin = new Padding(4, 3, 4, 3);
            cbSesSeviyesi.Name = "cbSesSeviyesi";
            cbSesSeviyesi.Size = new Size(81, 23);
            cbSesSeviyesi.TabIndex = 10;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(723, 12);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(81, 15);
            label1.TabIndex = 9;
            label1.Text = "Yenileme (sn):";
            // 
            // numYenilemeAraligi
            // 
            numYenilemeAraligi.Location = new Point(723, 29);
            numYenilemeAraligi.Margin = new Padding(4, 3, 4, 3);
            numYenilemeAraligi.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            numYenilemeAraligi.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numYenilemeAraligi.Name = "numYenilemeAraligi";
            numYenilemeAraligi.Size = new Size(58, 23);
            numYenilemeAraligi.TabIndex = 8;
            numYenilemeAraligi.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // btnKapat
            // 
            btnKapat.BackColor = Color.FromArgb(149, 165, 166);
            btnKapat.FlatStyle = FlatStyle.Flat;
            btnKapat.Font = new Font("Microsoft Sans Serif", 10F);
            btnKapat.ForeColor = Color.White;
            btnKapat.Location = new Point(996, 17);
            btnKapat.Margin = new Padding(4, 3, 4, 3);
            btnKapat.Name = "btnKapat";
            btnKapat.Size = new Size(117, 46);
            btnKapat.TabIndex = 5;
            btnKapat.Text = "✕ Kapat";
            btnKapat.UseVisualStyleBackColor = false;
            btnKapat.Click += btnKapat_Click;
            // 
            // btnDurdur
            // 
            btnDurdur.BackColor = Color.FromArgb(231, 76, 60);
            btnDurdur.FlatStyle = FlatStyle.Flat;
            btnDurdur.Font = new Font("Microsoft Sans Serif", 10F);
            btnDurdur.ForeColor = Color.White;
            btnDurdur.Location = new Point(583, 17);
            btnDurdur.Margin = new Padding(4, 3, 4, 3);
            btnDurdur.Name = "btnDurdur";
            btnDurdur.Size = new Size(117, 46);
            btnDurdur.TabIndex = 4;
            btnDurdur.Text = "⏹ Durdur";
            btnDurdur.UseVisualStyleBackColor = false;
            btnDurdur.Click += btnBaslaDurdur_Click;
            // 
            // btnYenile
            // 
            btnYenile.BackColor = Color.FromArgb(155, 89, 182);
            btnYenile.FlatStyle = FlatStyle.Flat;
            btnYenile.Font = new Font("Microsoft Sans Serif", 10F);
            btnYenile.ForeColor = Color.White;
            btnYenile.Location = new Point(432, 17);
            btnYenile.Margin = new Padding(4, 3, 4, 3);
            btnYenile.Name = "btnYenile";
            btnYenile.Size = new Size(140, 46);
            btnYenile.TabIndex = 3;
            btnYenile.Text = "🔄 Listeyi Yenile";
            btnYenile.UseVisualStyleBackColor = false;
            btnYenile.Click += btnYenile_Click;
            // 
            // btnSonraki
            // 
            btnSonraki.BackColor = Color.FromArgb(52, 152, 219);
            btnSonraki.FlatStyle = FlatStyle.Flat;
            btnSonraki.Font = new Font("Microsoft Sans Serif", 10F);
            btnSonraki.ForeColor = Color.White;
            btnSonraki.Location = new Point(303, 17);
            btnSonraki.Margin = new Padding(4, 3, 4, 3);
            btnSonraki.Name = "btnSonraki";
            btnSonraki.Size = new Size(117, 46);
            btnSonraki.TabIndex = 2;
            btnSonraki.Text = "Sonraki ⏭";
            btnSonraki.UseVisualStyleBackColor = false;
            btnSonraki.Click += btnSonraki_Click;
            // 
            // btnBaslaDurdur
            // 
            btnBaslaDurdur.BackColor = Color.FromArgb(46, 204, 113);
            btnBaslaDurdur.FlatStyle = FlatStyle.Flat;
            btnBaslaDurdur.Font = new Font("Microsoft Sans Serif", 10F);
            btnBaslaDurdur.ForeColor = Color.White;
            btnBaslaDurdur.Location = new Point(152, 17);
            btnBaslaDurdur.Margin = new Padding(4, 3, 4, 3);
            btnBaslaDurdur.Name = "btnBaslaDurdur";
            btnBaslaDurdur.Size = new Size(140, 46);
            btnBaslaDurdur.TabIndex = 1;
            btnBaslaDurdur.Text = "▶️ Çalmaya Başla";
            btnBaslaDurdur.UseVisualStyleBackColor = false;
            btnBaslaDurdur.Click += btnBaslaDurdur_Click;
            // 
            // btnOnceki
            // 
            btnOnceki.BackColor = Color.FromArgb(52, 152, 219);
            btnOnceki.FlatStyle = FlatStyle.Flat;
            btnOnceki.Font = new Font("Microsoft Sans Serif", 10F);
            btnOnceki.ForeColor = Color.White;
            btnOnceki.Location = new Point(23, 17);
            btnOnceki.Margin = new Padding(4, 3, 4, 3);
            btnOnceki.Name = "btnOnceki";
            btnOnceki.Size = new Size(117, 46);
            btnOnceki.TabIndex = 0;
            btnOnceki.Text = "⏮ Önceki";
            btnOnceki.UseVisualStyleBackColor = false;
            btnOnceki.Click += btnOnceki_Click;
            // 
            // progressSarki
            // 
            progressSarki.Location = new Point(325, 139);
            progressSarki.Margin = new Padding(4, 3, 4, 3);
            progressSarki.Name = "progressSarki";
            progressSarki.Size = new Size(467, 23);
            progressSarki.TabIndex = 6;
            // 
            // lblSure
            // 
            lblSure.AutoSize = true;
            lblSure.Font = new Font("Microsoft Sans Serif", 9F);
            lblSure.Location = new Point(790, 147);
            lblSure.Margin = new Padding(4, 0, 4, 0);
            lblSure.Name = "lblSure";
            lblSure.Size = new Size(78, 15);
            lblSure.TabIndex = 7;
            lblSure.Text = "00:00 / 00:00";
            // 
            // timerYenile
            // 
            timerYenile.Interval = 10000;
            // 
            // btnTumSarkilar
            // 
            btnTumSarkilar.BackColor = Color.FromArgb(230, 126, 34);
            btnTumSarkilar.FlatStyle = FlatStyle.Flat;
            btnTumSarkilar.Font = new Font("Microsoft Sans Serif", 9F);
            btnTumSarkilar.ForeColor = Color.White;
            btnTumSarkilar.Location = new Point(967, 69);
            btnTumSarkilar.Margin = new Padding(4, 3, 4, 3);
            btnTumSarkilar.Name = "btnTumSarkilar";
            btnTumSarkilar.Size = new Size(169, 35);
            btnTumSarkilar.TabIndex = 14;
            btnTumSarkilar.Text = "Tüm Şarıları Ekle && Çal";
            btnTumSarkilar.UseVisualStyleBackColor = false;
            btnTumSarkilar.Click += btnTumSarkilar_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1190, 544);
            Controls.Add(listViewSira);
            Controls.Add(panelTop);
            Controls.Add(panelControls);
            Margin = new Padding(4, 3, 4, 3);
            Name = "MainForm";
            Text = "Müzik Çalar Sistemi - SipApp Entegrasyonu";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelControls.ResumeLayout(false);
            panelControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numYenilemeAraligi).EndInit();
            ResumeLayout(false);
        }
        #endregion

        private Button btnTumSarkilar;
    }
}