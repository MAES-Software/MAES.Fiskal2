# MAES.Fiskal2

[![CI/CD](https://github.com/MAES-Software/MAES.Fiskal2/actions/workflows/main.yml/badge.svg)](https://github.com/MAES-Software/MAES.Fiskal2/actions/workflows/main.yml)
![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-512bd4?logo=dotnet)
[![NuGet](https://img.shields.io/nuget/v/MAES.Fiskal2.svg)](https://www.nuget.org/packages/MAES.Fiskal2/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MAES.Fiskal2)](https://www.nuget.org/packages/MAES.Fiskal2/)

MAES.Fiskal2 je C# biblioteka za rad s hrvatskim fiskalnim posrednicima. Omogućuje zajednički sloj za slanje, dohvat i obradu ulaznih i izlaznih e-računa preko različitih posrednika.

## Što projekt radi

Biblioteka definira baznu klasu `Posrednik` i konkretne implementacije za najčešće hrvatske fiskalne posrednike.

Podržane operacije uključuju:

- dohvat ulaznih i izlaznih e-računa
- dohvat UBL/XML i PDF sadržaja računa
- evidentiranje UBL dokumenta
- evidentiranje uplate za izlazni račun
- odbijanje ulaznog računa

Osim toga, postoje i modeli `ERacun`, `UlazniERacun` i `IzlazniERacun` koji sadrže osnovne podatke o računu i pružaju jednostavne pomoćne metode za rad s njima.

## Trenutno podržani posrednici

U `Posrednici/` direktoriju nalaze se konkretne implementacije

| Značajka / posrednik | `Super` | `EPoslovanje` | `Fina` | `MER` |
|---|:---:|:---:|:---:|:---:|
| Dohvat ulaznih računa | ✅ | ✅ | ❌ | ✅ |
| Dohvat izlaznih računa | ✅ | ✅ | ❌ | ✅ |
| Dohvat UBL sadržaja | ✅ | ✅ | ❌ | ✅ |
| Dohvat PDF sadržaja | ✅ | ✅ | ❌ | ❌ |
| Evidentiranje UBL dokumenta | ✅ | ✅ | ⚠️ | ✅ |
| Evidentiranje uplate | ✅ | ✅ | ❌ | ✅ |
| Odbijanje računa | ✅ | ✅ | ❌ | ✅ |

✅ Implementirano
⚠️ Nije testirano/Ne prolazi testove
🚧 WIP
❌ Posrednik ne podržava

## Instalacija

Instalirajte paket iz NuGeta:

```bash
dotnet add package MAES.Fiskal2
```

Ili dodajte referencu direktno u `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="MAES.Fiskal2" Version="*" />
</ItemGroup>
```

## Inicijalizacija posrednika

```csharp
using MAES.Fiskal2.Posrednici;

var super = new Super
{
    IsDev = true,
    BusinessGuid = "...",
    Username = "...",
    Password = "..."
};

var ePoslovanje = new EPoslovanje
{
    IsDev = true,
    ApiKey = "..."
};

var fina = new Fina
{
    Certificate = ...
};

var mer = new MER
{
    IsDev = true,
    Username = "...",
    Password = "...",
    CompanyId = "...",
    SoftwareId = "..."
};
```

## Primjer korištenja posrednika

```csharp
// dohvat ulaznih računa u razdoblju zadnjih mj. dana
var ulazniRacuni = posrednik.UlazniListAsync(DateTime.Now.AddMonths(-1), DateTime.Now);

if(ulazniRacuni.FirstOrDefault() is UlazniERacun ulazni)
{
    // dohvat ubl stringa i pdf byteova
    string ubl = await ulazni.DohvatiUBLAsync();
    byte[] pdf = await ulazni.DohvatiPdfAsync();

    // odbija ulazni račun
    await ulazni.OdbijAsync(RazlogOdbijanja.Ostalo, "Račun odbijen radi necega");
}

// dohvat izlaznih računa u razdoblju zadnjih mj. dana
var izlazniRacuni = posrednik.IzlazniListAsync(DateTime.Now.AddMonths(-1), DateTime.Now);

if(izlazniRacuni.FirstOrDefault() is IzlazniERacun izlazni)
{
    // dohvat ubl stringa i pdf byteova
    string ubl = await izlazni.DohvatiUBLAsync();
    byte[] pdf = await izlazni.DohvatiPdfAsync();

    // evidentira uplatu za izlazni račun
    await izlazni.EvidentirajUplatuAsync(DateTime.Now, 100, ERacunNacinPlacanja.TransakcijskimRacunom);
}

// evidentiranje računa
posrednik.EvidentirajUBLAsync(ubl);
```

> Neki posrednici nemaju podržane sve metode, neki nemaju sve fieldove u modelima tipa UlazniERacun i sl. Mora se voditi računa o tome...

## Dostupne metode

> Svaka metoda ima na kraju CancellationToken kojeg je poželjno postaviti, ali se može izostaviti

Abstraktna klasa `Posrednik` nudi sljedeće metode:

### Dohvat ulaznih e-računa
- `Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to)`
    
    Dohvaća popis ulaznih računa u vremenskom rasponu

- `Task<string> UlazniUBLAsync(string id)`
    
    Dohvaća XML/UBL sadržaj ulaznog računa

- `Task<byte[]> UlazniPdfAsync(string id)`

    Dohvaća PDF sadržaj ulaznog računa

### Dohvat izlaznih e-računa
- `Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to)`

    Dohvaća popis izlaznih računa u vremenskom rasponu

- `Task<string> IzlazniUBLAsync(string id)`

    Dohvaća XML/UBL sadržaj izlaznog računa

- `Task<byte[]> IzlazniPdfAsync(string id)`

    Dohvaća PDF sadržaj izlaznog računa


### Operacije na računima
- `Task EvidentirajUBLAsync(string ubl)`

    Evidentira UBL dokument
    
- `Task EvidentirajUplatuAsync(string id, DateTime datum, double iznos, NacinPlacanja nacinPlacanja)`

    Evidentira uplatu za izlazni račun

- `Task OdbijRacunAsync(string id, RazlogOdbijana razlog, string opis)`

    Odbija ulazni eRačun

UlazniERacun i IzlazniERacun imaju na sebi metode koje su vezane za taj specifičan e-račun (tipa izlazni ima izlazni.EvidentirajUplatuAsync, i sl.)

## Modeli i enum-ovi

- `ERacun` – zajednička baza za sve račune
- `UlazniERacun` – ulazni račun s dodatnim statusom i metodama za odbijanje
- `IzlazniERacun` – izlazni račun s dodatnim statusom i metodom za evidentiranje uplate
- `ERacunNacinPlacanja` – načini plaćanja
- `UlazniERacunStatus` / `IzlazniERacunStatus` – statusi računa
- `RazlogOdbijanja` – razlozi odbijanja ulaznog računa

## Dodatne informacije

### Spremanje posrednika u listu

Budući da sve konkretne implementacije nasljeđuju `Posrednik`, možete ih spremati u običan `List<Posrednik>`:

```csharp
using MAES.Fiskal2;
using MAES.Fiskal2.Posrednici;

var posrednici = new List<Posrednik>
{
    new Super
    {
        IsDev = true,
        BusinessGuid = "BUSINESS_GUID",
        Username = "USERNAME",
        Password = "PASSWORD"
    },
    new EPoslovanje
    {
        IsDev = true,
        ApiKey = "API_KEY"
    }
};

foreach (var posrednik in posrednici)
{
    Console.WriteLine(posrednik.GetType().Name);
}
```

Ovo je korisno ako želite voditi više konfiguracija posrednika u jednoj strukturi i pozivati iste metode nad njima kroz baznu klasu.

### Serializacija u JSON

Biblioteka koristi `System.Text.Json` i baza `Posrednik` je označena sa `JsonPolymorphic`/`JsonDerivedType`, što znači da možete lako spremiti i učitati različite tipove posrednika u JSON.

```csharp
using System.Text.Json;
using MAES.Fiskal2;
using MAES.Fiskal2.Posrednici;

var options = new JsonSerializerOptions
{
    WriteIndented = true
};

var posrednici = new List<Posrednik>
{
    new Super
    {
        IsDev = true,
        BusinessGuid = "BUSINESS_GUID",
        Username = "USERNAME",
        Password = "PASSWORD"
    }
};

string json = JsonSerializer.Serialize(posrednici, options);
File.WriteAllText("posrednici.json", json);

var loaded = JsonSerializer.Deserialize<List<Posrednik>>(json, options);
```

U izlaznom JSON-u bit će prisutan polje `$type`, pa se prilikom deserijalizacije zna točno koji konkretni tip posrednika treba vratiti.

### Dodatne napomene

- `IsDev` određuje koristi li se razvojno ili produkcijsko okruženje.
- Sve glavne metode podržavaju `CancellationToken` kao zadnji parametar.
- Za `Fina` je potreban X.509 certifikat, a podrška za dohvat računa i PDF-a nije dostupna.
- Ako koristite `ERacun` objekte, možete direktno pozivati `DohvatiUBLAsync()` i `DohvatiPdfAsync()` bez da svaki put ručno prolazite kroz posrednika.

## Izgradnja i pakiranje

```bash
dotnet build MAES.Fiskal2.csproj --configuration Release
```

```bash
dotnet pack MAES.Fiskal2.csproj --configuration Release
```

