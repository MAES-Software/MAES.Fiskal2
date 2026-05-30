using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace MAES.Fiskal2.Posrednici;

/// <summary>
/// Implementacija informacijskog posrednika FINA za razmjenu e-računa. Ovo je ili najgori servis ikada ili sam ja retardiran, ali FINA ne dozvoljava dohvat liste računa, PDF-a ili UBL-a. Jedino što se može je poslati račun
/// </summary>
public class Fina : Posrednik
{

    /// <summary>
    /// OIB poslovnog subjekta.
    /// </summary>
    public string OIB { get; set; } = "";

    /// <summary>
    /// X.509 certifikat koji se koristi za autentikaciju prema FINA e-Račun sustavu.
    /// </summary>
    public X509Certificate2? Certificate { get; set; }

    /// <summary>
    /// Inicijalizira novog FINA posrednika s definiranim URI postavkama za produkcijsko i razvojno okruženje.
    /// </summary>
    public Fina() : base("https://eracun.fina.hr/eracun-b2b/services/eRacunB2BPortType", "https://eracun-test.fina.hr/eracun-b2b/services/eRacunB2BPortType") { }

    /// <summary>
    /// Evidentira i šalje UBL/XML dokument prema FINA e-Račun sustavu.
    /// </summary>
    /// <param name="ubl">UBL/XML sadržaj dokumenta.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Asinkrona operacija slanja dokumenta.</returns>
    public override async Task EvidentirajUBLAsync(string ubl, CancellationToken token = default)
    {
        var xml = XDocument.Parse(ubl);
        var supplierInvoiceId = GetInvoiceId(xml);
        var buyerId = GetBuyerId(xml);

        var msg = new SendB2BOutgoingInvoiceMsg
        {
            HeaderSupplier = new HeaderSupplierType
            {
                MessageID = Guid.NewGuid().ToString("N"),
                SupplierID = OIB,
                ERPID = "MAES.Fiskal2",
                MessageType = "1"
            },

            Data = new SendB2BOutgoingInvoiceMsgData
            {
                B2BOutgoingInvoiceEnvelope = new ()
                {
                    XMLStandard = SendB2BOutgoingInvoiceMsgDataB2BOutgoingInvoiceEnvelopeXMLStandard.UBL,
                    SpecificationIdentifier = SendB2BOutgoingInvoiceMsgDataB2BOutgoingInvoiceEnvelopeSpecificationIdentifier.urnceneuen169312017complianturnmfingovhrcius202510conformanturnmfingovhrext202510,
                    SupplierInvoiceID = supplierInvoiceId,
                    BuyerID = buyerId,
                    AdditionalBuyerID = null,
                    Item = Encoding.UTF8.GetBytes(ubl),
                    ItemElementName = ItemChoiceType.InvoiceEnvelope
                }
            }
        };

        var baseAddress = IsDev ? "https://eracun-test.fina.hr/eracun-b2b/services/eRacunB2BPortType" : "https://eracun.fina.hr/eracun-b2b/services/eRacunB2BPortType";
        using var client = new eRacunB2BPortTypeClient(eRacunB2BPortTypeClient.EndpointConfiguration.eRacunB2BPortType, baseAddress);

        var res = await client.sendB2BOutgoingInvoiceAsync(msg);

        if (res.SendB2BOutgoingInvoiceAckMsg.MessageAck.AckStatus != AckStatusType.ACCEPTED)
            throw new Exception(res.SendB2BOutgoingInvoiceAckMsg.MessageAck.AckStatusText);
    }



    /// <summary>
    /// Evidentira uplatu za dokument unutar FINA sustava.
    /// </summary>
    /// <param name="id">ID dokumenta u sustavu posrednika.</param>
    /// <param name="date">Datum i vrijeme evidentiranja uplate.</param>
    /// <param name="amount">Iznos uplate.</param>
    /// <param name="paymentMethod">Način plaćanja.</param>
    /// <param name="token">Cancellation token.</param>
    public override async Task EvidentirajUplatuAsync(string id, DateTime date, double amount, NacinPlacanja paymentMethod, CancellationToken token = default) =>
        throw new NotSupportedException("Evidentiranje uplata nije podržano u FINA posredniku");

    /// <summary>
    /// Dohvaća popis izlaznih e-računa za zadani period.
    /// </summary>
    /// <param name="from">Početni datum pretrage.</param>
    /// <param name="to">Završni datum pretrage.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Popis izlaznih e-računa.</returns>
    public override Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to, CancellationToken token = default) =>
        throw new NotSupportedException("Lista izlaznih računa nije dostupna u FINA posredniku");

    /// <summary>
    /// Dohvaća PDF vizualizaciju izlaznog dokumenta.
    /// </summary>
    /// <param name="id">ID dokumenta.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>PDF dokument kao byte array.</returns>
    public override Task<byte[]> IzlazniPdfAsync(string id, CancellationToken token = default) =>
        throw new NotSupportedException("PDF izlaznih računa nije dostupan u FINA posredniku");

    /// <summary>
    /// Dohvaća UBL/XML sadržaj izlaznog dokumenta.
    /// </summary>
    /// <param name="id">ID dokumenta.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>UBL/XML sadržaj dokumenta.</returns>
    public override Task<string> IzlazniUBLAsync(string id, CancellationToken token = default) =>
        throw new NotSupportedException("UBL izlaznih računa nije dostupan u FINA posredniku");

    /// <summary>
    /// Odbija dokument uz zadani razlog i opis.
    /// </summary>
    /// <param name="id">ID dokumenta.</param>
    /// <param name="razlog">Razlog odbijanja.</param>
    /// <param name="opis">Dodatni opis odbijanja.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Asinkrona operacija odbijanja dokumenta.</returns>
    public override Task OdbijRacunAsync(string id, RazlogOdbijanja razlog, string opis, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Dohvaća popis ulaznih e-računa za zadani period.
    /// </summary>
    /// <param name="from">Početni datum pretrage.</param>
    /// <param name="to">Završni datum pretrage.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Popis ulaznih e-računa.</returns>
    public override Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to, CancellationToken token = default) =>
        throw new NotSupportedException("Lista ulaznih računa nije dostupna u FINA posredniku");

    /// <summary>
    /// Dohvaća PDF vizualizaciju ulaznog dokumenta.
    /// </summary>
    /// <param name="id">ID dokumenta.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>PDF dokument kao byte array.</returns>
    public override Task<byte[]> UlazniPdfAsync(string id, CancellationToken token = default) =>
        throw new NotSupportedException("PDF ulaznih računa nije dostupan u FINA posredniku");

    /// <summary>
    /// Dohvaća UBL/XML sadržaj ulaznog dokumenta.
    /// </summary>
    /// <param name="id">ID dokumenta.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>UBL/XML sadržaj dokumenta.</returns>
    public override Task<string> UlazniUBLAsync(string id, CancellationToken token = default) =>
        throw new NotSupportedException("UBL ulaznih računa nije dostupan u FINA posredniku");

    static string GetInvoiceId(XDocument xml)
    {
        XNamespace cbc =
            "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

        return xml.Root?
                   .Element(cbc + "ID")?
                   .Value
               ?? throw new InvalidOperationException("UBL nema Invoice ID.");
    }

    static string GetBuyerId(XDocument xml)
    {
        XNamespace cac =
            "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        XNamespace cbc =
            "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

        return xml.Root?
                   .Element(cac + "AccountingCustomerParty")?
                   .Descendants(cbc + "CompanyID")
                   .FirstOrDefault()?
                   .Value
               ?? throw new InvalidOperationException("UBL nema BuyerID.");
    }
}