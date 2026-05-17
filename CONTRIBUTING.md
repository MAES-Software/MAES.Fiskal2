# Doprinos MAES.Fiskal2 projektu

Hvala što razmišljaš o doprinošenju! Svi doprinosi su dobrodošli, bilo da se radi o izvještavanju grešaka, sugestijama za poboljšanja ili novim kodom.

## Kako započeti

1. Forkiraj repozitorij
2. Kreiraj granu za svoju značajku (`git checkout -b feature/tvoja-znacajka`)
3. Committed promjene s jasnom porukom (`git commit -m 'Dodaj novu značajku'`)
4. Pushaj granu (`git push origin feature/tvoja-znacajka`)
5. Kreiraj Pull Request s opisom promjena

## Izvještavanje grešaka

Ako pronađeš grešku, kreiraj Issue s:

- Opisom problema
- Koracima za reprodukciju
- Očekivanog i stvarnog ponašanja
- Verzije .NET-a i operativnog sustava

## Pull Request smjernice

- Osiguraj da kod kompajlira bez upozorenja
- Dodaj komentare za kompleksnije dijelove koda
- Ažuriraj `README.md` ako je potrebno
- Testiraj s oba posrednika ako je moguće (`Super` i `EPoslovanje`)

## Razvojne smjernice

### Struktura projekta
- `IPosrednik.cs` — Definira sučelje
- `Posrednici/` — Implementacije za različite posrednike
- `UlazniERacun.cs`, `IzlazniERacun.cs` — Modeli podataka

### Kodne smjernice
- Koristi C# 13 značajke (jer je .NET 10)
- Async/await gdje je primjenjivo
- XML dokumentaciju za javne članove
- Rukovanje greškama kroz iznimke ili rezultate

## Licenca

Svi doprinosi su licencirani pod MIT licencom.
