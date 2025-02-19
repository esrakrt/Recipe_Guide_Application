using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Acıktım
{
    public partial class MalzemeEklePopup : Window
    {
        public List<(string, string)> EklenenMalzemeler { get; private set; } = new List<(string, string)>();

        public MalzemeEklePopup()
        {
            InitializeComponent();
            VeritabanindanMalzemeleriGetir();
        }

        private void VeritabanindanMalzemeleriGetir()
        {
            string connectionString = "Server=DESKTOP\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;TrustServerCertificate=True;";
            string query = "SELECT MalzemeAdi FROM Malzemeler";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string malzemeAdi = reader["MalzemeAdi"].ToString();
                    cmbMalzemeler.Items.Add(malzemeAdi);  // ComboBox'a malzemeleri ekleme
                }
                reader.Close();
            }
        }

        private void BtnEkleMalzeme_Click(object sender, RoutedEventArgs e)
        {
            string malzemeAdi;
            string malzemeMiktari = txtMalzemeMiktari.Text;

            if (cmbMalzemeler.SelectedItem != null)  // Eğer ComboBox'tan seçim yapıldıysa
            {
                malzemeAdi = cmbMalzemeler.SelectedItem.ToString();
            }
            else  // Eğer yeni malzeme girildiyse
            {
                malzemeAdi = txtMalzemeAdi.Text;
            }

            if (!string.IsNullOrWhiteSpace(malzemeAdi) && !string.IsNullOrWhiteSpace(malzemeMiktari))
            {
                TextBlock yeniMalzemeTextBlock = new TextBlock
                {
                    Text = $"{malzemeAdi} - {malzemeMiktari}",
                    FontSize = 14,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                EklenenMalzemelerPanel.Children.Add(yeniMalzemeTextBlock);

                EklenenMalzemeler.Add((malzemeAdi, malzemeMiktari));

                txtMalzemeAdi.Clear();
                txtMalzemeMiktari.Clear();
                cmbMalzemeler.SelectedItem = null;
            }
            else
            {
                MessageBox.Show("Malzeme adı ve miktarı boş olamaz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnTamam_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
