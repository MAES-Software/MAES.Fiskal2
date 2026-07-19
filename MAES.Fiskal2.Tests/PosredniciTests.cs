
using MAES.Fiskal2.Posrednici;

namespace MAES.Fiskal2.Tests;

public class PosredniciTests
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
            ApiKey = Environment.GetEnvironmentVariable("EPOSLOVANJE_API_KEY") ?? throw new InvalidOperationException("EPOSLOVANJE_API_KEY environment variable is not set."),
            IsDev = true
        },
        new MER
        {
            Username = Environment.GetEnvironmentVariable("MER_USERNAME") ?? throw new InvalidOperationException("MER_USERNAME environment variable is not set."),
            Password = Environment.GetEnvironmentVariable("MER_PASSWORD") ?? throw new InvalidOperationException("MER_PASSWORD environment variable is not set."),
            CompanyId = Environment.GetEnvironmentVariable("MER_COMPANY_ID") ?? throw new InvalidOperationException("MER_COMPANY_ID environment variable is not set."),
            SoftwareId = Environment.GetEnvironmentVariable("MER_SOFTWARE_ID") ?? throw new InvalidOperationException("MER_SOFTWARE_ID environment variable is not set."),
            IsDev = true
        }
    ];

    [Fact]
    public async Task EvidentirajUBL()
    {
        foreach (var posrednik in posrednici)
        {
            try
            {
                await posrednik.EvidentirajUBLAsync(File.ReadAllText("ubl.xml"));
                Console.WriteLine($"{posrednik.GetType().Name}: EvidentirajUBL OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: EvidentirajUBL FAIL {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task IzlazniRacuni()
    {
        foreach (var posrednik in posrednici)
        {
            try
            {
                await posrednik.IzlazniListAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
                Console.WriteLine($"{posrednik.GetType().Name}: IzlazniList OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: IzlazniList FAIL {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task DohvatiIzlazniPdfUBL()
    {
        foreach (var posrednik in posrednici)
        {
            try
            {
                if ((await posrednik.IzlazniListAsync(DateTime.MinValue, DateTime.MaxValue)).FirstOrDefault() is IzlazniERacun racun)
                {
                    await racun.DohvatiPdfAsync();
                    await racun.DohvatiUBLAsync();
                    Console.WriteLine($"{posrednik.GetType().Name}: Izlazni PDF + UBL OK");
                }
                else Console.WriteLine($"{posrednik.GetType().Name}: nema izlaznih računa");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: Izlazni PDF + UBL FAIL {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task EvidentirajUplatu()
    {
        foreach (var posrednik in posrednici)
        {
            try
            {
                if ((await posrednik.IzlazniListAsync(DateTime.MinValue, DateTime.MaxValue)).FirstOrDefault() is IzlazniERacun racun)
                {
                    await racun.EvidentirajUplatuAsync(DateTime.Now, 100, ERacunNacinPlacanja.TransakcijskiRacun);
                    Console.WriteLine($"{posrednik.GetType().Name}: EvidentirajUplatu OK");
                }
                else Console.WriteLine($"{posrednik.GetType().Name}: nema izlaznih računa");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: EvidentirajUplatu FAIL {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task UlazniRacuni()
    {
        foreach (var posrednik in posrednici)
        {
            try
            {
                await posrednik.UlazniListAsync(DateTime.MinValue, DateTime.MaxValue);
                Console.WriteLine($"{posrednik.GetType().Name}: UlazniList OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: UlazniList FAIL {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task DohvatiUlazniPdfUBL()
    {
        foreach (var posrednik in posrednici)
        {
            try
            {
                if ((await posrednik.UlazniListAsync(DateTime.MinValue, DateTime.MaxValue)).FirstOrDefault() is UlazniERacun racun)
                {
                    await racun.DohvatiPdfAsync();
                    await racun.DohvatiUBLAsync();
                    Console.WriteLine($"{posrednik.GetType().Name}: Ulazni PDF + UBL OK");
                }
                else Console.WriteLine($"{posrednik.GetType().Name}: nema ulaznih računa");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: Ulazni PDF + UBL FAIL {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task OdbijPrviUlazniRacun()
    {
        foreach (var posrednik in posrednici)
        {
            try
            {
                if ((await posrednik.UlazniListAsync(DateTime.MinValue, DateTime.MaxValue)).FirstOrDefault() is UlazniERacun racun)
                {
                    await racun.OdbijAsync(RazlogOdbijanja.NeusklađenostKojaNeUtjeceNaObracunPoreza, "Krivi podaci o tvrtki");
                    Console.WriteLine($"{posrednik.GetType().Name}: OdbijRacun OK");
                }
                else Console.WriteLine($"{posrednik.GetType().Name}: nema ulaznih računa");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: OdbijRacun FAIL {ex.Message}");
            }
        }
    }
}