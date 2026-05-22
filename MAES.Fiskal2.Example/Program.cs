using MAES.Fiskal2.Posrednici;

// initialize Super provider with your credentials
Super super = new ()
{
    BusinessGuid = "YOUR_BUSINESS_GUID",
    Username = "YOUR_EMAIL",
    Password = "YOUR_PASSWORD"
};

await sendUBLInvoice(super);
await getInvoices(super);

async Task sendUBLInvoice(Super super)
{
    // create UBL invoice as string (you can generate it using any library or create it manually)
    // ubl must be in Croatian language and follow the UBL 2.1 standard with specific extensions for Croatian fiscalization
    string ubl = File.ReadAllText("ubl.xml");

    // send UBL invoice to Super provider
    await super.EvidentirajUBLAsync(ubl);
}

async Task getInvoices(Super super)
{
    // retrieve invoices from Super provider
    var invoices = await super.UlazniListAsync(DateTime.Now.AddDays(-30), DateTime.Now);

    // print retrieved invoices
    foreach (var invoice in invoices)
        Console.WriteLine($"Invoice: {invoice.Broj}, Date: {invoice.Datum}, Total: {invoice.Status}");
}