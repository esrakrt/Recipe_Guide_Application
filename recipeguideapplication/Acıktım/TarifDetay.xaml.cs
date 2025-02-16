using System.Data.SqlClient;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;

namespace Acıktım
{
    public partial class TarifDetay : UserControl
    {
        private string _tarifAdi;

        public TarifDetay(string tarifAdi)
        {
            InitializeComponent();
            _tarifAdi = tarifAdi;
            LoadTarifDetay();
        }
        private void LoadMalzemeler(int tarifId)
        {
            List<string> malzemeler = new List<string>();
            Dictionary<int, (string MalzemeAdi, string ToplamMiktar, string MalzemeBirim, decimal BirimFiyat)> malzemeDict = new Dictionary<int, (string, string, string, decimal)>();
            decimal toplamMaliyet = 0; // Toplam maliyet
            int toplamMalzemeSayisi = 0; // Toplam malzeme sayısını tutacak

            string connectionString = "Server=DESKTOP-D3P3LRR\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string malzemeQuery = "SELECT MalzemeID, MalzemeAdi, ToplamMiktar, MalzemeBirim, BirimFiyat FROM Malzemeler";
                SqlCommand malzemeCommand = new SqlCommand(malzemeQuery, connection);

                using (SqlDataReader malzemeReader = malzemeCommand.ExecuteReader())
                {
                    while (malzemeReader.Read())
                    {
                        int malzemeId = malzemeReader.GetInt32(0);
                        string malzemeAdi = malzemeReader.GetString(1);
                        string toplamMiktar = malzemeReader["ToplamMiktar"].ToString();
                        string malzemeBirim = malzemeReader["MalzemeBirim"].ToString();
                        decimal birimFiyat = malzemeReader.GetDecimal(4);

                        malzemeDict[malzemeId] = (malzemeAdi, toplamMiktar, malzemeBirim, birimFiyat);
                    }
                }
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string tarifQuery = "SELECT MalzemeID, MalzemeMiktar FROM TarifMalzeme WHERE TarifID = @tarifID";
                SqlCommand tarifCommand = new SqlCommand(tarifQuery, connection);
                tarifCommand.Parameters.AddWithValue("@tarifID", tarifId);

                using (SqlDataReader reader = tarifCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int malzemeId = reader.GetInt32(0);
                        string tarifMiktar = reader["MalzemeMiktar"].ToString();

                        if (malzemeDict.TryGetValue(malzemeId, out var malzemeBilgi))
                        {
                            toplamMalzemeSayisi++; // Her bulunan malzeme için sayıyı artır

                            string malzemeAdi = malzemeBilgi.MalzemeAdi;
                            string buzdolabindakiMiktar = malzemeBilgi.ToplamMiktar;
                            string malzemeBirim = malzemeBilgi.MalzemeBirim;
                            decimal birimFiyat = malzemeBilgi.BirimFiyat;

                            decimal buzdolabindakiMiktarDecimal = decimal.Parse(buzdolabindakiMiktar);
                            decimal tarifMiktarDecimal = decimal.Parse(tarifMiktar);

                            decimal malzemeMaliyeti = tarifMiktarDecimal * birimFiyat;
                            toplamMaliyet += malzemeMaliyeti;

                            TextBlock malzemeTextBlock = new TextBlock
                            {
                                Text = $"Malzeme: {malzemeAdi}, Gerekli: {tarifMiktar} {malzemeBirim}, Maliyet: {malzemeMaliyeti:C}"
                            };

                            if (buzdolabindakiMiktarDecimal < tarifMiktarDecimal)
                            {
                                malzemeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                            }
                            else
                            {
                                malzemeTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                            }

                            lstMalzemeler.Items.Add(malzemeTextBlock);
                        }
                    }
                }
            }

            TextBlock toplamMaliyetTextBlock = new TextBlock
            {
                Text = $"Toplam Maliyet: {toplamMaliyet:C}",
                Foreground = new SolidColorBrush(Colors.Blue)
            };
            lstMalzemeler.Items.Add(toplamMaliyetTextBlock);

            // Toplam malzeme sayısını göster
            
        }









        private void LoadTarifDetay()
        {
            string connectionString = "Server=DESKTOP-D3P3LRR\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Tarif detaylarını almak için SQL sorgusu
                string query = "SELECT TarifID, HazirlamaSuresi, Talimatlar, KategoriID, ResimYolu FROM Tarifler WHERE TarifAdi = @tarifAdi"; // ResimYolu eklendi
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@tarifAdi", _tarifAdi);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Verileri arayüze yaz
                        lblTarifAdi.Content = _tarifAdi;
                        lblHazirlanmaSuresi.Content = "Hazırlama Süresi: " + reader["HazirlamaSuresi"].ToString();

                        txtTalimatlar.Text = reader["Talimatlar"].ToString();

                        // Resim yolunu al ve resmi yükle
                        string resimYolu = reader["ResimYolu"].ToString();
                        if (!string.IsNullOrEmpty(resimYolu))
                        {
                            imgTarif.Source = new BitmapImage(new Uri(resimYolu, UriKind.RelativeOrAbsolute)); // Resmi yükle
                        }

                        // TarifID'yi al ve malzemeleri yükle
                        int tarifId = reader.GetInt32(0);
                        LoadMalzemeler(tarifId);

                        // KategoriID'yi al ve kategori adını yükle
                        int kategoriId = reader.GetInt32(3);
                        string kategoriAdi = GetKategoriAdi(kategoriId);
                        lblKategori.Content = $"Kategori: {kategoriAdi}"; // Kategori adını gösteren Label
                    }
                    else
                    {
                        MessageBox.Show("Tarif bulunamadı.");
                    }
                }
            }
        }

        private void Duzenle_Click(object sender, RoutedEventArgs e)
        {
            int tarifId = GetTarifId(_tarifAdi);
            if (tarifId > 0)
            {
                DüzenleTarif editWindow = new DüzenleTarif(tarifId);
                editWindow.ShowDialog(); // Modal pencere olarak aç
            }
            else
            {
                MessageBox.Show("Tarif bulunamadı.");
            }
        }

        private void Sil_Click(object sender, RoutedEventArgs e)
        {
            string tarifAdi = lblTarifAdi.Content.ToString();
            int tarifId = GetTarifId(tarifAdi);

            if (tarifId > 0)
            {
                string connectionString = "Server=DESKTOP-D3P3LRR\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction(); // Transaction başlat

                    try
                    {
                        // Önce TarifMalzeme tablosundan sil
                        string deleteMalzemeQuery = "DELETE FROM TarifMalzeme WHERE TarifID = @tarifID";
                        SqlCommand deleteMalzemeCommand = new SqlCommand(deleteMalzemeQuery, connection, transaction);
                        deleteMalzemeCommand.Parameters.AddWithValue("@tarifID", tarifId);
                        deleteMalzemeCommand.ExecuteNonQuery();

                        // Sonra Tarifler tablosundan sil
                        string deleteTarifQuery = "DELETE FROM Tarifler WHERE TarifID = @tarifID";
                        SqlCommand deleteTarifCommand = new SqlCommand(deleteTarifQuery, connection, transaction);
                        deleteTarifCommand.Parameters.AddWithValue("@tarifID", tarifId);
                        deleteTarifCommand.ExecuteNonQuery();

                        transaction.Commit(); // İşlemleri onayla

                        MessageBox.Show("Tarif ve malzemeleri başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Hata durumunda geri al
                        MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Tarif bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // TarifID'yi almak için bir metot
        private int GetTarifId(string tarifAdi)
        {
            int tarifId = -1; // Geçersiz ID başlangıcı
            string connectionString = "Server=DESKTOP-D3P3LRR\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT TarifID FROM Tarifler WHERE TarifAdi = @tarifAdi";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@tarifAdi", tarifAdi);

                var result = command.ExecuteScalar();
                if (result != null)
                {
                    tarifId = Convert.ToInt32(result);
                }
            }

            return tarifId;
        }

        private string GetKategoriAdi(int kategoriId)
        {
            string kategoriAdi = string.Empty;
            string connectionString = "Server=DESKTOP-D3P3LRR\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT KategoriAdi FROM Kategoriler WHERE KategoriID = @kategoriID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@kategoriID", kategoriId);

                var result = command.ExecuteScalar();
                if (result != null)
                {
                    kategoriAdi = result.ToString();
                }
            }

            return kategoriAdi;
        }

    }
}