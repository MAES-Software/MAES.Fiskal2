namespace MAES.Fiskal2.Posrednici;

/// <summary>
/// Implementacija informacijskog posrednika FINA za razmjenu e-računa.
/// </summary>
public class Fina : IPosrednik
{
    /// <summary>
    /// Evidentira i šalje UBL/XML dokument prema FINA e-Račun sustavu.
    /// </summary>
    /// <param name="ubl">UBL/XML sadržaj dokumenta.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Asinkrona operacija slanja dokumenta.</returns>
    public Task EvidentirajUBLAsync(string ubl, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Evidentira uplatu za dokument unutar FINA sustava.
    /// </summary>
    /// <param name="id">ID dokumenta u sustavu posrednika.</param>
    /// <param name="date">Datum i vrijeme evidentiranja uplate.</param>
    /// <param name="amount">Iznos uplate.</param>
    /// <param name="paymentMethod">Način plaćanja.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Asinkrona operacija evidentiranja uplate.</returns>
    public Task EvidentirajUplatuAsync(string id, DateTime date, double amount, NacinPlacanja paymentMethod, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Dohvaća popis izlaznih e-računa za zadani period.
    /// </summary>
    /// <param name="from">Početni datum pretrage.</param>
    /// <param name="to">Završni datum pretrage.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Popis izlaznih e-računa.</returns>
    public Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Dohvaća PDF vizualizaciju izlaznog dokumenta.
    /// </summary>
    /// <param name="id">ID dokumenta.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>PDF dokument kao byte array.</returns>
    public Task<byte[]> IzlazniPdfAsync(string id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Dohvaća UBL/XML sadržaj izlaznog dokumenta.
    /// </summary>
    /// <param name="id">ID dokumenta.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>UBL/XML sadržaj dokumenta.</returns>
    public Task<string> IzlazniUBLAsync(string id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Odbija dokument uz zadani razlog i opis.
    /// </summary>
    /// <param name="id">ID dokumenta.</param>
    /// <param name="razlog">Razlog odbijanja.</param>
    /// <param name="opis">Dodatni opis odbijanja.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Asinkrona operacija odbijanja dokumenta.</returns>
    public Task OdbijRacunAsync(string id, RazlogOdbijanja razlog, string opis, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Dohvaća popis ulaznih e-računa za zadani period.
    /// </summary>
    /// <param name="from">Početni datum pretrage.</param>
    /// <param name="to">Završni datum pretrage.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Popis ulaznih e-računa.</returns>
    public Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Dohvaća PDF vizualizaciju ulaznog dokumenta.
    /// </summary>
    /// <param name="id">ID dokumenta.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>PDF dokument kao byte array.</returns>
    public Task<byte[]> UlazniPdfAsync(string id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Dohvaća UBL/XML sadržaj ulaznog dokumenta.
    /// </summary>
    /// <param name="id">ID dokumenta.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>UBL/XML sadržaj dokumenta.</returns>
    public Task<string> UlazniUBLAsync(string id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}