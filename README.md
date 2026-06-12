# MAES.Fiskal2

[![CI/CD](https://github.com/MAES-Software/MAES.Fiskal2/actions/workflows/main.yml/badge.svg)](https://github.com/MAES-Software/MAES.Fiskal2/actions/workflows/main.yml)
[![Contributors](https://img.shields.io/github/contributors/MAES-Software/MAES.Fiskal2)](https://github.com/MAES-Software/MAES.Fiskal2/graphs/contributors)
[![Issues](https://img.shields.io/github/issues/MAES-Software/MAES.Fiskal2)](https://github.com/MAES-Software/MAES.Fiskal2/issues)
[![NuGet](https://img.shields.io/nuget/v/MAES.Fiskal2.svg)](https://www.nuget.org/packages/MAES.Fiskal2/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MAES.Fiskal2)](https://www.nuget.org/packages/MAES.Fiskal2/)

MAES.Fiskal2 je C# biblioteka za rad s Hrvatskim fiskalnim posrednicima. Cilj projekta je izraditi zajednički sloj za sve posrednike koji podržavaju razmjenu ulaznih i izlaznih e-računa u C#.

## Što projekt radi

Projekt definira zajedničku abstraktnu klasu `Posrednik` koja opisuje osnovne operacije za posrednike fiskalizacije:

- dohvat ulaznih i izlaznih e-računa
- dohvat UBL i PDF sadržaja računa
- evidentiranje UBL dokumenta
- evidentiranje uplate
- odbijanje računa

Modeli `UlazniERacun` i `IzlazniERacun` predstavljaju minimalne informacije o računu, uključujući OIB partnera, adresu, datum i status.

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

Ili direktno u datoteku `.csproj`:

```xml
<ItemGroup>
    <PackageReference Include="MAES.Fiskal2" Version="*" />
</ItemGroup>
```

## Inicijalizacija posrednika

```csharp
using MAES.Fiskal2.Posrednici;

// Super.hr
var super = new Super
{
    IsDev = true,
    BusinessGuid = "...",
    Username = "...",
    Password = "..."
};

// ePoslovanje
var eposl = new EPoslovanje
{
    IsDev = true,
    OIB = "...",
    Username = "...",
    Password = "..."
};

// Fina
var fina = new Fina
{
    Certificate = ...
};

// Moj eRačun
var mer = new MER
{
    IsDev = true,
    Username = "...",
    Password = "...",
    OIB = "..."
};
```

## Primjer korištenja posrednika

```csharp
// dohvat ulaznih računa u razdoblju zadnjih mj. dana
var ulazniRacuni = posrednik.UlazniListAsync(DateTime.Now.AddMonths(-1), DateTime.Now);

var ulazniRacun = ulazniRacuni.FirstOrDefault();
if(ulazniRacun != null)
{
    // dohvat ubl stringa i pdf byteova
    string ubl = await posrednik.UlazniUBLAsync(ulazniRacun.Id);
    byte[] pdf = await posrednik.UlazniPdfAsync(ulazniRacun.Id);

    // odbija ulazni račun
    await posrednik.OdbijRacunAsync(ulazniRacun.Id);
}

// dohvat izlaznih računa u razdoblju zadnjih mj. dana
var izlazniRacuni = posrednik.IzlazniListAsync(DateTime.Now.AddMonths(-1), DateTime.Now);

var izlazniRacun = izlazniRacuni.FirstOrDefault();
if(izlazniRacun != null)
{
    // dohvat ubl stringa i pdf byteova
    string ubl = await posrednik.IzlazniUBLAsync(izlazniRacun.Id);
    byte[] pdf = await posrednik.IzlazniPdfAsync(izlazniRacun.Id);

    // evidentira uplatu za izlazni račun
    await posrednik.EvidentirajUplatuAsync(izlazniRacun.Id);
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

## Izgradnja i pakiranje

Za izgradnju upotrijebite:

```bash
dotnet build MAES.Fiskal2.csproj --configuration Release
```

Ako želite napraviti NuGet paket:

```bash
dotnet pack MAES.Fiskal2.csproj --configuration Release
```

## Napomene

- Projekt je u razvoju.
- Neke metode još nisu implementirane.
- Trenutna sučelja i model podataka mogu se mijenjati dok se dovršava podrška za različite posrednike.
