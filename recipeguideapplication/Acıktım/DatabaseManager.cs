using System.Data.SqlClient;
using System;
using System.Windows;
using System.Configuration;


namespace Acıktım
{
    public class DatabaseManager
    {
        private string _connectionString;


        public DatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection OpenConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public void InitializeDatabase()
        {
            using (SqlConnection connection = OpenConnection())
            {
                try
                {
                    connection.Open();

                    // Kategoriler tablosunu oluştur
                    string createKategorilerTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Kategoriler]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE Kategoriler (
                            KategoriID INT PRIMARY KEY IDENTITY(1,1),
                            KategoriAdi VARCHAR(150) NOT NULL,
                            Aciklama VARCHAR(MAX)
                        );
                
                        INSERT INTO Kategoriler (KategoriAdi) VALUES 
                        ('Ana Yemek'), 
                        ('Çorba'), 
                        ('Salata ve Meze'), 
                        ('Tatlı'), 
                        ('İçecek');
                    END;";

                    string createTariflerTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tarifler]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE Tarifler (
                            TarifID INT PRIMARY KEY IDENTITY(1,1),
                            TarifAdi VARCHAR(150) NOT NULL,
                            KategoriID INT NOT NULL,
                            HazirlamaSuresi INT NOT NULL,
                            Talimatlar TEXT NOT NULL,
                            ResimYolu VARCHAR(500),
                            FOREIGN KEY (KategoriID) REFERENCES Kategoriler(KategoriID)
                        );
                    END;";

                    string createMalzemelerTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Malzemeler]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE Malzemeler (
                            MalzemeID INT PRIMARY KEY IDENTITY(1,1),
                            MalzemeAdi VARCHAR(150) NOT NULL,
                            ToplamMiktar VARCHAR(50) NOT NULL,
                            MalzemeBirim VARCHAR(50) NOT NULL,
                            BirimFiyat FLOAT NOT NULL
                        );
                    END;";

                    string createTarifMalzemeTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TarifMalzeme]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE TarifMalzeme (
                            TarifID INT NOT NULL,
                            MalzemeID INT NOT NULL,
                            MalzemeMiktar FLOAT NOT NULL,
                            PRIMARY KEY (TarifID, MalzemeID),
                            FOREIGN KEY (TarifID) REFERENCES Tarifler(TarifID),
                            FOREIGN KEY (MalzemeID) REFERENCES Malzemeler(MalzemeID)
                        );
                    END;";

                    ExecuteSqlQuery(connection, createKategorilerTable);  // Kategoriler tablosunu oluştur
                    ExecuteSqlQuery(connection, createTariflerTable);     // Tarifler tablosunu oluştur
                    ExecuteSqlQuery(connection, createMalzemelerTable);   // Malzemeler tablosunu oluştur
                    ExecuteSqlQuery(connection, createTarifMalzemeTable); // Tarif-Malzeme ilişki tablosunu oluştur
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Veritabanı işlemi sırasında hata oluştu: {ex.Message}\n{ex.StackTrace}");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void ExecuteSqlQuery(SqlConnection connection, string query)
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }


    }
}