using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Data.SqlClient;
using System.Windows.Media.Imaging;
using AciktimApp;

namespace Acıktım
{
    public partial class TarifAra : UserControl
    {
        private List<string> selectedMalzemeler = new List<string>(); // Seçilen malzemeleri burada saklayacağız

        public TarifAra()
        {
            InitializeComponent();
            LoadCategories();
            cbKategoriler.SelectionChanged += CbKategoriler_SelectionChanged;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "")
            {
                txtPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void BtnMalzemeSec_Click(object sender, RoutedEventArgs e)
        {
            MalzemeSecWindow malzemeSecWindow = new MalzemeSecWindow();

            if (malzemeSecWindow.ShowDialog() == true)
            {
                selectedMalzemeler = malzemeSecWindow.SelectedMalzemeler;
                if (selectedMalzemeler == null || selectedMalzemeler.Count == 0)
                {
                    MessageBox.Show("Hiç malzeme seçilmedi.");
                }
            }
        }

        private void BtnAra_Click(object sender, RoutedEventArgs e)
        {
            string aramaMetni = txtSearch.Text;
            string selectedKategori = cbKategoriler.SelectedItem?.ToString();
            lstTarifler.Items.Clear();

            var tarifler = GetTarifler(selectedMalzemeler, aramaMetni, selectedKategori);
            ListResults(tarifler);
        }

        private void LoadCategories()
        {
            string connectionString = "Server=DESKTOP\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;TrustServerCertificate=True;";
            List<string> kategoriler = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string kategoriQuery = "SELECT KategoriAdi FROM Kategoriler";
                SqlCommand command = new SqlCommand(kategoriQuery, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    kategoriler.Add(reader.GetString(0));
                }
                reader.Close();
            }

            cbKategoriler.ItemsSource = kategoriler;
        }

        private void ListResults(List<(int TarifID, string TarifAdi, string ResimYolu, int EslesmeYuzdesi)> tarifler)
        {
            lstTarifler.Items.Clear();

            if (tarifler.Count > 0)
            {
                foreach (var (tarifID, tarifAdi, resimYolu, eslesmeYuzdesi) in tarifler)
                {
                    decimal eksikMaliyet = GetEksikMalzemeMaliyeti(tarifID);

                    Image statusImage = new Image
                    {
                        Width = 30,
                        Height = 30,
                        Margin = new Thickness(5),
                        Source = new BitmapImage(new Uri(
                            eksikMaliyet > 0
                                ? "C:\\Users\\Desktop\\tarifresim\\png-clipart-smiley-face-emoticon-sad-miscellaneous-face-thumbnail.png"
                                : "C:\\Users\\Desktop\\tarifresim\\happy-face-emoji-smiley-emoticon-happiness-emotion-green-facial-expression-head-png-clipart.jpg",
                            UriKind.Absolute))
                    };

                    StackPanel panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

                    Image image = new Image
                    {
                        Source = new BitmapImage(new Uri(resimYolu, UriKind.RelativeOrAbsolute)),
                        Width = 100,
                        Height = 100
                    };

                    // Malzeme seçildiyse eşleşme yüzdesini göster, seçilmediyse göstermeden sadece tarif adını yazdır
                    string textContent = selectedMalzemeler != null && selectedMalzemeler.Count > 0
                        ? $"{tarifAdi} - Eşleşme: {eslesmeYuzdesi}% - Eksik Maliyet: {eksikMaliyet:C2}"
                        : $"{tarifAdi} - Eksik Maliyet: {eksikMaliyet:C2}";

                    TextBlock textBlock = new TextBlock
                    {
                        Text = textContent,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 0, 0)
                    };

                    panel.Children.Add(statusImage);
                    panel.Children.Add(image);
                    panel.Children.Add(textBlock);

                    lstTarifler.Items.Add(panel);
                }
            }
            else
            {
                MessageBox.Show("Uygun tarif bulunamadı.");
            }
        }





        private List<(int TarifID, string TarifAdi, string ResimYolu, int EslesmeYuzdesi)> GetTarifler(
            List<string> selectedMalzemeler, string aramaMetni, string selectedKategori = null)
        {
            var tarifler = new List<(int, string, string, int)>();
            string connectionString = "Server=DESKTOP\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;TrustServerCertificate=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Temel SQL sorgusunu oluştur
                string query = @"
            SELECT t.TarifID, t.TarifAdi, t.ResimYolu,
                   COUNT(DISTINCT tm.MalzemeID) AS TarifMalzemeSayisi,
                   SUM(CASE 
                       WHEN ";

                // Malzeme seçildiyse IN ifadesini ekle
                if (selectedMalzemeler != null && selectedMalzemeler.Count > 0)
                {
                    query += "m.MalzemeAdi IN (" + string.Join(", ", selectedMalzemeler.Select((_, i) => $"@malzeme{i}")) + ") THEN 1 ELSE 0 END) AS EslesmeSayisi ";
                }
                else
                {
                    query += "0 = 1 THEN 1 ELSE 0 END) AS EslesmeSayisi ";  // Eğer malzeme seçilmediyse eşleşme 0 olmalı
                }

                query += @"
            FROM Tarifler t
            INNER JOIN TarifMalzeme tm ON t.TarifID = tm.TarifID
            INNER JOIN Malzemeler m ON tm.MalzemeID = m.MalzemeID
            WHERE (1=1)";

                // Arama metni varsa ekle
                if (!string.IsNullOrWhiteSpace(aramaMetni))
                {
                    query += " AND t.TarifAdi LIKE @aramaMetni";
                }

                // Kategori seçildiyse ekle
                if (!string.IsNullOrWhiteSpace(selectedKategori))
                {
                    query += " AND t.KategoriID IN (SELECT KategoriID FROM Kategoriler WHERE KategoriAdi = @kategoriAdi)";
                }

                query += @"
            GROUP BY t.TarifID, t.TarifAdi, t.ResimYolu";

                // Malzeme seçilmişse HAVING ifadesini ekle
                if (selectedMalzemeler != null && selectedMalzemeler.Count > 0)
                {
                    query += @"
            HAVING SUM(CASE 
                        WHEN m.MalzemeAdi IN (" + string.Join(", ", selectedMalzemeler.Select((_, i) => $"@malzeme{i}")) + @") 
                        THEN 1 ELSE 0 END) > 0";
                }

                SqlCommand command = new SqlCommand(query, connection);

                // Parametreleri ekle
                if (selectedMalzemeler != null)
                {
                    for (int i = 0; i < selectedMalzemeler.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@malzeme{i}", selectedMalzemeler[i]);
                    }
                }

                if (!string.IsNullOrWhiteSpace(aramaMetni))
                {
                    command.Parameters.AddWithValue("@aramaMetni", $"%{aramaMetni}%");
                }

                if (!string.IsNullOrWhiteSpace(selectedKategori))
                {
                    command.Parameters.AddWithValue("@kategoriAdi", selectedKategori);
                }

                // Sorguyu çalıştır ve sonuçları oku
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int tarifID = reader.GetInt32(0);
                    string tarifAdi = reader.GetString(1);
                    string resimYolu = reader.GetString(2);
                    int toplamSecilenMalzeme = selectedMalzemeler.Count;
                    int eslesmeSayisi = reader.GetInt32(4);

                    // Eşleşme yüzdesini hesapla
                    int eslesmeYuzdesi = (toplamSecilenMalzeme > 0 && eslesmeSayisi == toplamSecilenMalzeme)
                                        ? 100
                                        : (int)((double)eslesmeSayisi / toplamSecilenMalzeme * 100);

                    tarifler.Add((tarifID, tarifAdi, resimYolu, eslesmeYuzdesi));
                }
                reader.Close();
            }

            // Eşleşme yüzdesine göre sıralama
            return tarifler.OrderByDescending(t => t.Item4).ToList();
        }













        private decimal GetEksikMalzemeMaliyeti(int tarifID)
        {
            decimal toplamMaliyet = 0;
            string connectionString = "Server=DESKTOP\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;TrustServerCertificate=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
            SELECT tm.MalzemeID, tm.MalzemeMiktar, m.ToplamMiktar, m.BirimFiyat
            FROM TarifMalzeme tm
            JOIN Malzemeler m ON tm.MalzemeID = m.MalzemeID
            WHERE tm.TarifID = @tarifID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@tarifID", tarifID);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    // Güvenli dönüşüm için Convert.ToDecimal kullanılıyor
                    decimal tariftekiMiktar = Convert.ToDecimal(reader["MalzemeMiktar"]);
                    decimal buzdolabindakiMiktar = Convert.ToDecimal(reader["ToplamMiktar"]);
                    decimal birimFiyat = Convert.ToDecimal(reader["BirimFiyat"]);

                    decimal eksikMiktar = tariftekiMiktar - buzdolabindakiMiktar;
                    if (eksikMiktar > 0)
                    {
                        toplamMaliyet += eksikMiktar * birimFiyat;
                    }
                }
                reader.Close();
            }

            return toplamMaliyet;
        }

        private void CbKategoriler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kullanıcı bir seçim yaptıysa placeholder'ı gizle, aksi halde göster
            if (cbKategoriler.SelectedIndex >= 0)
            {
                cbPlaceholder.Visibility = Visibility.Collapsed; // Gizle
            }
            else
            {
                cbPlaceholder.Visibility = Visibility.Visible; // Göster
            }
        }
        private void lstTarifler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstTarifler.SelectedItem is StackPanel panel)
            {
                // Çocuk elemanlardan TextBlock'u bul
                var textBlock = panel.Children.OfType<TextBlock>().FirstOrDefault();

                if (textBlock != null)
                {
                    string selectedTarif = textBlock.Text.Split('-')[0].Trim();

                    // Tarif detay sayfasını aç
                    var tarifDetay = new TarifDetay(selectedTarif);
                    var mainWindow = Application.Current.MainWindow as MainWindow;

                    if (mainWindow != null)
                    {
                        mainWindow.MainContent.Content = tarifDetay;
                    }
                    else
                    {
                        MessageBox.Show("Ana pencere bulunamadı.");
                    }
                }
                else
                {
                    MessageBox.Show("Seçilen tarifin bilgileri bulunamadı.");
                }

                // Seçimi temizleyin, böylece her tıklamada olay tetiklenir
                lstTarifler.SelectedItem = null;
            }
        }


    }
}