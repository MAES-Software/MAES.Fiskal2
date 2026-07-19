namespace MAES.Fiskal2;

/// <summary>
/// Abstraktna klasa koja je baza za e-račune
/// </summary>
public abstract class ERacun
{

#region Osnovno

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
    /// Poziv na broj koji piše na uplati u banci
    /// </summary>
    public string PozivNaBroj { get; set; } = "";

#endregion

#region Subjekt

    /// <summary>
    /// Naziv subjekta računa.
    /// Izdavatelj u slučaju ulaznog računa, Primatelj u slučaju izlaznog računa.
    /// </summary>
    public string SubjektNaziv { get; set; } = "";

    /// <summary>
    /// OIB subjekta računa.
    /// Izdavatelj u slučaju ulaznog računa, Primatelj u slučaju izlaznog računa.
    /// </summary>
    public string SubjektOIB { get; set; } = "";

    /// <summary>
    /// adresa subjekta računa.
    /// Izdavatelj u slučaju ulaznog računa, Primatelj u slučaju izlaznog računa.
    /// </summary>
    public string SubjektAdresa { get; set; } = "";

    /// <summary>
    /// Poštanski broj subjekta računa.
    /// Izdavatelj u slučaju ulaznog računa, Primatelj u slučaju izlaznog računa.
    /// </summary>
    public string SubjektPostanskiBroj { get; set; } = "";

    /// <summary>
    /// Mjesto subjekta računa.
    /// Izdavatelj u slučaju ulaznog računa, Primatelj u slučaju izlaznog računa.
    /// </summary>
    public string SubjektMjesto { get; set; } = "";

#endregion

#region Iznosi

    /// <summary>
    /// Ukupan iznos računa.
    /// </summary>
    public double Iznos { get; set; }

    /// <summary>
    /// Iznos koji je već podmiren (preplaćeni ili unaprijed plaćeni iznos).
    /// </summary>
    public double Preplaćeno { get; set; }

    /// <summary>
    /// Preostali iznos za plaćanje.
    /// </summary>
    public double ZaPlatiti => Iznos - Preplaćeno;

#endregion

    /// <summary>
    /// Metoda koja dohvati UBL e-računa
    /// </summary>
    /// <param name="token">Token za otkazivanej</param>
    /// <returns>UBL XML sadržaj računa</returns>
    public abstract Task<string> DohvatiUBLAsync(CancellationToken token);

    /// <summary>
    /// Metoda koja dohvati pdf e-računa
    /// </summary>
    /// <param name="token">Token za otkazivanej</param>
    /// <returns>Sadržaj PDF dokumenta kao niz bajtova.</returns>
    public abstract Task<byte[]> DohvatiPdfAsync(CancellationToken token);
}

/// <summary>
/// Predstavlja ulazni e-račun dobiven od posrednika.
/// </summary>
public class UlazniERacun(Posrednik posrednik) : ERacun
{
    /// <summary>
    /// Trenutni status ulaznog računa.
    /// </summary>
    public UlazniERacunStatus Status { get; set; }

    /// <summary>
    /// Metoda koja dohvati UBL ulaznog e-računa
    /// </summary>
    /// <param name="token">Token za otkazivanej</param>
    /// <returns>UBL XML sadržaj računa</returns>
    public override async Task<string> DohvatiUBLAsync(CancellationToken token = default) => await posrednik.UlazniUBLAsync(Id, token);

    /// <summary>
    /// Metoda koja dohvati Pdf ulaznog e-računa
    /// </summary>
    /// <param name="token">Token za otkazivanej</param>
    /// <returns>Sadržaj PDF dokumenta kao niz bajtova.</returns>
    public override async Task<byte[]> DohvatiPdfAsync(CancellationToken token = default) => await posrednik.UlazniPdfAsync(Id, token);

    /// <summary>
    /// Metoda koja odbija e-račun
    /// </summary>
    /// <param name="razlog">Razlog odbijanja računa</param>
    /// <param name="opis">Opis odbijanja računa</param>
    /// <param name="token">Token za otkazivanej</param>
    public async Task OdbijAsync(RazlogOdbijanja razlog, string opis, CancellationToken token = default) => await posrednik.OdbijRacunAsync(Id, razlog, opis, token);
}

/// <summary>
/// Predstavlja izlazni e-račun koji je poslаn posredniku.
/// </summary>
public class IzlazniERacun(Posrednik posrednik) : ERacun
{
    /// <summary>
    /// Trenutni status izlaznog računa.
    /// </summary>
    public IzlazniERacunStatus Status { get; set; }
    
    /// <summary>
    /// Identifikator UBL profila e-računa.
    /// Primjer: 'P1'.
    /// </summary>
    public string ProfileId { get; set; } = "";

    /// <summary>
    /// Metoda koja dohvati UBL ulaznog e-računa
    /// </summary>
    /// <param name="token">Token za otkazivanej</param>
    /// <returns>UBL XML sadržaj računa</returns>
    public override async Task<string> DohvatiUBLAsync(CancellationToken token = default) => await posrednik.IzlazniUBLAsync(Id, token);

    /// <summary>
    /// Metoda koja dohvati Pdf ulaznog e-računa
    /// </summary>
    /// <param name="token">Token za otkazivanej</param>
    /// <returns>Sadržaj PDF dokumenta kao niz bajtova.</returns>
    public override async Task<byte[]> DohvatiPdfAsync(CancellationToken token = default) => await posrednik.IzlazniPdfAsync(Id, token);

    /// <summary>
    /// Metoda koja evidentira uplatu na izlazni e-račun
    /// </summary>
    /// <param name="datum">Datum i vrijeme uplate.</param>
    /// <param name="amount">Iznos uplate.</param>
    /// <param name="nacinPlacanja">Način plaćanja.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    public async Task EvidentirajUplatuAsync(DateTime datum, double amount, ERacunNacinPlacanja nacinPlacanja, CancellationToken token = default) => 
        await posrednik.EvidentirajUplatuAsync(Id, datum, amount, nacinPlacanja, token);
}