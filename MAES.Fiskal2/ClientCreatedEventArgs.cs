namespace MAES.Fiskal2;

/// <summary>
/// Podaci događaja koji se prosljeđuju prilikom pokretanja događaja <see cref="Posrednik.OnClientCreated"/>. Sadrži referencu na kreirani <see cref="HttpClient"/> koji se koristi za slanje zahtjeva prema posredniku.
/// </summary>
/// <param name="client">Kreirani HTTP klijent.</param>
public class ClientCreatedEventArgs(HttpClient client) : EventArgs
{
    /// <summary>
    /// Kreirani HTTP klijent koji se koristi za komunikaciju s posrednikom. Dopušta dodatnu konfiguraciju (npr. dodavanje zaglavlja) prije slanja zahtjeva.
    /// </summary>
    public HttpClient Client { get; } = client;
}