using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace AciktimApp
{
    public partial class MalzemeSecWindow : Window
    {
        public List<string> SelectedMalzemeler { get; private set; }

        public MalzemeSecWindow()
        {
            InitializeComponent();
            LoadMalzemeler();
        }

        private void LoadMalzemeler()
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
                    lstMalzemeler.Items.Add(reader["MalzemeAdi"].ToString());
                }
            }
        }

        private void OnTamamClicked(object sender, RoutedEventArgs e)
        {
            SelectedMalzemeler = lstMalzemeler.SelectedItems.Cast<string>().ToList();
            this.DialogResult = true;
            this.Close();
        }
    }
}