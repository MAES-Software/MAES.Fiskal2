namespace MAES.Fiskal2;

public class UlazniERacun
{
    public Guid Guid { get; set; }
    public string Broj { get; set; } = "";
    public DateTime Datum { get; set; }
    public string Partner { get; set; } = "";
    public string PartnerOIB { get; set; } = "";
    public string PartnerAdresa { get; set; } = "";
    public UlazniERacunStatus Status { get; set; }
}

public enum UlazniERacunStatus
{
    Zaprimljeno = 10,
    Odobreno = 30,
    Odbijeno = 40,
    Likvidirano = 50 
}