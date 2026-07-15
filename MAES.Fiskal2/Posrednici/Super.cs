using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MAES.Fiskal2.Posrednici;

/// <summary>
/// Implementacija posrednika za Super, https://www.super.hr/
/// </summary>
public class Super : Posrednik
{
    /// <summary>
    /// Jedinstveni identifikator poslovnog subjekta u Super sustavu.
    /// </summary>
    public string BusinessGuid { get; set; } = "";

    /// <summary>
    /// Korisničko ime za autentifikaciju na Super API-u.
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// Lozinka za autentifikaciju na Super API-u.
    /// </summary>
    public string Password { get; set; } = "";

    static KeyValuePair<string, DateTime>? token = null;

    /// <summary>
    /// Inicijalizira novog Super posrednika s definiranim URI postavkama za produkcijsko i razvojno okruženje.
    /// </summary>
    public Super() : base("https://api.super.hr/", "https://apitest.super.hr/") { }

    /// <summary>
    /// Dohvaća XML/UBL sadržaj ulaznog računa po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator ulaznog računa.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    /// <returns>XML/UBL sadržaj računa kao tekst.</returns>
    public override async Task<string> UlazniUBLAsync(string id, CancellationToken cancellationToken = default)
    {
        var doc = await postRequestAsync("api/Invoice/GetInvoice", new Dictionary<string, string>
        {
            ["Guid"] = id.ToString()
        }, cancellationToken);

        if (!doc.RootElement.TryGetProperty("invoiceUbl", out var el)) throw new Exception("UBL not found in response");
        return Encoding.UTF8.GetString(Convert.FromBase64String(el.GetString()));
    }

    /// <summary>
    /// Dohvaća PDF sadržaj ulaznog računa po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator ulaznog računa.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    /// <returns>PDF sadržaj računa kao bajtni niz.</returns>
    public override async Task<byte[]> UlazniPdfAsync(string id, CancellationToken cancellationToken = default) => Convert.FromBase64String((await postRequestAsync("api/Invoice/GetInvoiceDetailVisualization", new Dictionary<string, string>
    {
        ["Guid"] = id.ToString()
    }, cancellationToken)).RootElement.GetProperty("invoiceDetailVisualization").GetString()!);

    /// <summary>
    /// Dohvaća popis ulaznih e-računa u zadanom vremenskom rasponu.
    /// </summary>
    /// <param name="from">Početni datum vremenskog raspona.</param>
    /// <param name="to">Krajnji datum vremenskog raspona.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    /// <returns>Popis ulaznih e-računa.</returns>
    public override async Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var doc = await postRequestAsync("api/Invoice/GetInvoiceList", new Dictionary<string, string>
        {
            ["DateFrom"] = from.ToString("yyyy-MM-dd"),
            ["DateTo"] = to.ToString("yyyy-MM-dd")
        }, cancellationToken);

        return doc.RootElement.GetProperty("invoices").EnumerateArray().Select(x => new UlazniERacun
        {
            Broj = x.GetProperty("UniqueId").GetString() ?? "",
            Datum = x.GetProperty("IssueDate").GetDateTime(),
            Partner = x.GetProperty("Supplier").GetString() ?? "",
            PartnerOIB = x.GetProperty("SupplierOib").GetString() ?? "",
            PartnerAdresa =
                $"{x.GetProperty("SupplierAddress").GetString()}, " +
                $"{x.GetProperty("SupplierZip").GetString()} " +
                $"{x.GetProperty("SupplierCity").GetString()}",
            Id = x.GetProperty("Guid").GetGuid().ToString(),
            Status = UlazniERacunStatus.Zaprimljeno // treba popravit
        });
    }

    /// <summary>
    /// Dohvaća XML/UBL sadržaj izlaznog računa po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator izlaznog računa.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    /// <returns>XML/UBL sadržaj računa kao tekst.</returns>
    public override async Task<string> IzlazniUBLAsync(string id, CancellationToken cancellationToken = default)
    {
        var doc = await postRequestAsync("api/SendingInvoice/GetSendingInvoice", new Dictionary<string, string>
        {
            ["Guid"] = id.ToString()
        }, cancellationToken);

        if (!doc.RootElement.TryGetProperty("sendingInvoiceUbl", out var el)) throw new Exception("UBL not found in response");
        return Encoding.UTF8.GetString(Convert.FromBase64String(el.GetString()));
        
    }

    /// <summary>
    /// Dohvaća PDF sadržaj izlaznog računa po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator izlaznog računa.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    /// <returns>PDF sadržaj računa kao bajtni niz.</returns>
    public override async Task<byte[]> IzlazniPdfAsync(string id, CancellationToken cancellationToken = default)
    {
        var doc = await postRequestAsync("api/SendingInvoice/GetSendingInvoiceDetailVisualization", new Dictionary<string, string>
        {
            ["Guid"] = id.ToString()
        }, cancellationToken);

        var root = doc.RootElement;

        if (!root.TryGetProperty("sendingInvoiceDetailVisualization", out var el)) throw new Exception("PDF not found in response");
        return Convert.FromBase64String(el.GetString());
        
    }

    /// <summary>
    /// Dohvaća popis izlaznih e-računa u zadanom vremenskom rasponu.
    /// </summary>
    /// <param name="from">Početni datum vremenskog raspona.</param>
    /// <param name="to">Krajnji datum vremenskog raspona.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    /// <returns>Popis izlaznih e-računa.</returns>
    public override async Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var doc = await postRequestAsync("api/SendingInvoice/GetSendingInvoiceList", new Dictionary<string, string>
        {
            ["DateFrom"] = from.ToString("yyyy-MM-dd"),
            ["DateTo"] = to.ToString("yyyy-MM-dd")
        }, cancellationToken);

        return doc.RootElement.GetProperty("sendingInvoices").EnumerateArray().Select(x => new IzlazniERacun
        {
            Broj = x.GetProperty("UniqueId").GetString() ?? "",
            Datum = x.GetProperty("IssueDate").GetDateTime(),
            PartnerNaziv = x.GetProperty("Supplier").GetString() ?? "",
            PartnerOIB = x.GetProperty("SupplierOib").GetString() ?? "",
            PartnerAdresa =
                $"{x.GetProperty("SupplierAddress").GetString()}, " +
                $"{x.GetProperty("SupplierZip").GetString()} " +
                $"{x.GetProperty("SupplierCity").GetString()}",
            Id = x.GetProperty("Guid").GetGuid().ToString(),
            Status = IzlazniERacunStatus.Poslano // treba popravit
        });
    }

    /// <summary>
    /// Evidentira ulazni UBL dokument u fiskalizacijski sustav.
    /// </summary>
    /// <param name="ubl">UBL XML dokument ulaznog računa.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    public override async Task EvidentirajUBLAsync(string ubl, CancellationToken cancellationToken = default)
    {
        var json = await postRequestAsync("api/SendingInvoice/GetSendingInvoiceList", new Dictionary<string, string>
        {
            ["Base64EncodedUbl"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(ubl)),
            ["UblDocumentType"] = "1", // 1 = račun, 2 = odobrenje
        }, cancellationToken);

        if(json.RootElement.TryGetProperty("Guid", out var el) && el.GetString() is string guid)
        {
            Console.WriteLine($"Super.hr račun prijavljen {guid}");
        }
        else if(json.RootElement.TryGetProperty("guid", out var el2) && el2.GetString() is string guid2)
        {
            Console.WriteLine($"Super.hr račun prijavljen {guid2}");
        }
        else throw new Exception(json.ToString());
    }

    /// <summary>
    /// Evidentira uplatu za račun po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator računa čiju uplatu treba evidentirati.</param>
    /// <param name="date">Datum uplate.</param>
    /// <param name="amount">Iznos uplate.</param>
    /// <param name="paymentMethod">Način plaćanja.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    public override async Task EvidentirajUplatuAsync(string id, DateTime date, double amount, NacinPlacanja paymentMethod, CancellationToken cancellationToken = default)
    {
        await postRequestAsync("api/Invoice/SetInvoicePayment", new Dictionary<string, string>
        {
            ["Guid"] = id.ToString()
        }, cancellationToken);
    }

    /// <summary>
    /// Odbija račun u fiskalizacijskom procesu prema zadanom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator računa koji se odbija.</param>
    /// <param name="razlog">Razlog odbijanja računa.</param>
    /// <param name="opis">Opis razloga odbijanja.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    public override async Task OdbijRacunAsync(string id, RazlogOdbijanja razlog, string opis, CancellationToken cancellationToken = default)
    {
        await postRequestAsync("api/SendingInvoice/RejectSendingInvoice", new Dictionary<string, string>
        {
            ["Guid"] = id.ToString(),
            ["RejectReasonType"] = razlog.ToString(),
            ["RejectionReasonDescription"] = opis
         }, cancellationToken);
    }

    async Task<JsonDocument> postRequestAsync(string uri, Dictionary<string, string> body, CancellationToken cancellationToken = default)
    {
        // dohvati token ako ga nema ili je istekao
        if (token == null || DateTime.UtcNow >= token.Value.Value)
        {
            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "Token");
            tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["grant_type"] = "password", ["username"] = Username, ["password"] = Password });

            var tokenContent = await SendRequest(tokenRequest, cancellationToken);

            using var doc = JsonDocument.Parse(tokenContent);

            token = new KeyValuePair<string, DateTime>(
                doc.RootElement.GetProperty("access_token").GetString()!,
                DateTime.Now.AddSeconds(doc.RootElement.GetProperty("expires_in").GetInt32() - 60)
            );
        }

        body.Add("MessageId", Guid.NewGuid().ToString());
        body.Add("CompanyGuid", BusinessGuid);

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.Key);
        request.Content = new FormUrlEncodedContent(body);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var content = await SendRequest(request, cancellationToken);

        var jsonDocument = JsonDocument.Parse(content);
        if(jsonDocument.RootElement.TryGetProperty("errorMessage", out var errorMessage) && errorMessage.GetString() is string error && !string.IsNullOrWhiteSpace(error)) throw new Exception(error);
        if(jsonDocument.RootElement.TryGetProperty("ErrorMessage", out var errorMessage2) && errorMessage2.GetString() is string error2 && !string.IsNullOrWhiteSpace(error2)) throw new Exception(error2);
        if(jsonDocument.RootElement.TryGetProperty("error_message", out var errorMessage3) && errorMessage3.GetString() is string error3 && !string.IsNullOrWhiteSpace(error3)) throw new Exception(error3);
        return jsonDocument;
    }
}