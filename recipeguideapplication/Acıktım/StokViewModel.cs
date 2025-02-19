using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace Acıktım
{
    public class StokViewModel
    {
        private string connectionString = "Server=DESKTOP\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;TrustServerCertificate=True;";

        public ObservableCollection<Malzeme> Malzemeler { get; set; }

        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public StokViewModel()
        {
            Malzemeler = new ObservableCollection<Malzeme>();
            DeleteCommand = new RelayCommand<Malzeme>(DeleteMalzeme);
            EditCommand = new RelayCommand<Malzeme>(EditMalzeme); // EditCommand'ı burada tanımlayın
            LoadMalzemeler();
        }

        private void LoadMalzemeler()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT TOP (1000) [MalzemeID], [MalzemeAdi], [ToplamMiktar], [MalzemeBirim], [BirimFiyat] FROM [TarifRehberi].[dbo].[Malzemeler]", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Malzemeler.Add(new Malzeme
                    {
                        MalzemeID = reader.GetInt32(0),
                        MalzemeAdi = reader.GetString(1),
                        ToplamMiktar = reader.GetString(2),
                        MalzemeBirim = reader.GetString(3),
                        BirimFiyat = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4)
                    });
                }
            }
        }

        private void DeleteMalzeme(Malzeme malzeme)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Malzemenin tariflerde kullanılıp kullanılmadığını kontrol et
                SqlCommand checkCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM [TarifRehberi].[dbo].[TarifMalzeme] WHERE [MalzemeID] = @MalzemeID",
                    connection);
                checkCommand.Parameters.AddWithValue("@MalzemeID", malzeme.MalzemeID);

                int kullanimSayisi = (int)checkCommand.ExecuteScalar();

                if (kullanimSayisi > 0)
                {
                    // Malzeme tariflerde kullanılıyorsa silme işlemini durdur
                    System.Windows.MessageBox.Show(
                        "Bu malzeme bir tarifte kullanıldığı için silinemez.",
                        "Silme Hatası",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Tariflerde kullanılmıyorsa silme işlemini gerçekleştir
                SqlCommand deleteCommand = new SqlCommand(
                    "DELETE FROM [TarifRehberi].[dbo].[Malzemeler] WHERE [MalzemeID] = @MalzemeID",
                    connection);
                deleteCommand.Parameters.AddWithValue("@MalzemeID", malzeme.MalzemeID);
                deleteCommand.ExecuteNonQuery();

                // ObservableCollection'dan kaldır
                Malzemeler.Remove(malzeme);

                System.Windows.MessageBox.Show(
                    "Malzeme başarıyla silindi.",
                    "Başarılı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }



        private void EditMalzeme(Malzeme malzeme)
        {
            // Pop-up penceresini açın
            EditMalzemeWindow editWindow = new EditMalzemeWindow(malzeme);
            if (editWindow.ShowDialog() == true)
            {
                // Malzeme güncellemeleri varsa veritabanında güncelleyin
                UpdateMalzemeInDatabase(malzeme);
            }
        }

        private void UpdateMalzemeInDatabase(Malzeme malzeme)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("UPDATE [TarifRehberi].[dbo].[Malzemeler] SET [MalzemeAdi] = @MalzemeAdi, [ToplamMiktar] = @ToplamMiktar, [MalzemeBirim] = @MalzemeBirim, [BirimFiyat] = @BirimFiyat WHERE [MalzemeID] = @MalzemeID", connection);
                command.Parameters.AddWithValue("@MalzemeID", malzeme.MalzemeID);
                command.Parameters.AddWithValue("@MalzemeAdi", malzeme.MalzemeAdi);
                command.Parameters.AddWithValue("@ToplamMiktar", malzeme.ToplamMiktar);
                command.Parameters.AddWithValue("@MalzemeBirim", malzeme.MalzemeBirim);
                command.Parameters.AddWithValue("@BirimFiyat", malzeme.BirimFiyat);
                command.ExecuteNonQuery();
            }
        }


    }

    public class RelayCommand<T> : ICommand
    {
        private Action<T> _execute;
        private Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);

        public void Execute(object parameter) => _execute((T)parameter);
    }
}
