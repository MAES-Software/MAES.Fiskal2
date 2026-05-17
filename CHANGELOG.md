# Changelog

Sve promjene na ovom projektu su dokumentirane u ovoj datoteci.

## [0.1.0] - 2026-05-17

### Dodano
- Inicijalna javna verzija biblioteke
- Sučelje `IPosrednik` s 9 osnovnih metoda za rad s fiskalnim posrednicima
- Implementacija za `Super` posrednike s potpunom podrškom
- Implementacija za `EPoslovanje` posrednike (parcijalna podrška)
- Modeli `UlazniERacun` i `IzlazniERacun`
- Podrška za dohvat UBL/XML i PDF sadržaja
- Podrška za evidentiranje dokumenata, uplata i odbijanja računa
- MIT licenca
- Dokumentacija u `README.md`
- Smjernice za doprinošenje u `CONTRIBUTING.md`

### Poznati problemi
- `EPoslovanje` implementacija nije u potpunosti dovršena
- Status računa u `Super` implementaciji trebam doradu
- Nedostaju unit testovi

## Budući plan
- [ ] Dovršiti `EPoslovanje` implementaciju
- [ ] Dodati unit testove
- [ ] Implementacija za dodatne hrvatskih posrednike
- [ ] Detaljni primjeri korištenja
- [ ] Performanse optimizacije za bulk operacije
