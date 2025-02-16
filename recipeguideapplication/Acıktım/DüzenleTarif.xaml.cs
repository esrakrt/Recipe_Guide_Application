using System;
using System.Data.SqlClient;
using System.Windows;

namespace Acıktım
{
    public partial class DüzenleTarif : Window
    {
        private int _tarifId;
        private string _connectionString = "Server=DESKTOP\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;";

        public DüzenleTarif(int tarifId)
        {
            InitializeComponent();
            _tarifId = tarifId;
            LoadTarif();
            LoadMalzemeler(); // Malzemeleri yükle
            LoadCategories();
        }



        private void LoadCategories()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT KategoriID, KategoriAdi FROM Kategoriler";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int kategoriId = (int)reader["KategoriID"];
                        string kategoriAdi = reader["KategoriAdi"].ToString();
                        cbKategori.Items.Add(new { KategoriID = kategoriId, KategoriAdi = kategoriAdi });
                    }
                }
            }
            cbKategori.DisplayMemberPath = "KategoriAdi"; // ComboBox'ta sadece kategori adını göster
            cbKategori.SelectedValuePath = "KategoriID";  // Seçilen değeri kategori ID olarak ayarla
        }

        private void LoadTarif()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu FROM Tarifler WHERE TarifID = @tarifID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@tarifID", _tarifId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtTarifAdi.Text = reader["TarifAdi"].ToString();
                        txtHazirlamaSuresi.Text = reader["HazirlamaSuresi"].ToString();
                        txtTalimatlar.Text = reader["Talimatlar"].ToString();

                        // Resim yolunu txtResimYolu TextBox'ına yazdır
                        txtResimYolu.Text = reader["ResimYolu"].ToString();

                        // Kategori ID'ye göre kategoriyi seç
                        int kategoriId = (int)reader["KategoriID"];
                        cbKategori.SelectedValue = kategoriId;
                    }
                }
            }
        }


        private void LoadMalzemeler()
        {
            lstMalzemeler.Items.Clear(); // Listeyi temizle
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
            SELECT m.MalzemeAdi, tm.MalzemeMiktar 
            FROM TarifMalzeme tm
            JOIN Malzemeler m ON tm.MalzemeID = m.MalzemeID
            WHERE tm.TarifID = @tarifID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@tarifID", _tarifId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string malzemeAdi = reader["MalzemeAdi"].ToString();
                        string malzemeMiktar = reader["MalzemeMiktar"].ToString();
                        lstMalzemeler.Items.Add($"Malzeme Adı: {malzemeAdi}, Miktar: {malzemeMiktar}");
                    }
                }
            }
        }


        private void BtnEkle_Click(object sender, RoutedEventArgs e)
        {
            string malzemeAdi = txtMalzemeAdi.Text;
            string malzemeMiktar = txtMalzemeMiktari.Text;

            if (string.IsNullOrWhiteSpace(malzemeAdi) || string.IsNullOrWhiteSpace(malzemeMiktar))
            {
                MessageBox.Show("Lütfen malzeme adı ve miktarı girin.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // MalzemeID'yi almak için SQL sorgusu
                string queryMalzemeID = "SELECT MalzemeID FROM Malzemeler WHERE MalzemeAdi = @MalzemeAdi";
                SqlCommand commandMalzemeID = new SqlCommand(queryMalzemeID, connection);
                commandMalzemeID.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);

                // Malzeme zaten var mı kontrolü
                object result = commandMalzemeID.ExecuteScalar();
                int malzemeID;

                if (result == null) // Malzeme yoksa
                {
                    // Malzeme ekleme sorgusu, eksik alanları varsayılan değerlerle dolduruyoruz
                    string queryInsertMalzeme = "INSERT INTO Malzemeler (MalzemeAdi, ToplamMiktar, MalzemeBirim, BirimFiyat) " +
                                                 "VALUES (@MalzemeAdi, 0, @MalzemeBirim, 0); SELECT SCOPE_IDENTITY();";

                    SqlCommand commandInsertMalzeme = new SqlCommand(queryInsertMalzeme, connection);
                    commandInsertMalzeme.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);
                    commandInsertMalzeme.Parameters.AddWithValue("@MalzemeBirim", "girilmedi"); // Varsayılan birim değeri
                                                                                                // Burada 0 olarak varsayılan birim fiyatı kullanıyoruz. İsterseniz farklı bir değer de verebilirsiniz.

                    // Malzemeyi ekle ve MalzemeID'yi al
                    malzemeID = Convert.ToInt32(commandInsertMalzeme.ExecuteScalar());
                }
                else // Malzeme zaten varsa
                {
                    malzemeID = (int)result;
                }

                // TarifMalzeme tablosuna ekleme yapmak için SQL sorgusu
                string queryInsertMalzemeRelation = "INSERT INTO TarifMalzeme (TarifID, MalzemeID, MalzemeMiktar) " +
                                                     "VALUES (@TarifID, @MalzemeID, @MalzemeMiktar)";
                SqlCommand commandInsertMalzemeRelation = new SqlCommand(queryInsertMalzemeRelation, connection);
                commandInsertMalzemeRelation.Parameters.AddWithValue("@TarifID", _tarifId);
                commandInsertMalzemeRelation.Parameters.AddWithValue("@MalzemeID", malzemeID);
                commandInsertMalzemeRelation.Parameters.AddWithValue("@MalzemeMiktar", malzemeMiktar);

                // Malzeme kaydet
                commandInsertMalzemeRelation.ExecuteNonQuery();
                MessageBox.Show("Malzeme başarıyla eklendi.");
                LoadMalzemeler(); // Malzeme listesini güncelle
            }
        }



        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            // Seçili malzemenin adını al
            string selectedMalzeme = (string)lstMalzemeler.SelectedItem;
            if (selectedMalzeme != null)
            {
                // Malzeme adını parçalayarak MalzemeAdi'ni çıkar
                string malzemeAdi = selectedMalzeme.Split(new[] { ": " }, StringSplitOptions.None)[1].Split(',')[0].Trim();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Önce MalzemeID'yi bul
                    string queryMalzemeID = "SELECT MalzemeID FROM Malzemeler WHERE MalzemeAdi = @malzemeAdi";
                    SqlCommand commandMalzemeID = new SqlCommand(queryMalzemeID, connection);
                    commandMalzemeID.Parameters.AddWithValue("@malzemeAdi", malzemeAdi);

                    object result = commandMalzemeID.ExecuteScalar();

                    if (result != null)
                    {
                        int malzemeID = (int)result;

                        // Malzeme silme sorgusu
                        string queryDelete = "DELETE FROM TarifMalzeme WHERE MalzemeID = @malzemeID AND TarifID = @tarifID";
                        SqlCommand commandDelete = new SqlCommand(queryDelete, connection);
                        commandDelete.Parameters.AddWithValue("@malzemeID", malzemeID);
                        commandDelete.Parameters.AddWithValue("@tarifID", _tarifId);

                        commandDelete.ExecuteNonQuery();
                        MessageBox.Show("Malzeme başarıyla silindi.");
                        LoadMalzemeler(); // Listeyi güncelle
                    }
                    else
                    {
                        MessageBox.Show("Malzeme bulunamadı.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silinecek bir malzeme seçin.");
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


        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Resim yolu da dahil olmak üzere sorguyu güncelle
                string query = "UPDATE Tarifler SET TarifAdi = @tarifAdi, KategoriID = @kategoriID, HazirlamaSuresi = @hazirlamaSuresi, Talimatlar = @talimatlar, ResimYolu = @resimYolu WHERE TarifID = @tarifID";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@tarifAdi", txtTarifAdi.Text);
                command.Parameters.AddWithValue("@kategoriID", (int)cbKategori.SelectedValue); // Seçili kategori ID
                command.Parameters.AddWithValue("@hazirlamaSuresi", txtHazirlamaSuresi.Text);
                command.Parameters.AddWithValue("@talimatlar", txtTalimatlar.Text);

                // Resim yolunu ekle
                command.Parameters.AddWithValue("@resimYolu", txtResimYolu.Text);

                command.Parameters.AddWithValue("@tarifID", _tarifId);

                command.ExecuteNonQuery();
                MessageBox.Show("Tarif başarıyla güncellendi.");
                this.Close(); // Pencereyi kapat
            }
        }


    }
}