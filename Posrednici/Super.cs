using System.Net.Http.Headers;
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

    static readonly JsonSerializerOptions jsonOpts = new(JsonSerializerDefaults.Web);
    static SuperTokenResponse? token;

    async Task<SuperTokenResponse> getTokenAsync(HttpClient client)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "Token");

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"]   = Username,
            ["password"]   = Password
        });

        var response = await client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"SUPER token error: {json}");

        return JsonSerializer.Deserialize<SuperTokenResponse>(json, jsonOpts)!;
    }

    public async Task<IEnumerable<IzlazniERacun>> IzlazniAsync(DateTime from, DateTime to)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(IsDev ? URI_DEV : URI);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var token = await getTokenAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/SendingInvoice/GetSendingInvoiceList");

        // Headers
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);

        // Body
        var body = new Dictionary<string, string>
        {
            ["MessageId"] = Guid.NewGuid().ToString(),
            ["CompanyGuid"] = BusinessGuid,
            ["DateFrom"] = from.ToString("yyyy-MM-dd"),
            ["DateTo"] = to.ToString("yyyy-MM-dd")
        };

        request.Content = new FormUrlEncodedContent(body);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var response = await client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);

        var result = JsonSerializer.Deserialize<SendingInvoiceListResponse>(json, jsonOpts)!;

        if (!string.IsNullOrEmpty(result.ErrorMessage))
            throw new Exception(result.ErrorMessage);

        return [.. result.Invoices.Select(x => new IzlazniERacun
        {
            Broj = x.Number,
            Datum = x.IssueDate,
            Status = IzlazniERacunStatus.Poslano, // <-- ovo treba popravit
            PartnerNaziv = x.Supplier,
            PartnerAdresa = $"{x.SupplierAddress}, {x.SupplierZip} {x.SupplierCity}",
            PartnerOIB = x.SupplierOib,
            Guid = x.Guid
        })];
    }
    public async Task<byte[]> DohvatiPdfAsync(string id, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(IsDev ? URI_DEV : URI );
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        token = await getTokenAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/Invoice/GetInvoiceDetailVisualization");

        // Headers
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);

        var body = new Dictionary<string, string>
        {
            ["MessageId"] = Guid.NewGuid().ToString(),
            ["Guid"] = id.ToString()
        };

        request.Content = new FormUrlEncodedContent(body);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var response = await client.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"SUPER GetInvoiceDetailVisualization error: {json}");

        var result = JsonSerializer.Deserialize<InvoiceDetailVisualizationResponse>(json, jsonOpts)!;
        return Convert.FromBase64String(result.InvoiceDetailVisualization);
    }

    public async Task<IEnumerable<UlazniERacun>> UlazniAsync(DateTime from, DateTime to)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(IsDev ? URI_DEV : URI );
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        token = await getTokenAsync(client);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/Invoice/GetInvoiceList");

        // Headers
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["MessageId"]   = Guid.NewGuid().ToString(),
            ["CompanyGuid"] = BusinessGuid,
            ["DateFrom"] = from.ToString("yyyy-MM-dd"),
            ["DateTo"] = to.ToString("yyyy-MM-dd")
        });

        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var response = await client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) throw new HttpRequestException(json);

        var result = JsonSerializer.Deserialize<SuperInvoiceListResponse>(json, jsonOpts)!;

        return result.Invoices.Select(x => new UlazniERacun()
        {
            Guid = x.Guid,
            Broj = x.Number,
            Datum = x.IssueDate,
            Partner = x.Supplier,
            PartnerOIB = x.SupplierOib,
            PartnerAdresa = $"{x.SupplierAddress}, {x.SupplierCity} {x.SupplierZip}"
        });
    }
    
    public async Task<Guid> EvidentirajUBLAsync(string ubl)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(IsDev ? URI_DEV : URI );
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        string base64Ubl = Convert.ToBase64String(Encoding.UTF8.GetBytes(ubl));

        token = await getTokenAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/SendingInvoice/SendSendingInvoiceUbl");

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["MessageId"]   = Guid.NewGuid().ToString(),
            ["CompanyGuid"] = BusinessGuid,
            ["Base64EncodedUbl"] = base64Ubl,
            ["UblDocumentType"] = "1", // 1 = račun, 2 = odobrenje
        });

        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var response = await client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"SUPER SendSendingInvoiceUbl error: {json}");

        var result = JsonSerializer.Deserialize<SuperSendInvoiceResponse>(json, jsonOpts)!;

        if (!string.IsNullOrWhiteSpace(result.ErrorMessage)) throw new Exception(result.ErrorMessage);

        return result.Guid;
    }
}

sealed class SuperTokenResponse
{
    public string access_token { get; set; } = "";
    public string token_type { get; set; } = "";
    public int expires_in { get; set; }
    public string userName { get; set; } = "";
    public string issued { get; set; } = "";
    public string expires { get; set; } = "";
}

sealed class SuperInvoiceListResponse
{
    public string MessageId { get; set; } = "";
    public string? ErrorMessage { get; set; }
    public List<SuperInvoiceDto> Invoices { get; set; } = [];
}

sealed class SuperInvoiceDto
{
    public int UniqueId { get; set; }
    public string Number { get; set; } = "";
    public Guid Guid { get; set; }
    public DateTime IssueDate { get; set; }
    public string IssueTime { get; set; } = "";
    public string Supplier { get; set; } = "";
    public string SupplierOib { get; set; } = "";
    public string SupplierAddress { get; set; } = "";
    public string SupplierCity { get; set; } = "";
    public string SupplierZip { get; set; } = "";
    public int InvoiceStatus { get; set; }
}

sealed class SuperSendInvoiceResponse
{
    public Guid MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid Guid { get; set; }
}

sealed class InvoiceDetailVisualizationResponse
{
    public string MessageId { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public string InvoiceDetailVisualization { get; set; } = "";
}

sealed class SendingInvoiceListResponse
{
    public string MessageId { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public string UniqueId { get; set; } = "";
    public Guid Guid { get; set; }
    public DateTime IssueDate { get; set; }
    public string IssueTime { get; set; } = "";
    public string Supplier { get; set; } = "";
    public string SupplierOib { get; set; } = "";
    public List<SendingInvoiceItem> Invoices { get; set; } = [];
}

sealed class SendingInvoiceItem
{
    public string UniqueId { get; set; } = "";
    public string Number { get; set; } = "";
    public Guid Guid { get; set; }
    public DateTime IssueDate { get; set; }
    public string IssueTime { get; set; } = "";
    public string Supplier { get; set; } = "";
    public string SupplierOib { get; set; } = "";
    public string SupplierAddress { get; set; } = "";
    public string SupplierCity { get; set; } = "";
    public string SupplierZip { get; set; } = "";
    public string SendingInvoiceStatus { get; set; } = "";
}