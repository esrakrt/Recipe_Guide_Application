using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using System.Data.SqlClient;
using Aciktim;


namespace Acıktım
{
    public partial class TarifListe : UserControl
    {
        public ObservableCollection<Tarif> Recipes { get; set; } = new ObservableCollection<Tarif>();

        public TarifListe()
        {
            InitializeComponent();
            Recipes = TarifService.GetTarifler();
            LoadRecipes();

            // Add event handler for recipe selection
            lvTarifler.SelectionChanged += LvTarifler_SelectionChanged;
        }


        private void BtnSirala_Click(object sender, RoutedEventArgs e)
        {
            SiralamaFiltreWindow siralamaFiltreWindow = new SiralamaFiltreWindow();

            if (siralamaFiltreWindow.ShowDialog() == true)
            {
                string siralamaKriteri = siralamaFiltreWindow.SiralamaKriteri;
                string siralamaYonu = siralamaFiltreWindow.SiralamaYonu;
                int? minMalzemeSayisi = siralamaFiltreWindow.MinMalzemeSayisi;
                int? maxMalzemeSayisi = siralamaFiltreWindow.MaxMalzemeSayisi;
                decimal? minMaliyet = siralamaFiltreWindow.MinMaliyet;
                decimal? maxMaliyet = siralamaFiltreWindow.MaxMaliyet;
                string secilenKategori = siralamaFiltreWindow.SecilenKategori;

                UygulaSiralamaVeFiltreleme(
                    siralamaKriteri, siralamaYonu, minMalzemeSayisi, maxMalzemeSayisi,
                    minMaliyet, maxMaliyet, secilenKategori);
            }
        }



        private void UygulaSiralamaVeFiltreleme(
            string siralamaKriteri, string siralamaYonu, int? minMalzemeSayisi, int? maxMalzemeSayisi,
            decimal? minMaliyet, decimal? maxMaliyet, string secilenKategori)
        {
            // Tarifleri kategoriye göre al
            var filteredRecipes = TarifService.GetTarifler(secilenKategori);

            // Filtreleme işlemi
            filteredRecipes = new ObservableCollection<Tarif>(filteredRecipes.Where(r =>
                (!minMalzemeSayisi.HasValue || r.MalzemeSayisi >= minMalzemeSayisi.Value) &&
                (!maxMalzemeSayisi.HasValue || r.MalzemeSayisi <= maxMalzemeSayisi.Value) &&
                (!minMaliyet.HasValue || r.ToplamMaliyet >= minMaliyet.Value) &&
                (!maxMaliyet.HasValue || r.ToplamMaliyet <= maxMaliyet.Value)));

            // Sıralama işlemi
            IOrderedEnumerable<Tarif> sortedRecipes = siralamaKriteri switch
            {
                "Hazırlama Süresi" => siralamaYonu == "Artan"
                    ? filteredRecipes.OrderBy(r => r.HazirlamaSuresi)
                    : filteredRecipes.OrderByDescending(r => r.HazirlamaSuresi),
                "Maliyet" => siralamaYonu == "Artan"
                    ? filteredRecipes.OrderBy(r => r.ToplamMaliyet)
                    : filteredRecipes.OrderByDescending(r => r.ToplamMaliyet),
                _ => filteredRecipes.OrderBy(r => r.TarifID) // Varsayılan sıralama
            };

            // Sonuçları güncelle
            lvTarifler.ItemsSource = new ObservableCollection<Tarif>(sortedRecipes);

            MessageBox.Show("Sıralama ve filtreleme işlemi tamamlandı.");
        }







        private void LvTarifler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if an item is selected
            if (lvTarifler.SelectedItem is Tarif selectedTarif)
            {
                // Navigate to the TarifDetay page, passing the selected recipe name
                TarifDetay detayPage = new TarifDetay(selectedTarif.TarifAdi);

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    // Display the UserControl inside the MainContent ContentControl
                    mainWindow.MainContent.Content = detayPage;
                }
            }
        }


        private void LoadRecipes(string selectedCategory = null)
        {


            lvTarifler.ItemsSource = Recipes;
        }




    }
}