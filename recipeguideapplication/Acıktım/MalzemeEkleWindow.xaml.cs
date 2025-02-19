using System;
using System.Data.SqlClient;
using System.Windows;

namespace Acıktım
{
    public partial class MalzemeEkleWindow : Window
    {
        private string connectionString = "Server=DESKTOP\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;TrustServerCertificate=True;";

        public MalzemeEkleWindow()
        {
            InitializeComponent();
        }

        private void Kaydet_Click(object sender, RoutedEventArgs e)
        {
            // Kullanıcının girdiği malzeme bilgilerini al
            string malzemeAdi = txtMalzemeAdi.Text;
            string toplamMiktar = txtToplamMiktar.Text;
            string malzemeBirim = txtMalzemeBirim.Text;
            decimal birimFiyat;

            if (!decimal.TryParse(txtBirimFiyat.Text, out birimFiyat))
            {
                MessageBox.Show("Birim fiyat geçerli bir sayı olmalıdır.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Malzemenin veritabanında olup olmadığını kontrol et
                SqlCommand kontrolCommand = new SqlCommand(
                    "SELECT COUNT(1) FROM [TarifRehberi].[dbo].[Malzemeler] WHERE MalzemeAdi = @MalzemeAdi", connection);
                kontrolCommand.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);

                int mevcutMu = (int)kontrolCommand.ExecuteScalar();

                if (mevcutMu > 0)
                {
                    MessageBox.Show("Bu malzeme zaten veritabanında mevcut.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Malzeme veritabanında yoksa ekleme işlemi yap
                SqlCommand ekleCommand = new SqlCommand(
                    "INSERT INTO [TarifRehberi].[dbo].[Malzemeler] (MalzemeAdi, ToplamMiktar, MalzemeBirim, BirimFiyat) " +
                    "VALUES (@MalzemeAdi, @ToplamMiktar, @MalzemeBirim, @BirimFiyat)", connection);

                ekleCommand.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);
                ekleCommand.Parameters.AddWithValue("@ToplamMiktar", toplamMiktar);
                ekleCommand.Parameters.AddWithValue("@MalzemeBirim", malzemeBirim);
                ekleCommand.Parameters.AddWithValue("@BirimFiyat", birimFiyat);

                try
                {
                    ekleCommand.ExecuteNonQuery();
                    MessageBox.Show("Malzeme başarıyla eklendi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; // Başarıyla eklendiği için pencereyi kapat
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Malzeme eklenirken bir hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
