using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;

namespace Aciktim
{
    public static class TarifService
    {
        private const string ConnectionString =
            "Server=DESKTOP\\SQLEXPRESS;Database=TarifRehberi;Trusted_Connection=True;";

        public static ObservableCollection<Tarif> GetTarifler(string kategoriAdi = null)
        {
            var tarifler = new ObservableCollection<Tarif>();
            string query;

            if (string.IsNullOrEmpty(kategoriAdi) || kategoriAdi == "Tüm Tarifler")
            {
                query = @"
                SELECT t.TarifID, t.TarifAdi, t.ResimYolu, t.HazirlamaSuresi,
                       CAST(ISNULL(SUM(tm.MalzemeMiktar * m.BirimFiyat), 0) AS DECIMAL(18,2)) AS ToplamMaliyet,
                       COUNT(tm.MalzemeID) AS MalzemeSayisi, t.KategoriID
                FROM Tarifler t
                LEFT JOIN TarifMalzeme tm ON t.TarifID = tm.TarifID
                LEFT JOIN Malzemeler m ON tm.MalzemeID = m.MalzemeID
                GROUP BY t.TarifID, t.TarifAdi, t.ResimYolu, t.HazirlamaSuresi, t.KategoriID;";
            }
            else
            {
                query = @"
                SELECT t.TarifID, t.TarifAdi, t.ResimYolu, t.HazirlamaSuresi,
                       CAST(ISNULL(SUM(tm.MalzemeMiktar * m.BirimFiyat), 0) AS DECIMAL(18,2)) AS ToplamMaliyet,
                       COUNT(tm.MalzemeID) AS MalzemeSayisi, t.KategoriID
                FROM Tarifler t
                INNER JOIN Kategoriler k ON t.KategoriID = k.KategoriID
                LEFT JOIN TarifMalzeme tm ON t.TarifID = tm.TarifID
                LEFT JOIN Malzemeler m ON tm.MalzemeID = m.MalzemeID
                WHERE k.KategoriAdi = @KategoriAdi
                GROUP BY t.TarifID, t.TarifAdi, t.ResimYolu, t.HazirlamaSuresi, t.KategoriID;";
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                if (!string.IsNullOrEmpty(kategoriAdi) && kategoriAdi != "Tüm Tarifler")
                {
                    command.Parameters.AddWithValue("@KategoriAdi", kategoriAdi);
                }

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var tarif = new Tarif
                        {
                            TarifID = reader.GetInt32(0),
                            TarifAdi = reader.GetString(1),
                            ResimYolu = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            HazirlamaSuresi = reader.GetInt32(3),
                            ToplamMaliyet = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                            MalzemeSayisi = reader.GetInt32(5),
                            KategoriID = reader.GetInt32(6)
                        };

                        tarifler.Add(tarif);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Veritabanı hatası: {ex.Message}");
                }
            }

            return tarifler;
        }
    }
}
