using MAES.Fiskal2;
using MAES.Fiskal2.Posrednici;

// Inicijalizacija posrednika
Super super = new()
{
    IsDev = true,
    BusinessGuid = "YOUR_BUSINESS_GUID",
    Username = "YOUR_EMAIL",
    Password = "YOUR_PASSWORD"
};

string ubl = File.ReadAllText("ubl.xml");

// 1) Pošalji UBL dokument
await super.EvidentirajUBLAsync(ubl);
Console.WriteLine("Sent UBL document.");

// 2) Dohvati ulazne račune
var ulazniRacuni = await super.UlazniListAsync(DateTime.Now.AddDays(-30), DateTime.Now);

// 3) Dohvati UBL, pdf, odbij ulazni račun
if(ulazniRacuni.FirstOrDefault() is UlazniERacun ulazni)
{
    // 3) Dohvati UBL ulaznog e-računa
    string ulazniUbl = await ulazni.DohvatiUBLAsync();

    // 4) Dohvati pdf ulaznog e-računa
    byte[] ulazniPdf = await ulazni.DohvatiPdfAsync();

    // 5) Odbij račun
    await ulazni.OdbijAsync(RazlogOdbijanja.Ostalo, "Primjer odbijanja ulaznog računa");
}

// 6) Dohvati izlazne račune
var izlazniRacuni = await super.IzlazniListAsync(DateTime.Now.AddDays(-30), DateTime.Now);
if (izlazniRacuni.FirstOrDefault() is IzlazniERacun izlazni)
{
    // 3) Dohvati UBL izlaznog e-računa
    string izlazniUbl = await izlazni.DohvatiUBLAsync();

    // 4) Dohvati pdf izlaznog e-računa
    byte[] izlazniPdf = await izlazni.DohvatiPdfAsync();

    // 7) Evidentiraj uplatu izlaznog računa
    await izlazni.EvidentirajUplatuAsync(DateTime.Now, 100, ERacunNacinPlacanja.TransakcijskiRacun);
}