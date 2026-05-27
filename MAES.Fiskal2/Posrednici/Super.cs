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
    public Super()
    {
        UriProd = "https://api.super.hr/";
        UriDev = "https://apitest.super.hr/";
    }

    async Task<JsonDocument> postRequest(string uri, Dictionary<string, string> body, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(Uri);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (token == null || DateTime.UtcNow >= token.Value.Value)
        {
            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "Token");
            tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["grant_type"] = "password", ["username"] = Username, ["password"] = Password });

            var tokenResponse = await client.SendAsync(tokenRequest, cancellationToken);

            if (!tokenResponse.IsSuccessStatusCode) throw new HttpRequestException($"Greška prilikom dohvaćanja tokena");

            using var doc = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());

            token = new KeyValuePair<string, DateTime>(
                doc.RootElement.GetProperty("access_token").GetString()!,
                DateTime.Now.AddSeconds(doc.RootElement.GetProperty("expires_in").GetInt32() - 60)
            );
        }

        body.Add("MessageId", Guid.NewGuid().ToString());
        body.Add("CompanyGuid", BusinessGuid);

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token!.Value.Key);
        request.Content = new FormUrlEncodedContent(body);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Greška prilikom slanja zahtjeva: {uri}");

        var jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if(jsonDocument.RootElement.GetProperty("errorMessage").GetString() is string error && !string.IsNullOrWhiteSpace(error)) throw new Exception(error);
        return jsonDocument;
    }

    /// <summary>
    /// Dohvaća XML/UBL sadržaj ulaznog računa po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator ulaznog računa.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    /// <returns>XML/UBL sadržaj računa kao tekst.</returns>
    public override async Task<string> UlazniUBLAsync(string id, CancellationToken cancellationToken = default)
    {
        var jDoc = await postRequest("api/Invoice/GetInvoice", new Dictionary<string, string>
        {
            ["Guid"] = id.ToString()
        }, cancellationToken);

        if (!jDoc.RootElement.TryGetProperty("invoiceUbl", out var el)) throw new Exception("UBL not found in response");
        return Encoding.UTF8.GetString(Convert.FromBase64String(el.GetString()));
    }

    /// <summary>
    /// Dohvaća PDF sadržaj ulaznog računa po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator ulaznog računa.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    /// <returns>PDF sadržaj računa kao bajtni niz.</returns>
    public override async Task<byte[]> UlazniPdfAsync(string id, CancellationToken cancellationToken = default) => Convert.FromBase64String((await postRequest("api/Invoice/GetInvoiceDetailVisualization", new Dictionary<string, string>
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
        var jsonDocument = await postRequest("api/Invoice/GetInvoiceList", new Dictionary<string, string>
        {
            ["DateFrom"] = from.ToString("yyyy-MM-dd"),
            ["DateTo"] = to.ToString("yyyy-MM-dd")
        }, cancellationToken);

        return jsonDocument.RootElement.GetProperty("invoices").EnumerateArray().Select(x => new UlazniERacun
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
        var jDoc = await postRequest("api/SendingInvoice/GetSendingInvoice", new Dictionary<string, string>
        {
            ["Guid"] = id.ToString()
        }, cancellationToken);

        if (!jDoc.RootElement.TryGetProperty("sendingInvoiceUbl", out var el)) throw new Exception("UBL not found in response");
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
        var jDoc = await postRequest("api/SendingInvoice/GetSendingInvoiceDetailVisualization", new Dictionary<string, string>
        {
            ["Guid"] = id.ToString()
        }, cancellationToken);

        var root = jDoc.RootElement;

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
        var jsonDocument = await postRequest("api/SendingInvoice/GetSendingInvoiceList", new Dictionary<string, string>
        {
            ["DateFrom"] = from.ToString("yyyy-MM-dd"),
            ["DateTo"] = to.ToString("yyyy-MM-dd")
        }, cancellationToken);

        return jsonDocument.RootElement.GetProperty("invoices").EnumerateArray().Select(x => new IzlazniERacun
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
    public override async Task EvidentirajUBLAsync(string ubl, CancellationToken cancellationToken = default) => await postRequest("api/SendingInvoice/GetSendingInvoiceList", new Dictionary<string, string>
    {
        ["Base64EncodedUbl"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(ubl)),
        ["UblDocumentType"] = "1", // 1 = račun, 2 = odobrenje
    }, cancellationToken);

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
        await postRequest("api/Invoice/SetInvoicePayment", new Dictionary<string, string>
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
        await postRequest("api/SendingInvoice/RejectSendingInvoice", new Dictionary<string, string>
        {
            ["Guid"] = id.ToString(),
            ["RejectReasonType"] = razlog.ToString(),
            ["RejectionReasonDescription"] = opis
         }, cancellationToken);
    }
}