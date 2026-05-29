using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MAES.Fiskal2.Posrednici;

namespace MAES.Fiskal2;

/// <summary>
/// Bazna implementacija posrednika za komunikaciju sa servisom fiskalizacije / eRačuna.
/// Sadrži zajedničke URI postavke i definira osnovne operacije za rad s ulaznim i izlaznim eRačunima.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Super), "Super")]
[JsonDerivedType(typeof(EPoslovanje), "EPoslovanje")]
[JsonDerivedType(typeof(Fina), "Fina")]
[JsonDerivedType(typeof(Doku), "Doku")]
[JsonDerivedType(typeof(MER), "MER")]
public abstract class Posrednik
{
    /// <summary>
    /// Označava koristi li se razvojno okruženje.
    /// Ako je <c>true</c>, koristi se <see cref="BaseAddressDev"/>; inače <see cref="BaseAddressProd"/>.
    /// </summary>
    public bool IsDev { get; set; }

    /// <summary>
    /// Produkcijski URI servisa.
    /// </summary>
    protected string BaseAddressProd { private get; set; } = "";

    /// <summary>
    /// URI razvojnog (testnog) okruženja servisa.
    /// </summary>
    protected string BaseAddressDev { private get; set; } = "";

    /// <summary>
    /// Događaj koji se pokreće nakon kreiranja HTTP klijenta unutar metode <see cref="SendRequest"/>. Omogućuje dodatnu konfiguraciju klijenta prije slanja zahtjeva.
    /// </summary>
    protected event EventHandler<ClientCreatedEventArgs>? OnClientCreated;

    /// <summary>
    /// Aktivni URI servisa ovisno o odabranom okruženju.
    /// </summary>
    public string BaseAddress => IsDev ? BaseAddressDev : BaseAddressProd;

    /// <summary>
    /// Evidentira izlazni eRačun na temelju UBL sadržaja.
    /// </summary>
    /// <param name="ubl">UBL XML sadržaj računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    public virtual Task EvidentirajUBLAsync(string ubl, CancellationToken token = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Evidentira uplatu za postojeći račun.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="date">Datum i vrijeme uplate.</param>
    /// <param name="amount">Iznos uplate.</param>
    /// <param name="paymentMethod">Način plaćanja.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    public virtual Task EvidentirajUplatuAsync(
        string id,
        DateTime date,
        double amount,
        NacinPlacanja paymentMethod,
        CancellationToken token = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Dohvaća popis izlaznih eRačuna unutar zadanog razdoblja.
    /// </summary>
    /// <param name="from">Početni datum razdoblja.</param>
    /// <param name="to">Završni datum razdoblja.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Kolekcija izlaznih eRačuna.</returns>
    public virtual Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Dohvaća PDF prikaz izlaznog računa.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Sadržaj PDF dokumenta kao niz bajtova.</returns>
    public virtual Task<byte[]> IzlazniPdfAsync(string id, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Dohvaća UBL XML izlaznog računa.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>UBL XML sadržaj računa.</returns>
    public virtual Task<string> IzlazniUBLAsync(string id, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Odbija ulazni eRačun uz navedeni razlog i opis.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="razlog">Razlog odbijanja.</param>
    /// <param name="opis">Dodatni opis odbijanja.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    public virtual Task OdbijRacunAsync(string id, RazlogOdbijanja razlog, string opis, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Dohvaća popis ulaznih eRačuna unutar zadanog razdoblja.
    /// </summary>
    /// <param name="from">Početni datum razdoblja.</param>
    /// <param name="to">Završni datum razdoblja.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Kolekcija ulaznih eRačuna.</returns>
    public virtual Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Dohvaća PDF prikaz ulaznog računa.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Sadržaj PDF dokumenta kao niz bajtova.</returns>
    public virtual Task<byte[]> UlazniPdfAsync(string id, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Dohvaća UBL XML ulaznog računa.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>UBL XML sadržaj računa.</returns>
    public virtual Task<string> UlazniUBLAsync(string id, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Prihvaća ulazni eRačun, označavajući ga kao zaprimljen ili odobren (ovisno o implementaciji posrednika).
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual Task PrihvatiRacunAsync(string id, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Generička metoda za slanje HTTP zahtjeva prema posredniku. Koristi se unutar specifičnih implementacija posrednika za komunikaciju s njihovim API-jem.
    /// </summary>
    /// <param name="method">Metoda HTTP zahtjeva.</param>
    /// <param name="url">URL zahtjeva.</param>
    /// <param name="body">Tijelo zahtjeva. (ostavi null ako je prazno)</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Content string</returns>
    /// <exception cref="HttpRequestException"></exception>
    protected async Task<string> SendRequest(HttpMethod method, string url, object? body = null, CancellationToken token = default)
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri(BaseAddress)
        };

        OnClientCreated?.Invoke(this, new (client));

        using var request = new HttpRequestMessage(method, url);

        if(body != null) request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request, token);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) throw new HttpRequestException(content);

        return content;
    }

    /// <summary>
    /// Alternativna metoda za slanje HTTP zahtjeva koja prima već pripremljeni <see cref="HttpRequestMessage"/>. Omogućuje veću fleksibilnost u konfiguraciji zahtjeva, poput postavljanja prilagođenih zaglavlja ili tijela koje nije JSON.
    /// </summary>
    /// <param name="request">Pripremljeni HTTP zahtjev.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Sadržaj odgovora kao niz znakova.</returns>
    /// <exception cref="HttpRequestException"></exception>
    protected async Task<string> SendRequest2(HttpRequestMessage request, CancellationToken token = default)
    {
        using var client = new HttpClient { BaseAddress = new Uri(BaseAddress) };

        using var response = await client.SendAsync(request, token);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) throw new HttpRequestException(content);

        return content;
    }
}