namespace MAES.Fiskal2;

/// <summary>
/// Predstavlja izlazni e-račun koji je poslан posredniku.
/// </summary>
public class IzlazniERacun
{
    /// <summary>
    /// Jedinstveni identifikator računa u sustavu posrednika.
    /// </summary>
    public Guid Guid { get; set; }

    /// <summary>
    /// Redni broj računa.
    /// </summary>
    public string Broj { get; set; } = "";

    /// <summary>
    /// Datum izdavanja računa.
    /// </summary>
    public DateTime Datum { get; set; }

    /// <summary>
    /// Naziv primatelja računa.
    /// </summary>
    public string PartnerNaziv { get; set; } = "";

    /// <summary>
    /// OIB primatelja računa.
    /// </summary>
    public string PartnerOIB { get; set; } = "";

    /// <summary>
    /// Adresa primatelja računa.
    /// Primjer: 'Ulica 10, 10000 Zagreb'
    /// Format: {ulica} {broj}, {poštanski broj} {grad}
    /// </summary>
    public string PartnerAdresa { get; set; } = "";

    /// <summary>
    /// Trenutni status izlaznog računa.
    /// </summary>
    public IzlazniERacunStatus Status { get; set; }
}

/// <summary>
/// Moguća stanja izlaznog e-računa u procesu fiskalizacije.
/// </summary>
public enum IzlazniERacunStatus
{
    /// <summary>
    /// Račun je kreiran, ali još nije poslan posredniku.
    /// </summary>
    Nacrt = 10,

    /// <summary>
    /// Račun je poslan posredniku, ali još nije obrađen.
    /// </summary>
    Poslano = 40,

    /// <summary>
    /// Račun je poslan posredniku, ali je došlo do greške.
    /// </summary>
    Greška = 50,

    /// <summary>
    /// Račun je poslan posredniku, ali nije moguće dostaviti.
    /// </summary>
    NemogućnostDostave = 55,

    /// <summary>
    /// Račun je poslan posredniku i dostavljen.
    /// </summary>
    Dostavljeno = 60,

    /// <summary>
    /// Račun je poslan posredniku, ali je odbijen.
    /// </summary>
    Odbijeno = 90,

    /// <summary>
    /// Račun je poslan posredniku, ali je djelomično plaćen.
    /// </summary>
    DjelomičnoPlaćeno = 100,

    /// <summary> 
    /// Račun je poslan posredniku i u potpunosti plaćen.
    /// </summary>
    Plaćeno = 110 
}

/// <summary>
/// Mogući načini plaćanja za evidentiranje uplate izlaznog računa.
/// </summary>
public enum NacinPlacanja
{
    /// <summary>
    /// Transakcijski račun.
    /// </summary>
    TransakcijskiRaCun = 1,

    /// <summary>
    /// Obračunsko plaćanje.
    /// </summary>
    ObračunskoPlaćanje = 2,

    /// <summary>
    /// Ostalo.
    /// </summary>
    Ostalo = 11
}