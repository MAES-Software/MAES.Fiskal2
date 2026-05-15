namespace MAES.Fiskal2;

public class IzlazniERacun
{
    public Guid Guid { get; set; }
    public string Broj { get; set; } = "";
    public DateTime Datum { get; set; }
    public string PartnerNaziv { get; set; } = "";
    public string PartnerOIB { get; set; } = "";
    public string PartnerAdresa { get; set; } = "";
    public IzlazniERacunStatus Status { get; set; }
}

public enum IzlazniERacunStatus
{
    Nacrt = 10,
    Poslano = 40,
    Greška = 50,
    NemogućnostDostave = 55,
    Dostavljeno = 60,
    Odbijeno = 90,
    DjelomičnoPlaćeno = 100,
    Plaćeno = 110 
}