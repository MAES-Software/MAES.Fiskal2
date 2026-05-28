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
        BaseAddressProd = "https://api.doku.hr";
        BaseAddressDev = "https://api-test.doku.hr";
        OnClientCreated += (s, e) =>
        {
            e.Client.DefaultRequestHeaders.TryAddWithoutValidation("DOKU-API-KEY", ApiKey);
        };
    }

    /// <summary>
    /// Evidentira UBL dokument u Doku sustavu. Doku očekuje Base64 enkodirani XML sadržaj UBL-a unutar JSON objekta s ključem "xml".
    /// </summary>
    /// <param name="ubl"></param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public override async Task EvidentirajUBLAsync(string ubl, CancellationToken token = default)
    {
        await SendRequest(HttpMethod.Post, "/documents/invoices/outgoing/upload", new
        {
            xml = Convert.ToBase64String(Encoding.UTF8.GetBytes(ubl))
        }, token);
    }

    /// <summary>
    /// Dohvaća listu izlaznih e-računa za zadano razdoblje.
    /// </summary>
    /// <param name="from">Datum početka razdoblja.</param>
    /// <param name="to">Datum kraja razdoblja.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns></returns>
    public override async Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to, CancellationToken token = default)
    {
        var res = await SendRequest(HttpMethod.Get, $"/documents/invoices/outgoing?IssueDateFrom={from:O}&IssueDateTo={to:O}", null, token);
        var doc = JsonDocument.Parse(res);
        
        var list = new List<IzlazniERacun>();
        foreach (var item in doc.RootElement.GetProperty("records").EnumerateArray())
        {
            list.Add(new IzlazniERacun
            {
                Id = item.GetProperty("id").GetInt64().ToString(),
                Broj = item.GetProperty("name").GetString()!,
                Datum = item.GetProperty("issueDate").GetDateTime(),
                PartnerNaziv = item.GetProperty("receiver").GetProperty("name").GetString()!,
                PartnerOIB = item.GetProperty("receiver").GetProperty("oib").GetString()!,
                Status = item.GetProperty("status").GetString() switch // <-- ovo treba popravit
                {
                    "DRAFT" => IzlazniERacunStatus.Nacrt,
                    "SENT" => IzlazniERacunStatus.Poslano,
                    "REJECTED" => IzlazniERacunStatus.Odbijeno,
                    _ => IzlazniERacunStatus.Dostavljeno
                }
            });
        }
        return list;
    }
}