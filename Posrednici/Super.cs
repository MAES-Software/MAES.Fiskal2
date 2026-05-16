using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace MAES.Fiskal2.Posrednici;

public class Super : IPosrednik
{
    const string URI = "https://api.super.hr/";
    const string URI_DEV = "https://apitest.super.hr/";

    public bool IsDev { get; set; }
    public string BusinessGuid { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    static KeyValuePair<string, DateTime>? token = null;

    async Task<JsonDocument> postRequest(string uri, Dictionary<string, string> body, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(IsDev ? URI_DEV : URI);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (token == null || DateTime.UtcNow >= token.Value.Value)
        {
            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "Token");
            tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["grant_type"] = "password", ["username"] = Username, ["password"] = Password });

            var tokenResponse = await client.SendAsync(tokenRequest, cancellationToken);

            if (!tokenResponse.IsSuccessStatusCode) throw new HttpRequestException($"Greška prilikom dohvaćanja tokena");

            using var doc = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync(cancellationToken));

            token = new KeyValuePair<string, DateTime>(
                doc.RootElement.GetProperty("access_token").GetString()!,
                DateTime.Now.AddSeconds(doc.RootElement.GetProperty("expires_in").GetInt32() - 60)
            );
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token!.Value.Key);
        request.Content = new FormUrlEncodedContent(body);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Greška prilikom slanja zahtjeva: {uri}");

        var jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if(jsonDocument.RootElement.GetProperty("ErrorMessage").GetString() is string error && !string.IsNullOrWhiteSpace(error)) throw new Exception(error);
        return jsonDocument;
    }

    public Task<string> UlazniUBLAsync(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<byte[]> UlazniPdfAsync(string id, CancellationToken cancellationToken = default) => Convert.FromBase64String((await postRequest("api/Invoice/GetInvoiceDetailVisualization", new Dictionary<string, string>
    {
        ["MessageId"] = Guid.NewGuid().ToString(),
        ["Guid"] = id.ToString()
    }, cancellationToken)).RootElement.GetProperty("InvoiceDetailVisualization").GetString()!);

    public async Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var jsonDocument = await postRequest("api/Invoice/GetInvoiceList", new Dictionary<string, string>
        {
            ["MessageId"] = Guid.NewGuid().ToString(),
            ["CompanyGuid"] = BusinessGuid,
            ["DateFrom"] = from.ToString("yyyy-MM-dd"),
            ["DateTo"] = to.ToString("yyyy-MM-dd")
        }, cancellationToken);

        return jsonDocument.RootElement.GetProperty("Invoices").EnumerateArray().Select(x => new UlazniERacun
        {
            Broj = x.GetProperty("UniqueId").GetString() ?? "",
            Datum = x.GetProperty("IssueDate").GetDateTime(),
            Partner = x.GetProperty("Supplier").GetString() ?? "",
            PartnerOIB = x.GetProperty("SupplierOib").GetString() ?? "",
            PartnerAdresa =
                $"{x.GetProperty("SupplierAddress").GetString()}, " +
                $"{x.GetProperty("SupplierZip").GetString()} " +
                $"{x.GetProperty("SupplierCity").GetString()}",
            Guid = x.GetProperty("Guid").GetGuid(),
            Status = UlazniERacunStatus.Zaprimljeno // treba popravit
        });
    }

    public Task<string> IzlazniUBLAsync(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> IzlazniPdfAsync(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var jsonDocument = await postRequest("api/SendingInvoice/GetSendingInvoiceList", new Dictionary<string, string>
        {
            ["MessageId"] = Guid.NewGuid().ToString(),
            ["CompanyGuid"] = BusinessGuid,
            ["DateFrom"] = from.ToString("yyyy-MM-dd"),
            ["DateTo"] = to.ToString("yyyy-MM-dd")
        }, cancellationToken);

        return jsonDocument.RootElement.GetProperty("Invoices").EnumerateArray().Select(x => new IzlazniERacun
        {
            Broj = x.GetProperty("UniqueId").GetString() ?? "",
            Datum = x.GetProperty("IssueDate").GetDateTime(),
            PartnerNaziv = x.GetProperty("Supplier").GetString() ?? "",
            PartnerOIB = x.GetProperty("SupplierOib").GetString() ?? "",
            PartnerAdresa =
                $"{x.GetProperty("SupplierAddress").GetString()}, " +
                $"{x.GetProperty("SupplierZip").GetString()} " +
                $"{x.GetProperty("SupplierCity").GetString()}",
            Guid = x.GetProperty("Guid").GetGuid(),
            Status = IzlazniERacunStatus.Poslano // treba popravit
        });
    }

    public async Task EvidentirajUBLAsync(string ubl, CancellationToken cancellationToken = default) => await postRequest("api/SendingInvoice/GetSendingInvoiceList", new Dictionary<string, string>
    {
        ["MessageId"]   = Guid.NewGuid().ToString(),
        ["CompanyGuid"] = BusinessGuid,
        ["Base64EncodedUbl"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(ubl)),
        ["UblDocumentType"] = "1", // 1 = račun, 2 = odobrenje
    }, cancellationToken);

    public async Task EvidentirajUplatuAsync(string id, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task OdbijRacunAsync(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}