using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MAES.Fiskal2.Posrednici;

/// <summary>
/// Implementacija posrednika za ePoslovanje, https://eposlovanje.hr/
/// </summary>
public class EPoslovanje : IPosrednik
{
    const string URI = "https://eracun.eposlovanje.hr";
    const string URI_DEV = "https://test.eposlovanje.hr";

    /// <summary>
    /// Označava je li povezivanje na razvojni (test) API endpoint.
    /// </summary>
    public bool IsDev { get; set; }

    /// <summary>
    /// OIB poslovnog subjekta.
    /// </summary>
    public string OIB { get; set; } = "";

    /// <summary>
    /// Korisničko ime za autentifikaciju.
    /// </summary>
    public string Username { get; set; } = "";
    
    /// <summary>
    /// Lozinka za autentifikaciju.
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    /// Dohvaća XML/UBL sadržaj ulaznog računa. Nije implementirano.
    /// </summary>
    public Task<string> UlazniUBLAsync(string id, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Dohvaća PDF sadržaj ulaznog računa. Nije implementirano.
    /// </summary>
    public Task<byte[]> UlazniPdfAsync(string id, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Dohvaća popis ulaznih e-računa. Nije implementirano.
    /// </summary>
    public Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Dohvaća XML/UBL sadržaj izlaznog računa. Nije implementirano.
    /// </summary>
    public Task<string> IzlazniUBLAsync(string id, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Dohvaća PDF sadržaj izlaznog računa. Nije implementirano.
    /// </summary>
    public Task<byte[]> IzlazniPdfAsync(string id, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Dohvaća popis izlaznih e-računa. Nije implementirano.
    /// </summary>
    public Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Evidentira UBL dokument u ePoslovanje sustav.
    /// </summary>
    public async Task EvidentirajUBLAsync(string ubl, CancellationToken token = default)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(IsDev ? URI_DEV : URI);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var key = await apiKey(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(key);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v2/document/send");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["document"]   = ubl,
            ["softwareId"] = "MAES.Blagajna"
        }), Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"ePoslovanje send document error: {json}");

        // TODO: Ovo treba popravit, dohvatit id od eposlovanja
    }

    /// <summary>
    /// Evidentira uplatu za račun. Nije implementirano.
    /// </summary>
    public Task EvidentirajUplatuAsync(string id, CancellationToken token = default) => throw new NotImplementedException();

    /// <summary>
    /// Odbija račun. Nije implementirano.
    /// </summary>
    public Task OdbijRacunAsync(string id, CancellationToken token = default) => throw new NotImplementedException();

    async Task<string> apiKey(HttpClient client)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v2/account/apikey");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["username"]   = Username,
            ["password"]   = Password,
            ["vatId"]      = OIB,
            ["softwareId"] = "MAES.Blagajna"
        }), Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"{json}");

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("apiKey").GetString()!;
    }
}