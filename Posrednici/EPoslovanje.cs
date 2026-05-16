using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MAES.Fiskal2.Posrednici;

public class EPoslovanje : IPosrednik
{
    const string URI = "https://eracun.eposlovanje.hr";
    const string URI_DEV = "https://test.eposlovanje.hr";

    public bool IsDev { get; set; }
    public string OIB { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    public async Task<IEnumerable<IzlazniERacun>> IzlazniAsync(DateTime from, DateTime to) => throw new NotImplementedException();
    public async Task<IEnumerable<UlazniERacun>> UlazniAsync(DateTime from, DateTime to) => throw new NotImplementedException();
    public async Task<Guid> EvidentirajUBLAsync(string ubl)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(IsDev ? URI_DEV : URI );
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

        return Guid.NewGuid();
    }
    public async Task<byte[]> DohvatiPdfAsync(string id, CancellationToken token) => throw new NotImplementedException();

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

    public Task<string> UlazniUBLAsync(string id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> UlazniPdfAsync(string id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<string> IzlazniUBLAsync(string id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> IzlazniPdfAsync(string id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> EvidentirajUBLAsync(string ubl, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}