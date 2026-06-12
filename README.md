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

| Značajka / posrednik | `Super` | `EPoslovanje` | `Fina` | `MER` | `Redok` | `Tvoj eRačun` | `Doku` |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Dohvat ulaznih računa | ✅ | ✅ | ❌ | ✅ | 🚧 | 🚧 | ⚠️ |
| Dohvat izlaznih računa | ✅ | ✅ | ❌ | ✅ | 🚧 | 🚧 | ⚠️ |
| Dohvat UBL sadržaja | ✅ | ✅ | ❌ | ⚠️ | ✅ | 🚧 | ⚠️ |
| Dohvat PDF sadržaja | ✅ | ✅ | ❌ | ❌ | ❌ | 🚧 | ❌ |
| Evidentiranje UBL dokumenta | ✅ | ✅ | ⚠️ | ✅ | 🚧 | 🚧 | ⚠️ |
| Evidentiranje uplate | ✅ | ✅ | ❌ | 🚧 | ✅ | 🚧 | ⚠️ |
| Odbijanje računa | ✅ | ✅ | ❌ | 🚧 | ✅ | 🚧 | ⚠️ |
| Prihvaćanje računa | ❌ | 🚧 | ❌ | ✅ | 🚧 | 🚧 | 🚧 |

* ✅ — Implementirano
* ⚠️ — Nije testirano/Ne prolazi testove
* 🚧 — Nije još implementirano
* ❌ — Posrednik ne podržava

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
var posrednik = new Super
{
    IsDev = true,
    BusinessGuid = "...",
    Username = "...",
    Password = "..."
};

// ePoslovanje
var posrednik = new EPoslovanje
{
    IsDev = true,
    OIB = "...",
    Username = "...",
    Password = "..."
};

// Fina
var posrednik = new Fina
{
    IsDev = true,
    OIB = "...",
    Certificate = ...
};

// Moj eRačun
var posrednik = new MER
{
    IsDev = true,
    Username = "...",
    Password = "...",
    OIB = "..."
};

// Moj eRačun
var posrednik = new MER
{
    Username = "...",
    Password = "...",
    SoftwareId = "...",
    CompanyId = "..."
};
```

### Primjer korištenja posrednika

```csharp
// dohvat računa u razdoblju zadnjih mj. dana
var racuni = posrednik.UlazniListAsync(DateTime.Now.AddMonths(-1), DateTime.Now);

var racun = racuni.FirstOrDefault();
if(racun != null)
{
    // dohvat ubl stringa i pdf byteova
    string ubl = await posrednik.UlazniUBLAsync(racun.Id);
    byte[] pdf = await posrednik.UlazniPdfAsync(racun.Id);
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

- `Task<IEnumerable<UlazniERacun>> UlazniListAsync(int page, int pageSize)`
    
    Dohvaća popis ulaznih računa (pagination)

- `Task<string> UlazniUBLAsync(string id)`
    
    Dohvaća XML/UBL sadržaj ulaznog računa

- `Task<byte[]> UlazniPdfAsync(string id)`

    Dohvaća PDF sadržaj ulaznog računa

### Dohvat izlaznih e-računa
- `Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to)`

    Dohvaća popis izlaznih računa u vremenskom rasponu

- `Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(int page, int pageSize)`
    
    Dohvaća popis izlaznih računa (pagination)

- `Task<string> IzlazniUBLAsync(string id)`

    Dohvaća XML/UBL sadržaj izlaznog računa

- `Task<byte[]> IzlazniPdfAsync(string id)`

    Dohvaća PDF sadržaj izlaznog računa


### Operacije na računima
- `Task EvidentirajUBLAsync(string ubl)`

    Evidentira UBL dokument
    
- `Task EvidentirajUplatuAsync(string id, DateTime datum, double iznos, NacinPlacanja nacinPlacanja)`

    Evidentira uplatu za račun

- `Task OdbijRacunAsync(string id, RazlogOdbijana razlog, string opis)`

    Odbija ulazni eRačun

- `Task PrihvatiRacunAsync(string id)`

    Prihvaća ulazni eRačun

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
