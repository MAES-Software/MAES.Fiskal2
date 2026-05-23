namespace MAES.Fiskal2;

/// <summary>
/// Predstavlja ulazni e-račun dobiven od posrednika.
/// </summary>
public class UlazniERacun
{
    /// <summary>
    /// Jedinstveni identifikator računa u sustavu posrednika.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Redni broj računa.
    /// </summary>
    public string Broj { get; set; } = "";

    /// <summary>
    /// Datum izdavanja računa.
    /// </summary>
    public DateTime Datum { get; set; }

    /// <summary>
    /// Naziv partnera koji je izdao račun.
    /// </summary>
    public string Partner { get; set; } = "";

    /// <summary>
    /// OIB partnera koji je izdao račun.
    /// </summary>
    public string PartnerOIB { get; set; } = "";

    /// <summary>
    /// Adresa partnera koji je izdao račun.
    /// </summary>
    public string PartnerAdresa { get; set; } = "";

    /// <summary>
    /// Trenutni status ulaznog računa.
    /// </summary>
    public UlazniERacunStatus Status { get; set; }
}

/// <summary>
/// Moguća stanja ulaznog e-računa u procesu fiskalizacije.
/// </summary>
public enum UlazniERacunStatus
{
    /// <summary>
    /// Račun je zaprimljen od posrednika, ali još nije obrađen.
    /// </summary>
    Zaprimljeno = 10,
    
    /// <summary>
    /// Račun je odobren.
    /// </summary>
    Odobreno = 30,

    /// <summary>
    /// Račun je odbijen.
    /// </summary>
    Odbijeno = 40,

    /// <summary>
    /// Račun je likvidiran.
    /// </summary>
    Likvidirano = 50 
}

/// <summary>
/// Mogući razlozi odbijanja ulaznog računa u procesu fiskalizacije.
/// </summary>
public enum RazlogOdbijanja
{
    /// <summary>
    /// Neusklađenost koja ne utječe na obračun poreza.
    /// </summary>
    NeusklađenostKojaNeUtjeceNaObracunPoreza = 1,

    /// <summary>
    /// Neusklađenost koja utječe na obračun poreza.
    /// </summary>
    NeusklađenostKojaUtjeceNaObracunPoreza = 2,

    /// <summary>
    /// Ostalo.
    /// </summary>
    Ostalo = 11
}