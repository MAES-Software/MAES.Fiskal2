using MAES.Fiskal2.Posrednici;

namespace MAES.Fiskal2.Tests;

public class SuperTests
{
    readonly List<Posrednik> posrednici =
    [
        new Super
        {
            BusinessGuid = Environment.GetEnvironmentVariable("SUPER_BUSINESS_GUID") ?? throw new InvalidOperationException("SUPER_BUSINESS_GUID environment variable is not set."),
            Username = Environment.GetEnvironmentVariable("SUPER_USERNAME") ?? throw new InvalidOperationException("SUPER_USERNAME environment variable is not set."),
            Password = Environment.GetEnvironmentVariable("SUPER_PASSWORD") ?? throw new InvalidOperationException("SUPER_PASSWORD environment variable is not set."),
            IsDev = true
        },
        new EPoslovanje
        {
            OIB = Environment.GetEnvironmentVariable("EPOSLOVANJE_OIB") ?? throw new InvalidOperationException("EPOSLOVANJE_OIB environment variable is not set."),
            Username = Environment.GetEnvironmentVariable("EPOSLOVANJE_USERNAME") ?? throw new InvalidOperationException("EPOSLOVANJE_USERNAME environment variable is not set."),
            Password = Environment.GetEnvironmentVariable("EPOSLOVANJE_PASSWORD") ?? throw new InvalidOperationException("EPOSLOVANJE_PASSWORD environment variable is not set."),
            IsDev = true
        },
        new Fina
        {
            OIB = Environment.GetEnvironmentVariable("FINA_OIB") ?? throw new InvalidOperationException("FINA_OIB environment variable is not set."),
            //Certificate = LoadCertificateFromStore(Environment.GetEnvironmentVariable("FINA_CERT_THUMBPRINT") ?? throw new InvalidOperationException("FINA_CERT_THUMBPRINT environment variable is not set.")),
            IsDev = true
        }
    ];

    [Fact]
    public async Task SendInvoiceUBL()
    {
        var ubl = File.ReadAllText("ubl.xml");

        foreach (var posrednik in posrednici)
        {
            await posrednik.EvidentirajUBLAsync(ubl);
        }
    }

    [Fact]
    public async Task NoFinaTests()
    {
        foreach (var posrednik in posrednici.Where(p => p is not Fina))
        {
            var izlazni = await posrednik.IzlazniListAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
            Assert.NotNull(izlazni);

            var first = izlazni.FirstOrDefault();
            if(first != null)
            {
                var pdf = await posrednik.IzlazniPdfAsync(first.Id);
                Assert.NotNull(pdf);

                var ubl = await posrednik.IzlazniUBLAsync(first.Id);
                Assert.NotNull(ubl);

                await posrednik.EvidentirajUplatuAsync(first.Id, DateTime.UtcNow, 100, NacinPlacanja.TransakcijskiRaCun);
            }

            var ulazni = await posrednik.UlazniListAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
            Assert.NotNull(ulazni);

            var firstUlazni = ulazni.FirstOrDefault();
            if(firstUlazni != null)
            {
                var pdf = await posrednik.UlazniPdfAsync(firstUlazni.Id);
                Assert.NotNull(pdf);

                var ubl = await posrednik.UlazniUBLAsync(firstUlazni.Id);
                Assert.NotNull(ubl);

                await posrednik.OdbijRacunAsync(firstUlazni.Id, RazlogOdbijanja.NeusklađenostKojaNeUtjeceNaObracunPoreza, "Nedostaje OIB");
            }
        }
    }
}