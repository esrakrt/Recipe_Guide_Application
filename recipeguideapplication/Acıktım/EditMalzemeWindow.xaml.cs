using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Acıktım
{
    /// <summary>
    /// EditMalzemeWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class EditMalzemeWindow : Window
    {
        private Malzeme _malzeme;

        public EditMalzemeWindow(Malzeme malzeme)
        {
            InitializeComponent();
            _malzeme = malzeme;

            // Mevcut malzeme bilgilerini textbox'lara yerleştirin
            txtMalzemeAdi.Text = malzeme.MalzemeAdi;
            txtToplamMiktar.Text = malzeme.ToplamMiktar;
            txtMalzemeBirim.Text = malzeme.MalzemeBirim;
            txtBirimFiyat.Text = malzeme.BirimFiyat.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Güncellenen değerleri al
            string malzemeAdi = txtMalzemeAdi.Text;
            string toplamMiktar = txtToplamMiktar.Text;
            string malzemeBirim = txtMalzemeBirim.Text;
            decimal birimFiyat = decimal.Parse(txtBirimFiyat.Text);

            // Malzemeyi güncelle
            _malzeme.Update(malzemeAdi, toplamMiktar, malzemeBirim, birimFiyat);

            // Pencereyi kapat
            DialogResult = true;
            Close();
        }
    }
}