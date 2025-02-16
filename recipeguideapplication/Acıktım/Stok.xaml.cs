using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Acıktım
{
    public partial class Stok : UserControl
    {
        private string connectionString = "Server=DESKTOP-D3P3LRR\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;TrustServerCertificate=True;";

        public Stok()
        {
            InitializeComponent();
            this.DataContext = new StokViewModel();
            LoadMalzemeler();
        }

        private void BtnMalzemeEkle_Click(object sender, RoutedEventArgs e)
        {
            MalzemeEkleWindow malzemeEkleWindow = new MalzemeEkleWindow();
            bool? result = malzemeEkleWindow.ShowDialog();

            if (result == true)
            {
                LoadMalzemeler(); // Yeni malzeme eklendiğinde DataGrid'i güncelle
            }
        }

        private void LoadMalzemeler()
        {
            List<Malzeme> malzemelerListesi = new List<Malzeme>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(
                    "SELECT TOP (1000) [MalzemeID], [MalzemeAdi], [ToplamMiktar], [MalzemeBirim], [BirimFiyat] " +
                    "FROM [TarifRehberi].[dbo].[Malzemeler]",
                    connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    malzemelerListesi.Add(new Malzeme
                    {
                        MalzemeID = reader.GetInt32(0),
                        MalzemeAdi = reader.GetString(1),
                        ToplamMiktar = reader.GetString(2),
                        MalzemeBirim = reader.GetString(3),
                        BirimFiyat = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4)
                    });
                }
            }

            dgMalzemeler.ItemsSource = malzemelerListesi;
        }
        private void BtnMalzemeSil_Click(object sender, RoutedEventArgs e)
        {
            if (dgMalzemeler.SelectedItem is Malzeme selectedMalzeme)
            {
                DeleteMalzeme(selectedMalzeme);
            }
        }

        private void DeleteMalzeme(Malzeme selectedMalzeme)
        {
            if (selectedMalzeme == null) return;

            // Kullanıcıdan silme işlemi için onay al
            var result = MessageBox.Show(
                $"{selectedMalzeme.MalzemeAdi} malzemesini silmek istediğinizden emin misiniz?",
                "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // TarifMalzeme tablosunda malzemenin kullanılıp kullanılmadığını kontrol et
                    using (SqlCommand checkCommand = new SqlCommand(
                        "SELECT COUNT(*) FROM [TarifRehberi].[dbo].[TarifMalzeme] WHERE MalzemeID = @MalzemeID",
                        connection))
                    {
                        checkCommand.Parameters.AddWithValue("@MalzemeID", selectedMalzeme.MalzemeID);

                        int kullanimSayisi = (int)checkCommand.ExecuteScalar();

                        if (kullanimSayisi > 0)
                        {
                            MessageBox.Show(
                                "Bu malzeme bir tarifte kullanıldığı için silinemez.",
                                "Silme Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Malzeme tariflerde kullanılmıyorsa silme işlemini gerçekleştir
                    using (SqlCommand deleteCommand = new SqlCommand(
                        "DELETE FROM [TarifRehberi].[dbo].[Malzemeler] WHERE MalzemeID = @MalzemeID",
                        connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@MalzemeID", selectedMalzeme.MalzemeID);
                        deleteCommand.ExecuteNonQuery();
                    }

                    MessageBox.Show("Malzeme başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadMalzemeler(); // Silme sonrası DataGrid'i güncelle
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }

    public class Malzeme
    {
        public int MalzemeID { get; set; }
        public string MalzemeAdi { get; set; }
        public string ToplamMiktar { get; set; }
        public string MalzemeBirim { get; set; }
        public decimal BirimFiyat { get; set; }

        public void Update(string malzemeAdi, string toplamMiktar, string malzemeBirim, decimal birimFiyat)
        {
            MalzemeAdi = malzemeAdi;
            ToplamMiktar = toplamMiktar;
            MalzemeBirim = malzemeBirim;
            BirimFiyat = birimFiyat;
        }
    }
}
