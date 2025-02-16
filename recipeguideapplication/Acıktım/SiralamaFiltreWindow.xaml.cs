using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Aciktim
{
    public partial class SiralamaFiltreWindow : Window
    {
        public string SiralamaKriteri { get; private set; }
        public string SiralamaYonu { get; private set; }
        public int? MinMalzemeSayisi { get; private set; }
        public int? MaxMalzemeSayisi { get; private set; }
        public decimal? MinMaliyet { get; private set; }
        public decimal? MaxMaliyet { get; private set; }
        public string SecilenKategori { get; private set; } // Seçilen Kategori

        public SiralamaFiltreWindow()
        {
            InitializeComponent();
            LoadCategories(); // Kategorileri yükle
        }

        // Veritabanından kategorileri çeken metod
        private void LoadCategories()
        {
            string connectionString = "Server=DESKTOP-D3P3LRR\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;";
            string query = "SELECT KategoriAdi FROM Kategoriler;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string kategoriAdi = reader.GetString(0);
                        cbKategoriler.Items.Add(kategoriAdi);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Kategoriler yüklenirken hata oluştu: {ex.Message}");
                }
            }
        }

        private void BtnTamam_Click(object sender, RoutedEventArgs e)
        {
            // Sıralama kriterini ve yönünü al
            SiralamaKriteri = (cbSiralamaKriter.SelectedItem as ComboBoxItem)?.Content.ToString();
            SiralamaYonu = (cbSiralamaYonu.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Seçilen kategoriyi al
            SecilenKategori = cbKategoriler.SelectedItem?.ToString();

            // Malzeme sayısı aralığını al ve kontrol et
            if (int.TryParse(txtMinMalzemeSayisi.Text, out int minMalzemeSayisi))
                MinMalzemeSayisi = minMalzemeSayisi;

            if (int.TryParse(txtMaxMalzemeSayisi.Text, out int maxMalzemeSayisi))
                MaxMalzemeSayisi = maxMalzemeSayisi;

            // Maliyet aralığını al ve kontrol et
            if (decimal.TryParse(txtMinMaliyet.Text, out decimal minMaliyet))
                MinMaliyet = minMaliyet;

            if (decimal.TryParse(txtMaxMaliyet.Text, out decimal maxMaliyet))
                MaxMaliyet = maxMaliyet;

            // Pencereyi kapat ve sonucu dön
            DialogResult = true;
            Close();
        }
    }
}
