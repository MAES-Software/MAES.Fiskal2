
using MAES.Fiskal2.Posrednici;
using Xunit.Abstractions;

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
            OIB = "51560545524",
            Username = Environment.GetEnvironmentVariable("EPOSLOVANJE_USERNAME") ?? throw new InvalidOperationException("EPOSLOVANJE_USERNAME environment variable is not set."),
            Password = Environment.GetEnvironmentVariable("EPOSLOVANJE_PASSWORD") ?? throw new InvalidOperationException("EPOSLOVANJE_PASSWORD environment variable is not set."),
            ApiKey = Environment.GetEnvironmentVariable("EPOSLOVANJE_API_KEY") ?? throw new InvalidOperationException("EPOSLOVANJE_API_KEY environment variable is not set."),
            IsDev = true
        },
        new Doku
        {
            ApiKey = Environment.GetEnvironmentVariable("DOKU_API_KEY") ?? throw new InvalidOperationException("DOKU_API_KEY environment variable is not set."),
            IsDev = true
        }
    ];

    [Fact]
    public async Task EvidentirajUBL()
    {
        foreach (var posrednik in posrednici.Where(p => p is not Fina))
        {
            try
            {
                await posrednik.EvidentirajUBLAsync(File.ReadAllText("ubl.xml"));
                Console.WriteLine($"{posrednik.GetType().Name}: EvidentirajUBL OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: EvidentirajUBL FAIL");
                Console.WriteLine(ex.ToString());
            }
        }
    }

    [Fact]
    public async Task DohvatiIzlazneRacune()
    {
        foreach (var posrednik in posrednici.Where(p => p is not Fina))
        {
            try
            {
                var izlazni = await posrednik.IzlazniListAsync(
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow);

                Console.WriteLine($"{posrednik.GetType().Name}: IzlazniList OK ({izlazni.Count()})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: IzlazniList FAIL");
                Console.WriteLine(ex.ToString());
            }
        }
    }

    [Fact]
    public async Task DohvatiPrviIzlazniPdfIUBL()
    {
        foreach (var posrednik in posrednici.Where(p => p is not Fina))
        {
            try
            {
                var izlazni = await posrednik.IzlazniListAsync(
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow);

                var first = izlazni.FirstOrDefault();

                if (first == null)
                {
                    Console.WriteLine($"{posrednik.GetType().Name}: nema izlaznih računa");
                    continue;
                }

                await posrednik.IzlazniPdfAsync(first.Id);
                await posrednik.IzlazniUBLAsync(first.Id);

                Console.WriteLine($"{posrednik.GetType().Name}: Izlazni PDF + UBL OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: Izlazni PDF + UBL FAIL");
                Console.WriteLine(ex.ToString());
            }
        }
    }

    [Fact]
    public async Task EvidentirajUplatu()
    {
        foreach (var posrednik in posrednici.Where(p => p is not Fina))
        {
            try
            {
                var izlazni = await posrednik.IzlazniListAsync(
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow);

                var first = izlazni.FirstOrDefault();

                if (first == null)
                {
                    Console.WriteLine($"{posrednik.GetType().Name}: nema izlaznih računa za uplatu");
                    continue;
                }

                await posrednik.EvidentirajUplatuAsync(
                    first.Id,
                    DateTime.UtcNow,
                    100,
                    NacinPlacanja.TransakcijskiRaCun);

                Console.WriteLine($"{posrednik.GetType().Name}: EvidentirajUplatu OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: EvidentirajUplatu FAIL");
                Console.WriteLine(ex.ToString());
            }
        }
    }

    [Fact]
    public async Task UlazniRacuni()
    {
        foreach (var posrednik in posrednici.Where(p => p is not Fina))
        {
            try
            {
                var ulazni = await posrednik.UlazniListAsync(
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow);

                var first = ulazni.FirstOrDefault();

                if (first == null)
                {
                    Console.WriteLine($"{posrednik.GetType().Name}: nema ulaznih računa");
                    continue;
                }

                await posrednik.UlazniPdfAsync(first.Id);
                await posrednik.UlazniUBLAsync(first.Id);

                Console.WriteLine($"{posrednik.GetType().Name}: Ulazni PDF + UBL OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: Ulazni FAIL");
                Console.WriteLine(ex.ToString());
            }
        }
    }

    [Fact]
    public async Task OdbijPrviUlazniRacun()
    {
        foreach (var posrednik in posrednici.Where(p => p is not Fina))
        {
            try
            {
                var ulazni = await posrednik.UlazniListAsync(
                    DateTime.UtcNow.AddDays(-30),
                    DateTime.UtcNow);

                var first = ulazni.FirstOrDefault();

                if (first == null)
                {
                    Console.WriteLine($"{posrednik.GetType().Name}: nema ulaznih za odbijanje");
                    continue;
                }

                await posrednik.OdbijRacunAsync(
                    first.Id,
                    RazlogOdbijanja.NeusklađenostKojaNeUtjeceNaObracunPoreza,
                    "Nedostaje OIB");

                Console.WriteLine($"{posrednik.GetType().Name}: OdbijRacun OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{posrednik.GetType().Name}: OdbijRacun FAIL");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}