# MAES.Fiskal2

MAES.Fiskal2 je C# biblioteka za rad s Hrvatskim fiskalnim posrednicima. Cilj projekta je izraditi zajednički sloj za sve posrednike koji podržavaju razmjenu ulaznih i izlaznih e-računa u C#.

## Što projekt radi

Projekt definira zajedničko sučelje `IPosrednik` koje opisuje osnovne operacije za posrednike fiskalizacije:

- dohvat ulaznih i izlaznih e-računa
- dohvat UBL i PDF sadržaja računa
- evidentiranje UBL dokumenta
- evidentiranje uplate
- odbijanje računa

Modeli `UlazniERacun` i `IzlazniERacun` predstavljaju minimalne informacije o računu, uključujući OIB partnera, adresu, datum i status.

## Trenutno podržani posrednici

U `Posrednici/` direktoriju nalaze se konkretne implementacije

| Značajka / posrednik | `Super` | `EPoslovanje` | `Fina` | `bizBox` | `Editel` |
|---|:---:|:---:|:---:|:---:|:---:|
| Dohvat ulaznih e-računa | ✅ | ❌ | ❌ | ❌ | ❌ |
| Dohvat izlaznih e-računa | ✅ | ❌ | ❌ | ❌ | ❌ |
| Dohvat UBL sadržaja | ✅ | ❌ | ❌ | ❌ | ❌ |
| Dohvat PDF sadržaja | ✅ | ❌ | ❌ | ❌ | ❌ |
| Evidentiranje UBL dokumenta | ✅ | ✅* | ❌ | ❌ | ❌ |
| Evidentiranje uplate | ✅ | ❌ | ❌ | ❌ | ❌ |
| Odbijanje računa | ✅ | ❌ | ❌ | ❌ | ❌ |

## Zahtjevi

- .NET Standard 2.0

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

## Kako koristiti

Primjer inicijalizacije posrednika:

```csharp
using MAES.Fiskal2.Posrednici;

var posrednik = new Super
{
    IsDev = true,
    BusinessGuid = "...",
    Username = "...",
    Password = "..."
};

var racuni = await posrednik.UlazniListAsync(DateTime.Today.AddDays(-7), DateTime.Today);
```

## Dostupne metode

Sučelje `IPosrednik` nudi sljedeće metode:

### Dohvat ulaznih e-računa
- `Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to, CancellationToken token = default)`
    
    Dohvaća popis ulaznih računa u vremenskom rasponu

- `Task<string> UlazniUBLAsync(string id, CancellationToken token = default)`
    
    Dohvaća XML/UBL sadržaj ulaznog računa

- `Task<byte[]> UlazniPdfAsync(string id, CancellationToken token = default)`

    Dohvaća PDF sadržaj ulaznog računa

### Dohvat izlaznih e-računa
- `Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to, CancellationToken token = default)`

    Dohvaća popis izlaznih računa u vremenskom rasponu

- `Task<string> IzlazniUBLAsync(string id, CancellationToken token = default)`

    Dohvaća XML/UBL sadržaj izlaznog računa

- `Task<byte[]> IzlazniPdfAsync(string id, CancellationToken token = default)`

    Dohvaća PDF sadržaj izlaznog računa


### Operacije na računima
- `Task EvidentirajUBLAsync(string ubl, CancellationToken token = default)`

    Evidentira UBL dokument
    
- `Task EvidentirajUplatuAsync(string id, CancellationToken token = default)`

    Evidentira uplatu za račun

- `Task OdbijRacunAsync(string id, CancellationToken token = default)`

    Odbija račun

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

## Licenca

Ovaj projekt je licenciran pod MIT licencom. Vidjeti [LICENSE](LICENSE) datoteku za više detalja.
