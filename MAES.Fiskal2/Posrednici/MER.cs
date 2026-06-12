using System.Text;
using System.Text.Json;

namespace MAES.Fiskal2.Posrednici;

/// <summary>
/// Implementacija posrednika za Moj-eRačun, servis za slanje i obradu elektroničkih računa.
/// Omogućuje evidentiranje UBL/XML dokumenata putem demo ili produkcijskog API-ja.
/// </summary>
public class MER : Posrednik
{
    /// <summary>
    /// Korisničko ime za autentifikaciju na Moj-eRačun API-u.
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// Lozinka za autentifikaciju na Moj-eRačun API-u.
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    /// OIB poslovnog subjekta pošiljatelja.
    /// </summary>
    public string OIB { get; set; } = "";

    /// <summary>
    /// ID softvera za autentifikaciju na Moj-eRačun API-u.
    /// </summary>
    public string SoftwareId { get; set; } = "";

    /// <summary>
    /// ID poslovnog subjekta koji se koristi u nekim Moj-eRačun API pozivima, obično jednak OIB-u.
    /// </summary>
    public string CompanyId { get; set; } = "";

    /// <summary>
    /// Inicijalizira novog Moj-eRačun posrednika s definiranim URI adresama
    /// za produkcijsko i razvojno okruženje.
    /// </summary>
    public MER() : base("https://www.moj-eracun.hr", "https://demo.moj-eracun.hr") { }

    /// <summary>
    /// Šalje UBL/XML dokument izlaznog e-računa na Moj-eRačun servis.
    /// Dokument se validira i obrađuje, a u slučaju uspješnog slanja
    /// servis generira jedinstveni identifikator dokumenta.
    /// </summary>
    /// <param name="ubl">UBL XML sadržaj e-računa.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    public override async Task EvidentirajUBLAsync(string ubl, CancellationToken cancellationToken = default) => await sendRequest(HttpMethod.Post, "/apis/v2/send", new
    {
        Username,
        Password,
        CompanyId,
        SoftwareId,
        File = Convert.ToBase64String(Encoding.UTF8.GetBytes(ubl))
    }, cancellationToken);

    /// <summary>
    /// Dohvat ulaznog UBL dokumenta.
    /// </summary>
    /// <param name="id">Identifikator dokumenta.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    public override async Task<string> UlazniUBLAsync(string id, CancellationToken cancellationToken = default) => await sendRequest(HttpMethod.Post, $"/apis/v2/receive", new
    {
        Username,
        Password,
        CompanyId = OIB,
        SoftwareId = "MAES.Fiskal2",
        ElectronicId = id
    }, cancellationToken);

    /// <summary>
    /// Dohvat PDF prikaza ulaznog računa trenutno nije podržan putem Moj-eRačun integracije.
    /// </summary>
    /// <param name="id">Identifikator dokumenta.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    public override Task<byte[]> UlazniPdfAsync(
        string id,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    /// <summary>
    /// Dohvat popisa ulaznih e-računa trenutno nije podržan putem Moj-eRačun integracije.
    /// </summary>
    /// <param name="from">Početni datum raspona.</param>
    /// <param name="to">Završni datum raspona.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    public override async Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var response = await sendRequest(HttpMethod.Post, $"/apis/v2/queryInbox", new
        {
            Username,
            Password,
            CompanyId = OIB,
            SoftwareId = "MAES.Fiskal2",
            From = from.ToString("yyyy-MM-dd"),
            To = to.ToString("yyyy-MM-dd")
        }, cancellationToken);

        List<UlazniERacun> racuni = [];
        foreach (var item in JsonSerializer.Deserialize<List<object>>(response) ?? [])
        {
            racuni.Add(new UlazniERacun
            {
                Id = item.GetType().GetProperty("ElectronicId")?.GetValue(item)?.ToString() ?? "",
                Datum = DateTime.Parse(item.GetType().GetProperty("Sent")?.GetValue(item)?.ToString() ?? ""),
                Partner = item.GetType().GetProperty("SenderBusinessName")?.GetValue(item)?.ToString() ?? "",
                PartnerOIB = item.GetType().GetProperty("SenderBusinessNumber")?.GetValue(item)?.ToString() ?? "",
                Broj = item.GetType().GetProperty("DocumentNr")?.GetValue(item)?.ToString() ?? "",
                Status = (int.TryParse(item.GetType().GetProperty("StatusId")?.GetValue(item)?.ToString() ?? "0", out var status) ? status : 0) switch
                {
                    20 => UlazniERacunStatus.Zaprimljeno,
                    30 => UlazniERacunStatus.Odobreno,
                    40 => UlazniERacunStatus.Odbijeno,
                    _ => UlazniERacunStatus.Likvidirano
                }
            });
        }
        return racuni;
    }

    /// <summary>
    /// Dohvat izlaznog UBL dokumenta.
    /// </summary>
    /// <param name="id">Identifikator dokumenta.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    public override async Task<string> IzlazniUBLAsync(string id, CancellationToken cancellationToken = default) => await UlazniUBLAsync(id, cancellationToken);

    /// <summary>
    /// Dohvat PDF prikaza izlaznog računa trenutno nije podržan putem Moj-eRačun integracije.
    /// </summary>
    /// <param name="id">Identifikator dokumenta.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    public override Task<byte[]> IzlazniPdfAsync(
        string id,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    /// <summary>
    /// Dohvat popisa izlaznih e-računa trenutno nije podržan putem Moj-eRačun integracije.
    /// </summary>
    /// <param name="from">Početni datum raspona.</param>
    /// <param name="to">Završni datum raspona.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    public override async Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
         var response = await sendRequest(HttpMethod.Post, $"/apis/v2/queryOutbox", new
        {
            Username,
            Password,
            CompanyId = OIB,
            SoftwareId = "MAES.Fiskal2",
            From = from.ToString("yyyy-MM-dd"),
            To = to.ToString("yyyy-MM-dd")
        }, cancellationToken);

        List<IzlazniERacun> racuni = [];
        foreach (var item in JsonSerializer.Deserialize<List<object>>(response) ?? [])
        {
            racuni.Add(new IzlazniERacun
            {
                Id = item.GetType().GetProperty("ElectronicId")?.GetValue(item)?.ToString() ?? "",
                Datum = DateTime.Parse(item.GetType().GetProperty("Sent")?.GetValue(item)?.ToString() ?? ""),
                PartnerNaziv = item.GetType().GetProperty("RecipientBusinessName")?.GetValue(item)?.ToString() ?? "",
                PartnerOIB = item.GetType().GetProperty("RecipientBusinessNumber")?.GetValue(item)?.ToString() ?? "",
                Broj = item.GetType().GetProperty("DocumentNr")?.GetValue(item)?.ToString() ?? "",
                Status = (int.TryParse(item.GetType().GetProperty("StatusId")?.GetValue(item)?.ToString() ?? "0", out var status) ? status : 0) switch
                {
                    20 => IzlazniERacunStatus.Dostavljeno,
                    30 => IzlazniERacunStatus.Poslano,
                    40 => IzlazniERacunStatus.Odbijeno,
                    _ => IzlazniERacunStatus.Greška
                }
            });
        }
        return racuni;
    }

    /// <summary>
    /// Evidentira uplatu za račun po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="date">Datum uplate.</param>
    /// <param name="amount">Iznos uplate.</param>
    /// <param name="paymentMethod">Način plaćanja.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    public override async Task EvidentirajUplatuAsync(string id, DateTime date, double amount, NacinPlacanja paymentMethod, CancellationToken cancellationToken = default)
    {
        await sendRequest(HttpMethod.Post, $"/api/fiscalization/markPaid", new
        {
            Username,
            Password,
            CompanyId = OIB,
            SoftwareId = "MAES.Fiskal2",
            ElectronicId = id,
            PaymentDate = date.ToString("yyyy-MM-dd"),
            PaymentAmount = amount,
            PaymentMethod = "T" // TODO: ovo treba mapirati iz NacinPlacanja enum-a
        }, cancellationToken);
    }

    /// <summary>
    /// Odbijanje računa trenutno nije podržano putem Moj-eRačun integracije.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="razlog">Razlog odbijanja.</param>
    /// <param name="opis">Opis razloga odbijanja.</param>
    /// <param name="cancellationToken">Token za otkazivanje operacije.</param>
    public override async Task OdbijRacunAsync(string id, RazlogOdbijanja razlog, string opis, CancellationToken cancellationToken = default)
    {
        await sendRequest(HttpMethod.Post, $"/api/fiscalization/rejectWithoutElectronicID", new
        {
            Username,
            Password,
            CompanyId = OIB,
            SoftwareId = "MAES.Fiskal2",
            ElectronicId = id,
            RejectionDate = DateTime.Now,
            RejectionReasonDescription = opis,
            RejectionReasonType = razlog switch
            {
                RazlogOdbijanja.NeusklađenostKojaNeUtjeceNaObracunPoreza => "N",
                RazlogOdbijanja.NeusklađenostKojaUtjeceNaObracunPoreza => "U",
                _ => "O"
            }
        }, cancellationToken);
    }

    async Task<string> sendRequest(HttpMethod method, string uri, object? body = null, CancellationToken token = default)
    {
        using var request = new HttpRequestMessage(method, uri);

        if(body != null)
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        return await SendRequest(request, token);
    }
}