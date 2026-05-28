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
    /// <param name="ubl">UBL dokument.</param>
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
    /// <returns>Lista izlaznih e-računa.</returns>
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

    /// <summary>
    /// Dohvaća UBL/XML sadržaj izlaznog računa. Doku vraća Base64 enkodirani XML sadržaj unutar JSON objekta s ključem "xml".
    /// </summary>
    /// <param name="id">ID izlaznog računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Base64 enkodirani XML sadržaj izlaznog računa.</returns>
    public override async Task<string> IzlazniUBLAsync(string id, CancellationToken token = default)
    {
        var content = await SendRequest(HttpMethod.Get, $"/documents/invoices/outgoing/{id}/download?format=ubl", null, token);
        var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("data").GetProperty("xml").GetString()!;
    }

    /// <summary>
    /// Evidentira uplatu za izlazni račun.
    /// </summary>
    /// <param name="id">ID izlaznog računa.</param>
    /// <param name="date">Datum uplate.</param>
    /// <param name="amount">Iznos uplate.</param>
    /// <param name="paymentMethod">Način plaćanja.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns></returns>
    public override async Task EvidentirajUplatuAsync(string id, DateTime date, double amount, NacinPlacanja paymentMethod, CancellationToken token = default)
    {
        await SendRequest(HttpMethod.Post, $"/documents/invoices/outgoing/{id}/payments", new
        {
            datumNaplate = date,
            naplaceniIznos = amount,
            nacinPlacanja = paymentMethod switch
            {
                NacinPlacanja.ObračunskoPlaćanje => "Z",
                NacinPlacanja.TransakcijskiRaCun => "T",
                _ => "O"
            }
        }, token);
    }

    /// <summary>
    /// Dohvaća ulazne e-račune za zadano razdoblje.
    /// </summary>
    /// <param name="from">Početni datum.</param>
    /// <param name="to">Završni datum.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Lista ulaznih e-računa.</returns>
    public override async Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to, CancellationToken token = default)
    {
        var content = await SendRequest(HttpMethod.Get, $"/documents/invoices/incoming?IssueDateFrom={from:O}&IssueDateTo={to:O}", null, token);
        var doc = JsonDocument.Parse(content);
        var list = new List<UlazniERacun>();
        foreach (var item in doc.RootElement.GetProperty("records").EnumerateArray())
        {
            list.Add(new UlazniERacun
            {
                Id = item.GetProperty("id").GetInt64().ToString(),
                Broj = item.GetProperty("name").GetString()!,
                Datum = item.GetProperty("issueDate").GetDateTime(),
                Partner = item.GetProperty("sender").GetProperty("name").GetString()!,
                PartnerOIB = item.GetProperty("sender").GetProperty("oib").GetString()!,
                Status = item.GetProperty("status").GetString() switch
                {
                    "RECIVED" => UlazniERacunStatus.Zaprimljeno, // <-- ovo treba popravit mozda treba pogledat dokumentaciju
                    "APPROVED" => UlazniERacunStatus.Odobreno,
                    "REJECTED" => UlazniERacunStatus.Odbijeno,
                    _ => UlazniERacunStatus.Likvidirano
                }
            });
        }
        return list;
    }

    /// <summary>
    /// Dohvaća UBL/XML sadržaj ulaznog računa. Doku vraća Base64 enkodirani XML sadržaj.
    /// </summary>
    /// <param name="id">ID ulaznog računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Base64 enkodirani XML sadržaj ulaznog računa.</returns>
    public override async Task<string> UlazniUBLAsync(string id, CancellationToken token = default)
    {
        var content = await SendRequest(HttpMethod.Get, $"/documents/invoices/incoming/{id}/export", null, token);
        var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("data").GetProperty("xml").GetString()!;
    }

    /// <summary>
    /// Odbija ulazni račun uz navedeni razlog i opis.
    /// </summary>
    /// <param name="id">ID ulaznog računa.</param>
    /// <param name="razlog">Razlog odbijanja.</param>
    /// <param name="opis">Opis odbijanja.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns></returns> 
    public override async Task OdbijRacunAsync(string id, RazlogOdbijanja razlog, string opis, CancellationToken token = default)
    {
        await SendRequest(HttpMethod.Post, $"/documents/invoices/incoming/{id}/reject", new
        {
            datumOdbijanja = DateTime.Now.ToString("yyyy-MM-dd"),
            vrstaRazlogaOdbijanja = razlog switch
            {
                RazlogOdbijanja.NeusklađenostKojaNeUtjeceNaObracunPoreza => "N",
                RazlogOdbijanja.NeusklađenostKojaUtjeceNaObracunPoreza => "U",
                _ => "O"
            },
            razlogOdbijanja = opis
        }, token);
    }
}