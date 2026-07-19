namespace MAES.Fiskal2;

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