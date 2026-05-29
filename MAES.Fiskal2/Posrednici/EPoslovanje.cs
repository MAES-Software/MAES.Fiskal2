using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MAES.Fiskal2.Posrednici;

/// <summary>
/// Implementacija posrednika za ePoslovanje, https://eposlovanje.hr/
/// </summary>
public class EPoslovanje : Posrednik
{
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
    /// API ključ za autentifikaciju.
    /// </summary>
    public string ApiKey { get; set; } = "";

    string apiKey = "";

    /// <summary>
    ///  Inicijalizira novog ePoslovanje posrednika s definiranim URI postavkama za produkcijsko i razvojno okruženje.
    /// </summary>
    public EPoslovanje()
    {
        BaseAddressProd = "https://eracun.eposlovanje.hr";
        BaseAddressDev = "https://test.eposlovanje.hr";
        OnClientCreated += async (s, e) =>
        {
            if(string.IsNullOrWhiteSpace(apiKey))
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(IsDev ? "https://test.eposlovanje.hr" : "https://eracun.eposlovanje.hr")
                };

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var request = new HttpRequestMessage(HttpMethod.Post, "/api/v2/account/apikey")
                {
                    Content = new StringContent(JsonSerializer.Serialize(new
                    {
                        username = Username,
                        password = Password,
                        vatId = OIB,
                        softwareId = "MAES.Fiskal2"
                    }), Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException(json);

                using var doc = JsonDocument.Parse(json);
                apiKey = doc.RootElement.GetProperty("apiKey").GetString()!;
            }
            e.Client.DefaultRequestHeaders.Add("ApiKeyAuth", apiKey);
        };
    }

    async Task changeStatusAsync(string id, int status, string? note = null, double? partialPaymentAmount = null, CancellationToken token = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["status"] = status,
            ["changedOn"] = DateTimeOffset.Now.ToString("O")
        };

        if (!string.IsNullOrWhiteSpace(note)) body["note"] = note;
        if (partialPaymentAmount.HasValue) body["partialPaymentAmount"] = partialPaymentAmount.Value;

        await SendRequest(HttpMethod.Post, $"/api/v2/document/changestatus/{id}", body, token);
    }

    /// <summary>
    /// Dohvaća XML/UBL sadržaj ulaznog računa.
    /// </summary>
    public override async Task<string> UlazniUBLAsync(string id, CancellationToken token = default) => 
        JsonDocument.Parse(await SendRequest(HttpMethod.Get, $"/api/v2/document/get/{id}", null, token)).RootElement.GetProperty("document").GetString()!;

    /// <summary>
    /// Dohvaća PDF sadržaj ulaznog računa.
    /// </summary>
    public override async Task<byte[]> UlazniPdfAsync(string id, CancellationToken token = default) =>
        Convert.FromBase64String(JsonDocument.Parse(await SendRequest(HttpMethod.Get, $"/api/v2/document/visualization/{id}", null, token)).RootElement.GetProperty("pdf").GetString()!);

    /// <summary>
    /// Dohvaća popis ulaznih e-računa.
    /// </summary>
    public override async Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to, CancellationToken token = default)
    {
        var content = await SendRequest(HttpMethod.Get, $"/api/v2/document/incoming?insertedFrom={from:O}&insertedTo={to:O}&limit=1000&offset=0", null, token);
        var doc = JsonDocument.Parse(content);

        var list = new List<UlazniERacun>();

        foreach (var item in doc.RootElement.EnumerateArray())
        {
            list.Add(new UlazniERacun
            {
                Id = item.GetProperty("id").GetInt64().ToString(),
                Datum = item.GetProperty("issuedOn").GetDateTime(),
                Broj = item.GetProperty("documentId").GetString()!,
                Status = UlazniERacunStatus.Zaprimljeno, // TODO: ovo treba popravit
                Partner = item.GetProperty("customerPartyName").GetString()!,
                PartnerOIB = item.GetProperty("customerPartyVATId").GetString()!
            });
        }

        return list;
    }

    /// <summary>
    /// Dohvaća XML/UBL sadržaj izlaznog računa.
    /// </summary>
    public override async Task<string> IzlazniUBLAsync(string id, CancellationToken token = default)
        => await UlazniUBLAsync(id, token);

    /// <summary>
    /// Dohvaća PDF sadržaj izlaznog računa.
    /// </summary>
    public override async Task<byte[]> IzlazniPdfAsync(string id, CancellationToken token = default)
        => await UlazniPdfAsync(id, token);

    /// <summary>
    /// Dohvaća popis izlaznih e-računa.
    /// </summary>
    public override async Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(
    DateTime from,
    DateTime to,
    CancellationToken token = default)
    {
        var content = await SendRequest(HttpMethod.Get, $"/api/v2/document/outgoing?insertedFrom={from:O}&insertedTo={to:O}&limit=1000&offset=0", null, token);
        var doc = JsonDocument.Parse(content);

        var list = new List<IzlazniERacun>();

        foreach (var item in doc.RootElement.EnumerateArray())
        {
            list.Add(new IzlazniERacun
            {
                Id = item.GetProperty("id").GetInt64().ToString(),
                Broj = item.GetProperty("documentId").GetString()!,
                Datum = item.GetProperty("issuedOn").GetDateTime(),
                PartnerNaziv = item.GetProperty("customerPartyName").GetString()!,
                PartnerOIB = item.GetProperty("customerPartyVATId").GetString()!,
                Status = IzlazniERacunStatus.Poslano // TODO: ovo treba popravit
            });
        }

        return list;
    }

    /// <summary>
    /// Evidentira UBL dokument u ePoslovanje sustav.
    /// </summary>
    public override async Task EvidentirajUBLAsync(string ubl, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v2/document/send")
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                document = ubl,
                softwareId = "MAES.Blagajna"
            }), Encoding.UTF8, "application/json")
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        await checkApiKeyAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

        await SendRequest2(request, token);
        // await SendRequest(HttpMethod.Post, "/api/v2/document/send", new
        // {
        //     document = ubl,
        //     softwareId = "MAES.Blagajna"
        // }, token);
    }

    /// <summary>
    /// Evidentira uplatu za račun.
    /// </summary>
    public override Task EvidentirajUplatuAsync(string id, DateTime date, double amount, NacinPlacanja paymentMethod, CancellationToken token = default)
    {
        var status = amount > 0 ? 8 : 7; // 8: partialno, 7: potpuno
        return changeStatusAsync(id, status, partialPaymentAmount: status == 8 ? amount : null, token: token);
    }

    /// <summary>
    /// Odbija račun.
    /// </summary>
    public override Task OdbijRacunAsync(string id, RazlogOdbijanja razlog, string opis, CancellationToken token = default) =>
        changeStatusAsync(id, status: 6, $"{razlog}: {opis}", token: token);

    async Task checkApiKeyAsync()
    {
        if(string.IsNullOrWhiteSpace(apiKey))
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(BaseAddress)
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v2/account/apikey")
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    username = Username,
                    password = Password,
                    vatId = OIB,
                    softwareId = "MAES.Fiskal2"
                }), Encoding.UTF8, "application/json")
            };
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(json);
            using var doc = JsonDocument.Parse(json);
            apiKey = doc.RootElement.GetProperty("apiKey").GetString()!;
        }
    }
}