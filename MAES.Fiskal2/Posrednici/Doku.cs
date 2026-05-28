using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MAES.Fiskal2.Posrednici;

/// <summary>
/// Implementacija posrednika za Doku. https://api.doku.hr/docs/
/// </summary>
public class Doku : Posrednik
{
    /// <summary>
    /// API ključ za autentifikaciju.
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// Konstruktor za inicijalizaciju Doku posrednika.
    /// </summary>
    public Doku()
    {
        UriProd = "https://api.doku.hr";
        UriDev = "https://api-test.doku.hr";
    }

    /// <summary>
    /// Evidentira UBL dokument u Doku sustavu. Doku očekuje Base64 enkodirani XML sadržaj UBL-a unutar JSON objekta s ključem "xml".
    /// </summary>
    /// <param name="ubl"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public override async Task EvidentirajUBLAsync(string ubl, CancellationToken token = default)
    {
        await sendRequest(HttpMethod.Post, "/documents/invoices/outgoing/upload?publicLink=false", new
        {
            xml = Convert.ToBase64String(Encoding.UTF8.GetBytes(ubl))
        }, token);
    }

    async Task<string> sendRequest(HttpMethod method, string url, object? body, CancellationToken token)
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri(Uri)
        };

        client.DefaultRequestHeaders.TryAddWithoutValidation("DOKU-API-KEY", ApiKey);

        using var request = new HttpRequestMessage(method, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        using var response = await client.SendAsync(request, token);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) throw new HttpRequestException(responseBody);

        return responseBody;
    }
}