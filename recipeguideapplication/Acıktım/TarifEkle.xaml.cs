using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Acıktım
{
    public partial class TarifEkle : UserControl
    {
        public TarifEkle()
        {
            InitializeComponent();
            LoadKategoriler();
            cmbKategori.SelectionChanged += CmbKategori_SelectionChanged;
        }



        // Veritabanından Kategoriler'i yükleyen metot
        private void LoadKategoriler()
        {
            // Veritabanı bağlantı dizesi (kendi sunucu bilgilerinize göre ayarlayın)
            string connectionString = "Server=DESKTOP-D3P3LRR\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;TrustServerCertificate=True;";

            // SQL sorgusu
            string query = "SELECT KategoriAdi FROM Kategoriler";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Bağlantıyı aç
                    connection.Open();

                    // SQL komutunu oluştur
                    SqlCommand command = new SqlCommand(query, connection);

                    // Verileri okumak için SqlDataReader kullan
                    SqlDataReader reader = command.ExecuteReader();

                    // Verileri ComboBox'a ekle
                    while (reader.Read())
                    {
                        string kategoriAdi = reader["KategoriAdi"].ToString();
                        cmbKategori.Items.Add(new ComboBoxItem() { Content = kategoriAdi });
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj göster
                MessageBox.Show("Veritabanına bağlanırken bir hata oluştu: " + ex.Message);
            }
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            // Girdi bilgilerini al
            string tarifAdi = txtTarifAdi.Text;
            string kategori = (cmbKategori.SelectedItem as ComboBoxItem)?.Content.ToString();
            string hazirlamaSuresi = txtHazirlamaSuresi.Text;
            string talimatlar = txtTalimatlar.Text;

            // Tarif adı kontrolü için SQL sorgusu
            string queryCheckTarif = "SELECT COUNT(*) FROM Tarifler WHERE TarifAdi = @TarifAdi";

            // KategoriID'yi almak için SQL sorgusu
            string queryKategoriID = "SELECT KategoriID FROM Kategoriler WHERE KategoriAdi = @KategoriAdi";

            // Tarifler tablosuna ekleme yapmak için SQL sorgusu
            string queryInsert = "INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu) " +
                                 "VALUES (@TarifAdi, @KategoriID, @HazirlamaSuresi, @Talimatlar, @ResimYolu); SELECT SCOPE_IDENTITY();";

            // Veritabanı bağlantı dizesi
            string connectionString = "Server=DESKTOP-D3P3LRR\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;TrustServerCertificate=True;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Tarif adı kontrolü için SQL komutunu oluştur
                    SqlCommand commandCheckTarif = new SqlCommand(queryCheckTarif, connection);
                    commandCheckTarif.Parameters.AddWithValue("@TarifAdi", tarifAdi);

                    int tarifCount = (int)commandCheckTarif.ExecuteScalar();

                    if (tarifCount > 0)
                    {
                        // Aynı isimde bir tarif zaten varsa ekleme yapma
                        MessageBox.Show("Bu isimde bir tarif zaten mevcut. Lütfen farklı bir isim kullanın.");
                        return;
                    }

                    // KategoriID'yi almak için SQL komutunu oluştur
                    SqlCommand commandKategoriID = new SqlCommand(queryKategoriID, connection);
                    commandKategoriID.Parameters.AddWithValue("@KategoriAdi", kategori);
                    int kategoriID = (int)commandKategoriID.ExecuteScalar(); // KategoriID'yi al

                    // Tarifler tablosuna ekleme yapmak için SQL komutunu oluştur
                    SqlCommand commandInsert = new SqlCommand(queryInsert, connection);
                    commandInsert.Parameters.AddWithValue("@TarifAdi", tarifAdi);
                    commandInsert.Parameters.AddWithValue("@KategoriID", kategoriID);
                    commandInsert.Parameters.AddWithValue("@HazirlamaSuresi", hazirlamaSuresi);
                    commandInsert.Parameters.AddWithValue("@Talimatlar", talimatlar);
                    commandInsert.Parameters.AddWithValue("@ResimYolu", txtResimYolu.Text); // Resim yolunu ekle

                    // Komutu çalıştır ve veriyi ekle, tarifin ID'sini al
                    int tarifID = Convert.ToInt32(commandInsert.ExecuteScalar());

                    if (tarifID > 0)
                    {
                        // Malzeme ekleme işlemi
                        foreach (var malzeme in MalzemePanel.Children)
                        {
                            if (malzeme is StackPanel malzemePanel)
                            {
                                var malzemeTextBlock = malzemePanel.Children[0] as TextBlock;
                                string[] parts = malzemeTextBlock.Text.Split(new[] { " - " }, StringSplitOptions.None);
                                string malzemeAdi = parts[0].Replace("Malzeme: ", "").Trim();
                                string malzemeMiktar = parts[1].Replace("Miktar: ", "").Trim();

                                // MalzemeID'yi almak için SQL sorgusu
                                string queryMalzemeID = "SELECT MalzemeID FROM Malzemeler WHERE MalzemeAdi = @MalzemeAdi";
                                SqlCommand commandMalzemeID = new SqlCommand(queryMalzemeID, connection);
                                commandMalzemeID.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);

                                object result = commandMalzemeID.ExecuteScalar();
                                int malzemeID;

                                if (result == null)
                                {
                                    string queryInsertMalzeme = "INSERT INTO Malzemeler (MalzemeAdi, ToplamMiktar, MalzemeBirim, BirimFiyat) " +
                                                                 "VALUES (@MalzemeAdi, 0, @MalzemeBirim, 0); SELECT SCOPE_IDENTITY();";

                                    SqlCommand commandInsertMalzeme = new SqlCommand(queryInsertMalzeme, connection);
                                    commandInsertMalzeme.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);
                                    commandInsertMalzeme.Parameters.AddWithValue("@MalzemeBirim", "girilmedi");

                                    malzemeID = Convert.ToInt32(commandInsertMalzeme.ExecuteScalar());
                                }
                                else
                                {
                                    malzemeID = (int)result;
                                }

                                string queryInsertMalzemeRelation = "INSERT INTO TarifMalzeme (TarifID, MalzemeID, MalzemeMiktar) " +
                                                                     "VALUES (@TarifID, @MalzemeID, @MalzemeMiktar)";
                                SqlCommand commandInsertMalzemeRelation = new SqlCommand(queryInsertMalzemeRelation, connection);
                                commandInsertMalzemeRelation.Parameters.AddWithValue("@TarifID", tarifID);
                                commandInsertMalzemeRelation.Parameters.AddWithValue("@MalzemeID", malzemeID);
                                commandInsertMalzemeRelation.Parameters.AddWithValue("@MalzemeMiktar", malzemeMiktar);

                                commandInsertMalzemeRelation.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("Tarif başarıyla kaydedildi.");
                    }
                    else
                    {
                        MessageBox.Show("Tarif kaydedilemedi.");
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }




        private void BtnGozat_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All Files (*.*)|*.*",
                Title = "Bir Resim Seçin"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Seçilen dosyanın yolunu txtResimYolu TextBox'ına yaz
                txtResimYolu.Text = openFileDialog.FileName;
            }
        }

        private void CmbKategori_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kullanıcı bir seçim yaptıysa placeholder'ı gizle, aksi halde göster
            if (cmbKategori.SelectedIndex >= 0)
            {
                cmbKategoriPlaceholder.Visibility = Visibility.Collapsed; // Gizle
            }
            else
            {
                cmbKategoriPlaceholder.Visibility = Visibility.Visible; // Göster
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // TextBox odağı aldığında ilgili placeholder'ı gizle
            if (sender == txtTarifAdi && string.IsNullOrWhiteSpace(txtTarifAdi.Text))
            {
                txtTarifAdiPlaceholder.Visibility = Visibility.Collapsed;
            }
            else if (sender == txtHazirlamaSuresi && string.IsNullOrWhiteSpace(txtHazirlamaSuresi.Text))
            {
                txtHazirlamaSuresiPlaceholder.Visibility = Visibility.Collapsed;
            }
            else if (sender == txtTalimatlar && string.IsNullOrWhiteSpace(txtTalimatlar.Text))
            {
                txtTalimatlarPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Kullanıcı yazmaya başladığında placeholder'ları gizle
            if (sender == txtTarifAdi)
            {
                txtTarifAdiPlaceholder.Visibility =
                    string.IsNullOrWhiteSpace(txtTarifAdi.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (sender == txtHazirlamaSuresi)
            {
                txtHazirlamaSuresiPlaceholder.Visibility =
                    string.IsNullOrWhiteSpace(txtHazirlamaSuresi.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (sender == txtTalimatlar)
            {
                txtTalimatlarPlaceholder.Visibility =
                    string.IsNullOrWhiteSpace(txtTalimatlar.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void BtnMalzemeEkle_Click(object sender, RoutedEventArgs e)
        {
            // Yeni pop-up penceresini aç
            MalzemeEklePopup malzemeEklePopup = new MalzemeEklePopup();

            // Pop-up'ı göster ve kullanıcı eklemeleri tamamladıysa işlemi gerçekleştir
            if (malzemeEklePopup.ShowDialog() == true)
            {
                // Eklenen malzemeleri ana pencereye ekle
                foreach (var malzeme in malzemeEklePopup.EklenenMalzemeler)
                {
                    StackPanel yeniMalzemePanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    TextBlock malzemeTextBlock = new TextBlock
                    {
                        Text = $"Malzeme: {malzeme.Item1} - Miktar: {malzeme.Item2}",
                        Width = 300,
                        FontSize = 16,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    yeniMalzemePanel.Children.Add(malzemeTextBlock);
                    MalzemePanel.Children.Add(yeniMalzemePanel);
                }
            }
        }

    }
}