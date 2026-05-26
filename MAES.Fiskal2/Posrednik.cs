using System.Text.Json.Serialization;
using MAES.Fiskal2.Posrednici;

namespace MAES.Fiskal2;

/// <summary>
/// Bazna implementacija posrednika za komunikaciju sa servisom fiskalizacije / eRačuna.
/// Sadrži zajedničke URI postavke i definira osnovne operacije za rad s ulaznim i izlaznim eRačunima.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Super), "Super")]
[JsonDerivedType(typeof(EPoslovanje), "EPoslovanje")]
[JsonDerivedType(typeof(Fina), "Fina")]
public abstract class Posrednik
{
    /// <summary>
    /// Označava koristi li se razvojno okruženje.
    /// Ako je <c>true</c>, koristi se <see cref="UriDev"/>; inače <see cref="UriProd"/>.
    /// </summary>
    public bool IsDev { get; set; }

    /// <summary>
    /// Produkcijski URI servisa.
    /// </summary>
    protected string UriProd { get; set; } = "";

    /// <summary>
    /// URI razvojnog (testnog) okruženja servisa.
    /// </summary>
    protected string UriDev { get; set; } = "";

    /// <summary>
    /// Aktivni URI servisa ovisno o odabranom okruženju.
    /// </summary>
    protected string Uri => IsDev ? UriDev : UriProd;

    /// <summary>
    /// Evidentira izlazni eRačun na temelju UBL sadržaja.
    /// </summary>
    /// <param name="ubl">UBL XML sadržaj računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    public virtual Task EvidentirajUBLAsync(string ubl, CancellationToken token = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Evidentira uplatu za postojeći račun.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="date">Datum i vrijeme uplate.</param>
    /// <param name="amount">Iznos uplate.</param>
    /// <param name="paymentMethod">Način plaćanja.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    public virtual Task EvidentirajUplatuAsync(
        string id,
        DateTime date,
        double amount,
        NacinPlacanja paymentMethod,
        CancellationToken token = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Dohvaća popis izlaznih eRačuna unutar zadanog razdoblja.
    /// </summary>
    /// <param name="from">Početni datum razdoblja.</param>
    /// <param name="to">Završni datum razdoblja.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Kolekcija izlaznih eRačuna.</returns>
    public virtual Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(
        DateTime from,
        DateTime to,
        CancellationToken token = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Dohvaća PDF prikaz izlaznog računa.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Sadržaj PDF dokumenta kao niz bajtova.</returns>
    public virtual Task<byte[]> IzlazniPdfAsync(
        string id,
        CancellationToken token = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Dohvaća UBL XML izlaznog računa.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>UBL XML sadržaj računa.</returns>
    public virtual Task<string> IzlazniUBLAsync(
        string id,
        CancellationToken token = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Odbija račun uz navedeni razlog i opis.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="razlog">Razlog odbijanja.</param>
    /// <param name="opis">Dodatni opis odbijanja.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    public virtual Task OdbijRacunAsync(
        string id,
        RazlogOdbijanja razlog,
        string opis,
        CancellationToken token = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Dohvaća popis ulaznih eRačuna unutar zadanog razdoblja.
    /// </summary>
    /// <param name="from">Početni datum razdoblja.</param>
    /// <param name="to">Završni datum razdoblja.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Kolekcija ulaznih eRačuna.</returns>
    public virtual Task<IEnumerable<UlazniERacun>> UlazniListAsync(
        DateTime from,
        DateTime to,
        CancellationToken token = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Dohvaća PDF prikaz ulaznog računa.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Sadržaj PDF dokumenta kao niz bajtova.</returns>
    public virtual Task<byte[]> UlazniPdfAsync(
        string id,
        CancellationToken token = default) =>
        throw new NotImplementedException();

    /// <summary>
    /// Dohvaća UBL XML ulaznog računa.
    /// </summary>
    /// <param name="id">Identifikator računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>UBL XML sadržaj računa.</returns>
    public virtual Task<string> UlazniUBLAsync(
        string id,
        CancellationToken token = default) =>
        throw new NotImplementedException();
}