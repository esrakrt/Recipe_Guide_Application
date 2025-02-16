    using System.Data.SqlClient;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    namespace Acıktım
    {
        public partial class MainWindow : Window
        {
            // Veritabanı bağlantı dizesi, sınıfın alanı olarak tanımlanıyor
            private string connectionString = "Server=DESKTOP-D3P3LRR\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;TrustServerCertificate=True;";

            public MainWindow()
            {
                InitializeComponent();


                
            }
        private void btnTumTarifler_Click(object sender, RoutedEventArgs e)
        {
            // "Tüm Tarifler" kategorisi için tarifleri yükle
            LoadRecipes("Tüm Tarifler");
        }




        private void LoadRecipes(string category)
            {
                // Tarifleri tutacak bir sayfa oluştur
                TarifListe tarifListePage = new TarifListe();

                // Veritabanından tarifleri al
                string query;

                // "Tüm Tarifler" seçeneği için tüm tarifleri al
                if (category == "Tüm Tarifler")
                {
                    query = "SELECT TarifID, TarifAdi, ResimYolu, HazirlamaSuresi FROM Tarifler"; // Tüm tarifleri al
                }
                else
                {
                    query = @"
                SELECT Tarifler.TarifID, Tarifler.TarifAdi, Tarifler.ResimYolu, Tarifler.HazirlamaSuresi 
                FROM Tarifler 
                INNER JOIN Kategoriler ON Tarifler.KategoriID = Kategoriler.KategoriID 
                WHERE Kategoriler.KategoriAdi = @Kategori"; // Kategori adını kullan
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);

                    // Eğer kategori "Tüm Tarifler" değilse, kategori parametresini ekleyin
                    if (category != "Tüm Tarifler")
                    {
                        command.Parameters.AddWithValue("@Kategori", category);
                    }

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        // Tarifleri yükleyin
                        while (reader.Read())
                        {
                            // Yeni tarif nesnesi oluşturun
                            Tarif tarif = new Tarif
                            {
                                TarifID = (int)reader["TarifID"], // TarifID'yi ekleyin
                                TarifAdi = reader["TarifAdi"].ToString(),
                                ResimYolu = reader["ResimYolu"].ToString(),
                                HazirlamaSuresi = (int)reader["HazirlamaSuresi"] // Hazırlama süresini ekleyin
                            };

                            // Toplam maliyeti yükle
                            tarif.ToplamMaliyet = LoadMalzemeler(tarif.TarifID);

                            // Tarif listesini ekleyin
                            tarifListePage.Recipes.Add(tarif);
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"Tarifleri yüklerken hata oluştu: {ex.Message}");
                    }
                }

                // Tarif listesini içerik kontrolüne yükle
                MainContent.Content = tarifListePage;
            }

            private decimal LoadMalzemeler(int tarifId)
            {
                Dictionary<int, (string MalzemeAdi, string ToplamMiktar, string MalzemeBirim, decimal BirimFiyat)> malzemeDict = new Dictionary<int, (string, string, string, decimal)>();
                decimal toplamMaliyet = 0; // Toplam maliyet için bir değişken

                string connectionString = "Server=DESKTOP-D3P3LRR\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;";

                // Malzeme bilgilerini ve birim fiyatlarını almak için sorgu
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
                            decimal birimFiyat = malzemeReader.GetDecimal(4); // Birim fiyat

                            malzemeDict[malzemeId] = (malzemeAdi, toplamMiktar, malzemeBirim, birimFiyat);
                        }
                    }
                }

                // Tarif malzemelerini almak ve maliyet hesaplamak için sorgu
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
                                string malzemeAdi = malzemeBilgi.MalzemeAdi;
                                string buzdolabindakiMiktar = malzemeBilgi.ToplamMiktar;
                                string malzemeBirim = malzemeBilgi.MalzemeBirim;
                                decimal birimFiyat = malzemeBilgi.BirimFiyat;

                                // Miktarları karşılaştır
                                decimal buzdolabindakiMiktarDecimal = decimal.Parse(buzdolabindakiMiktar);
                                decimal tarifMiktarDecimal = decimal.Parse(tarifMiktar);

                                // Malzemenin maliyetini hesapla
                                decimal malzemeMaliyeti = tarifMiktarDecimal * birimFiyat;
                                toplamMaliyet += malzemeMaliyeti;

                                // Maliyet ve miktar bilgilerini göster (bu kısmı ihtiyaçlarınıza göre düzenleyebilirsiniz)
                                TextBlock malzemeTextBlock = new TextBlock
                                {
                                    Text = $"Malzeme: {malzemeAdi}, Gerekli: {tarifMiktar} {malzemeBirim}, Maliyet: {malzemeMaliyeti:C}"
                                };


                            }
                        }
                    }
                }

                // Toplam maliyeti döndür
                return toplamMaliyet;
            }





            private void LoadAnaSayfa()
            {
                AnaSayfa anaSayfa = new AnaSayfa();
                MainContent.Content = anaSayfa; // AnaSayfa'yı içeriğe yükle
            }
            private void btnAnaSayfa_Click(object sender, RoutedEventArgs e)
            {
                // Ana sayfayı göster
                MainContent.Content = new AnaSayfa();
            }
            private void btnTarifEkle_Click(object sender, RoutedEventArgs e)
            {
                // TarifEkle sayfasını oluştur ve yükle
                TarifEkle tarifEklePage = new TarifEkle();
                MainContent.Content = tarifEklePage; // İçeriği değiştir
            }
            private void btnStok_Click(object sender, RoutedEventArgs e)
            {
                // Stok sayfasını oluştur ve içeriği değiştir
                Stok stokPage = new Stok();
                MainContent.Content = stokPage;
            }

            private void btnTarifAra_Click(object sender, RoutedEventArgs e)
            {
                // TarifAra sayfasını oluştur ve yükle
                TarifAra tarifAraPage = new TarifAra();
                MainContent.Content = tarifAraPage; // İçeriği değiştir
            }
        }
    }