using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Acıktım
{
    public partial class AnaSayfa : UserControl
    {
        private DispatcherTimer timer; // Tarifleri belirli aralıklarla güncellemek için

        public AnaSayfa()
        {
            InitializeComponent();
            LoadRandomTarifler(); // Başlangıçta tarifleri yükle
            SetupTimer(); // Zamanlayıcı kur
        }

        // Timer ayarları
        private void SetupTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10); // 10 saniyede bir güncelle
            timer.Tick += (s, e) => LoadRandomTarifler(); // Timer tetiklendiğinde tarifleri güncelle
            timer.Start();
        }

        private void lstTarifler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if an item is selected
            if (lstTarifler.SelectedItem is Tarif selectedTarif)
            {
                // Open the TarifDetay page with the selected recipe's name
                TarifDetay detayPage = new TarifDetay(selectedTarif.TarifAdi);

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    // Display the UserControl inside the MainContent ContentControl
                    mainWindow.MainContent.Content = detayPage;
                }
            }
        }




        // Veritabanından rastgele tarifleri getir
        private void LoadRandomTarifler()
        {
            List<Tarif> tarifler = new List<Tarif>();

            string connectionString = "Server=DESKTOP\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;";
            string query = "SELECT TOP 8 [TarifAdi], [ResimYolu] FROM [TarifRehberi].[dbo].[Tarifler] ORDER BY NEWID()";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        tarifler.Add(new Tarif
                        {
                            TarifAdi = reader["TarifAdi"].ToString(),
                            ResimYolu = reader["ResimYolu"].ToString() // Resim yolu
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Hata mesajı yönetimi
                    MessageBox.Show($"Tarifler yüklenirken bir hata oluştu: {ex.Message}");
                }
            }

            // Tarifleri ListBox'a bağla
            lstTarifler.ItemsSource = tarifler;
        }

        // Tarif model sınıfı
        public class Tarif
        {
            public string TarifAdi { get; set; }
            public string ResimYolu { get; set; } // Resim yolu (gösterim için)
        }
    }
}
