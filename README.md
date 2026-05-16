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

Obje klase implementiraju `IPosrednik`, ali su neke metode još uvijek označene kao `NotImplementedException`. Trenutna implementacija uključuje osnovne HTTP zahtjeve i autorizaciju, ali još treba dovršiti potpuni rad s UBL dokumentima, PDF-om i statusima.

## Struktura projekta

- `IPosrednik.cs` — zajedničko sučelje za sve fiskalne posrednike
- `UlazniERacun.cs` — model ulaznog računa
- `IzlazniERacun.cs` — model izlaznog računa
- `Posrednici/EPoslovanje.cs` — implementacija za ePoslovanje
- `Posrednici/Super.cs` — implementacija za Super
- `MAES.Fiskal2.csproj` — .NET 10 projekt

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

Repository trenutno nema definiranu licencu. Preporučuje se dodavanje `LICENSE` datoteke ako planirate javno objaviti projekt.
