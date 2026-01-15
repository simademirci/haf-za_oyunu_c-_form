using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;

namespace hafıza_oyunu_31
{
    public partial class Form1 : Form
    {
        bool oyunBasladi = false;                        // Oyunun başlayıp başlamadığını tutan kontrol değişkeni
        private PictureBox firstClicked = null;           // İlk tıklanan kartı tutan PictureBox referansı
        private PictureBox secondClicked = null;          // İkinci tıklanan kartı tutan PictureBox referansı



        private Random rnd = new Random();                // Rastgele sayı üretmek için kullanılan Random nesnesi

        int gecenSure = 0;// saniye cinsinden             // Oyunda geçen süreyi saniye cinsinden tutar

        SoundPlayer clickSound;                           // Kart tıklama sesi için ses oynatıcı
        SoundPlayer dogruSound;                            // Doğru eşleşme sesi için ses oynatıcı
        SoundPlayer yanlisSound;                          // Yanlış eşleşme sesi için ses oynatıcı
        public Form1()
        {
            InitializeComponent();

            clickSound = new SoundPlayer(Properties.Resources.click);    // Click sesi Resources içinden yüklenir
            dogruSound = new SoundPlayer(Properties.Resources.dogru);    // Doğru eşleşme sesi yüklenir
            yanlisSound = new SoundPlayer(Properties.Resources.yanlis);  // Yanlış eşleşme sesi yüklenir

            // Designer Timer zaten var
            eslesmeTimer.Interval = 1000; // 1 saniye              // Yanlış eşleşme sonrası kartları kapatacak timer’ın süresi
            eslesmeTimer.Tick += EslesmeTimer_Tick;          // Timer her tetiklendiğinde çalışacak metot bağlanır

            gecenSure = 0;                           // Süre değişkeni sıfırlanır
            lblSure.Text = "Süre: 00:00";             // Süre etiketi başlangıç değeri
            //sureTimer.Start();

            // Oyunu başlat
            //LoadAndAssignImages();
            GuncelleBestSure();                      // Kaydedilmiş en iyi süre ekrana yazdırılır
                                                     


        }

        private void YeniOyunBaslat()          // Yeni oyunu başlatan metot
        {
            if (!oyunBasladi) return;          // Oyun başlamadıysa metottan çıkılır
            eslesmeTimer.Stop();               // Olası açık eşleşme timer’ı durdurulur

            firstClicked = null;               // Önceki kart seçimleri temizlenir
            secondClicked = null;

            // Süre sıfırla
            gecenSure = 0;                        // Geçen süre sıfırlanır
            lblSure.Text = "Süre: 00:00";              // Süre etiketi güncellenir
            sureTimer.Stop();                          // Süre timer’ı durdurulur
            sureTimer.Start();                         // Süre timer’ı tekrar başlatılır

            // Kartların benzersiz kimliklerini tutan liste
            // Kart ID'leri
            List<string> cardIDs = new List<string>()
    {
        "resim1","resim1",
        "resim2","resim2",
        "resim3","resim3",
        "resim4","resim4",
        "resim5","resim5",
        "resim6","resim6",
        "resim7","resim7",
        "resim8","resim8"
    };

            // 🔀 GERÇEK KARIŞTIRMA (Fisher-Yates)
            // Kartların sırasını rastgele karıştıran döngü
            for (int i = cardIDs.Count - 1; i > 0; i--)
            {
                // Rastgele bir indeks üretilir
                int j = rnd.Next(i + 1);
                // Geçici değişken ile yer değiştirme yapılır
                string temp = cardIDs[i];
                cardIDs[i] = cardIDs[j];
                cardIDs[j] = temp;
            }

            int indexID = 0;               // Kart ID’lerini dağıtmak için indeks

            // Kartlara sırayla dağıt
            foreach (Control control in tableLayoutPanel1.Controls) // TableLayoutPanel içindeki tüm kontroller dolaşılır       
            {
                PictureBox pb = control as PictureBox;           // Kontrol PictureBox’a çevrilir
                if (pb != null)                                  // Eğer gerçekten PictureBox ise
                {
                    pb.Tag = cardIDs[indexID];                    // Kartın kimliği Tag özelliğine atanır
                    pb.Image = Properties.Resources.arkayuz;       // Kartın arka yüzü gösterilir
                    pb.Enabled = true;                             // Kart tıklanabilir yapılır
                    //
                    pb.Click -= Kart_Click;               // Aynı event birden fazla eklenmesin diye önce kaldırılır
                    pb.Click += Kart_Click;               // Kart tıklama olayı ekleni

                    indexID++;                            // Bir sonraki kart ID’sine geçilir
                }
            }
        }
        //Kart tıklama işlemi,
        //aynı isimli fakat farklı parametre alan metodlar ile overload edilerek polimorfizm uygulanmıştır.
        // Bir karta tıklandığında çalışan olay metodu
        private void Kart_Click(object sender, EventArgs e)
        {
            // Kart tıklama olayında gelen sender nesnesi,
            // polimorfik metoda gönderilir
            KartTiklamayiIsle(sender);                          // Tıklanan nesne overload edilmiş metoda gönderilir
        }
        // 1️⃣ Overload → object
        private void KartTiklamayiIsle(object sender)            // Overload edilmiş metot (object parametreli)
        {
            // Gelen object tipi PictureBox’a çevrilir
            KartTiklamayiIsle(sender as PictureBox);            // Gelen nesne PictureBox’a çevrilerek diğer metoda gönderilir
        }
        // 2️⃣ Asıl işi yapan → PictureBox
        // Asıl kart tıklama işlemlerinin yapıldığı metot
        private void KartTiklamayiIsle(PictureBox pb)   //fonksiyonun parametreleri farklı olduğu için fonksiyon aşırı yükleme
        {
            if (pb == null) return;                     // PictureBox null ise işlem yapılmaz
            if (!oyunBasladi) return;                   // Oyun başlamadıysa kartlara tıklanamaz
            if (eslesmeTimer.Enabled) return;           // Eşleşme animasyonu devam ederken tıklama engellenir

            clickSound.Play();                           // Kart tıklama sesi çalınır

            string kartID = (string)pb.Tag;                    // Kartın  kimliği Tag özelliğinden alınır

            if (pb.Image == (Image)Properties.Resources.ResourceManager.GetObject(kartID))
                return;                                               // Kart zaten açıksa tekrar tıklanması engellenir

            pb.Image = (Image)Properties.Resources.ResourceManager.GetObject(kartID);  // Kartın ön yüzü gösterilir

            if (firstClicked == null)                            // İlk kart seçilmediyse
            {
                firstClicked = pb;                               // Bu kart ilk kart olarak atanır
                return;
            }

            secondClicked = pb;                                      // İkinci kart seçilir
            EslesmeyiKontrolEt();                                    // Eşleşme kontrolü ayrı bir metoda aktarılır
        }
        private void EslesmeyiKontrolEt()                  // İki kartın eşleşip eşleşmediğini kontrol eden metot
        {
            // Kart ID’leri aynıysa
            if ((string)firstClicked.Tag == (string)secondClicked.Tag)
            {
                dogruSound.Play();                      // Doğru eşleşme sesi

                firstClicked.Enabled = false;               // Doğru eşleşen kartlar pasif hale getirilir
                secondClicked.Enabled = false;

                firstClicked = null;                         // Seçimler sıfırlanır
                secondClicked = null;

                CheckForWinner();                            // Oyun bitmiş mi kontrol edilir
            }
            else
            {
                yanlisSound.Play();                           // Yanlış eşleşme sesi
                eslesmeTimer.Start();                         // Kartların kapanması için timer başlatılır
            }
        }


        // Yanlış eşleşmede kartları kapatan timer metodu
        private void EslesmeTimer_Tick(object sender, EventArgs e)
        {
            eslesmeTimer.Stop();                                  // Timer durdurulur

            if (firstClicked != null && secondClicked != null)     // Kartlar boş değilse
            {
                firstClicked.Image = Properties.Resources.arkayuz;  // Kartların arka yüzü tekrar gösterilir
                secondClicked.Image = Properties.Resources.arkayuz;
            }

            firstClicked = null;             // Seçimler temizlenir
            secondClicked = null;
        }
        
        private void CheckForWinner()          // Oyunun bitip bitmediğini kontrol eden metot
        {
            
            foreach (Control control in tableLayoutPanel1.Controls)  // Tüm kartlar kontrol edilir
            {
                if (control is PictureBox pb && pb.Enabled)           // Eğer hâlâ aktif kart varsa oyun bitmemiştir
                    return;
            }

            sureTimer.Stop();                                   // Süre timer’ı durdurulur

            int best = Properties.Settings.Default.BestTime;      // Kayıtlı en iyi süre alınır

            if (best == 0 || gecenSure < best)                       // Yeni rekor kontrolü
            {
                Properties.Settings.Default.BestTime = gecenSure;           // Yeni rekor kaydedilir
                Properties.Settings.Default.Save();

                MessageBox.Show(
                    $"🎉 YENİ REKOR!\nSüren: {lblSure.Text}",
                    "Tebrikler"
                );
            }
            else
            {
                MessageBox.Show(
                    $"Tebrikler!\nSüren: {lblSure.Text}",
                    "Oyun Bitti"
                );
            }
            // En iyi süre etiketi güncellenir
            GuncelleBestSure(); // ⭐ LABEL'I GÜNCELLE

            DialogResult sonuc = MessageBox.Show(
                "Yeni oyun başlasın mı?",
                "Devam",
                MessageBoxButtons.YesNo
            );

            if (sonuc == DialogResult.Yes)
            {
                YeniOyunBaslat();
            }
        }

        private void sureTimer_Tick(object sender, EventArgs e)    // Süreyi her saniye artıran timer metodu
        {
            gecenSure++;                         // Süre 1 saniye artırılır

            int dakika = gecenSure / 60;            // Dakika hesaplanır
            int saniye = gecenSure % 60;             // Saniye hesaplanır

            lblSure.Text = $"Süre: {dakika:D2}:{saniye:D2}";       // Süre etiketi güncellenir
        }

        private void btnYeniOyun_Click(object sender, EventArgs e)            // Yeni oyun butonuna basıldığında
        {
            YeniOyunBaslat();                     // Yeni oyun başlatılır
        }

        private void GuncelleBestSure()                    // En iyi süreyi label üzerinde gösteren metot
        {
            int best = Properties.Settings.Default.BestTime;         // En iyi süre alınır

            if (best == 0)              // Eğer hiç rekor yoksa
            {
                lblBest.Text = "En İyi Süre: --:--";
            }
            else              // Rekor varsa formatlanarak yazdırılır
            {
                int dk = best / 60;
                int sn = best % 60;
                lblBest.Text = $"En İyi Süre: {dk:D2}:{sn:D2}";
            }
        }

        private void OyunuBaslat()     //encapsulation(kapsülleme) // Oyunu başlatan kapsüllenmiş metot
        {
            oyunBasladi = true;                // Oyun başladı olarak işaretlenir

            gecenSure = 0;                        // Süre sıfırlanır
            lblSure.Text = "Süre: 00:00";          // Süre etiketi sıfırlanır

            sureTimer.Stop();                         // Süre timer’ı yeniden başlatılır
            sureTimer.Start();

            YeniOyunBaslat();           // Kartlar dağıtılır
        }

        private void btnBasla_Click(object sender, EventArgs e)           // Başlat butonuna basıldığında
        {
            //oyunBasladi = true;
            //gecenSure = 0;
            //lblSure.Text = "Süre: 00:00";
            //sureTimer.Start();   // ✅ SADECE BURADA
            //YeniOyunBaslat();
            OyunuBaslat();      //encapsulation (kapsülleme)   // Oyunu başlat

        }
    }
}
