using MAES.Fiskal2.Posrednici;

namespace MAES.Fiskal2.Tests;

public class SuperTests
{
    Super superProvider = new ()
    {
        BusinessGuid = Environment.GetEnvironmentVariable("SUPER_BUSINESS_GUID") ?? throw new InvalidOperationException("SUPER_BUSINESS_GUID environment variable is not set."),
        Username = Environment.GetEnvironmentVariable("SUPER_USERNAME") ?? throw new InvalidOperationException("SUPER_USERNAME environment variable is not set."),
        Password = Environment.GetEnvironmentVariable("SUPER_PASSWORD") ?? throw new InvalidOperationException("SUPER_PASSWORD environment variable is not set."),
        IsDev = true
    };

    [Fact]
    public async Task SendInvoiceUBL()
    {
        // dohvati ubl iz datoteke
        await superProvider.EvidentirajUBLAsync("");
    }
}