public class Tarif
{
    public int TarifID { get; set; }
    public string TarifAdi { get; set; }
    public string ResimYolu { get; set; }
    public int HazirlamaSuresi { get; set; }
    public decimal ToplamMaliyet { get; set; } // Toplam maliyet alanı
    public int EslesmeYuzdesi { get; set; }    // Eşleşme yüzdesi alanı
    public int MalzemeSayisi { get; set; }
    public int KategoriID { get; set; } // Kategori ID alanı
}
