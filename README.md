# MAES.Fiskal2

MAES.Fiskal2 je C# biblioteka za rad s fiskalnim posrednicima hrvatskog e-fiskalizacijskog sustava. Cilj projekta je izraditi zajednički zajednički (common) sloj za sve posrednike koji podržavaju razmjenu ulaznih i izlaznih e-računa.

## Što projekt radi

Projekt definira zajedničko sučelje `IPosrednik` koje opisuje osnovne operacije za posrednike fiskalizacije:

- dohvat ulaznih i izlaznih e-računa
- dohvat UBL/XML i PDF sadržaja računa
- evidentiranje UBL dokumenta
- evidentiranje uplate
- odbijanje računa

Modeli `UlazniERacun` i `IzlazniERacun` predstavljaju minimalne informacije o računu, uključujući OIB partnera, adresu, datum i status.

## Trenutno podržani posrednici

U `Posrednici/` direktoriju nalaze se konkretne implementacije:

- `EPoslovanje` — rad s API-jem ePoslovanje.hr
- `Super` — rad s API-jem Super.hr

| Značajka / posrednik | `Super` | `EPoslovanje` | `Fina` | `bizBox` | `Editel` |
|---|:---:|:---:|:---:|:---:|:---:|
| Dohvat ulaznih e-računa | ✅ | ❌ | ❌ | ❌ | ❌ |
| Dohvat izlaznih e-računa | ✅ | ❌ | ❌ | ❌ | ❌ |
| Dohvat UBL/XML sadržaja | ✅ | ❌ | ❌ | ❌ | ❌ |
| Dohvat PDF sadržaja | ✅ | ❌ | ❌ | ❌ | ❌ |
| Evidentiranje UBL dokumenta | ✅ | ✅* | ❌ | ❌ | ❌ |
| Evidentiranje uplate | ✅ | ❌ | ❌ | ❌ | ❌ |
| Odbijanje računa | ✅ | ❌ | ❌ | ❌ | ❌ |

> `Super` i `EPoslovanje` su jedini trenutno implementirani posrednici u ovom repozitoriju. Ostali navedeni pružatelji su značajni u Hrvatskoj, ali za njih još nije dodana podrška.

## Zahtjevi

- .NET SDK 10

## Kako koristiti

1. Klonirajte repo.
2. Otvorite projekt u Visual Studio ili drugom .NET IDE-u.
3. Izgradite s `dotnet build`.
4. Kreirajte instancu odgovarajućeg posrednika i implementirajte pozive prema `IPosrednik`.

Primjer inicijalizacije posrednika:

```csharp
var posrednik = new MAES.Fiskal2.Posrednici.Super
{
    IsDev = true,
    BusinessGuid = "...",
    Username = "...",
    Password = "..."
};

var racuni = await posrednik.UlazniListAsync(DateTime.Today.AddDays(-7), DateTime.Today);
```

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
