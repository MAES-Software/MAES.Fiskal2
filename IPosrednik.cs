using System.Text.Json.Serialization;
using MAES.Fiskal2.Posrednici;

namespace MAES.Fiskal2;

/// <summary>
/// Sučelje koje opisuje osnovne operacije za posrednike hrvatskog e-fiskalizacijskog sustava.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Super), "Super")]
[JsonDerivedType(typeof(EPoslovanje), "EPoslovanje")]
public interface IPosrednik
{
    /// <summary>
    /// Dohvaća XML/UBL sadržaj ulaznog računa po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator ulaznog računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>XML/UBL sadržaj računa kao tekst.</returns>
    public Task<string> UlazniUBLAsync(string id, CancellationToken token = default);

    /// <summary>
    /// Dohvaća PDF sadržaj ulaznog računa po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator ulaznog računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>PDF sadržaj računa kao bajtni niz.</returns>
    public Task<byte[]> UlazniPdfAsync(string id, CancellationToken token = default);

    /// <summary>
    /// Dohvaća popis ulaznih e-računa u zadanom vremenskom rasponu.
    /// </summary>
    /// <param name="from">Početni datum vremenskog raspona.</param>
    /// <param name="to">Krajnji datum vremenskog raspona.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Popis ulaznih e-računa.</returns>
    public Task<IEnumerable<UlazniERacun>> UlazniListAsync(DateTime from, DateTime to, CancellationToken token = default);

    /// <summary>
    /// Dohvaća XML/UBL sadržaj izlaznog računa po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator izlaznog računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>XML/UBL sadržaj računa kao tekst.</returns>
    public Task<string> IzlazniUBLAsync(string id, CancellationToken token = default);

    /// <summary>
    /// Dohvaća PDF sadržaj izlaznog računa po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator izlaznog računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>PDF sadržaj računa kao bajtni niz.</returns>
    public Task<byte[]> IzlazniPdfAsync(string id, CancellationToken token = default);

    /// <summary>
    /// Dohvaća popis izlaznih e-računa u zadanom vremenskom rasponu.
    /// </summary>
    /// <param name="from">Početni datum vremenskog raspona.</param>
    /// <param name="to">Krajnji datum vremenskog raspona.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    /// <returns>Popis izlaznih e-računa.</returns>
    public Task<IEnumerable<IzlazniERacun>> IzlazniListAsync(DateTime from, DateTime to, CancellationToken token = default);

    /// <summary>
    /// Evidentira ulazni UBL dokument u fiskalizacijski sustav.
    /// </summary>
    /// <param name="ubl">UBL XML dokument ulaznog računa.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    public Task EvidentirajUBLAsync(string ubl, CancellationToken token = default);

    /// <summary>
    /// Evidentira uplatu za račun po njegovom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator računa čiju uplatu treba evidentirati.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    public Task EvidentirajUplatuAsync(string id, CancellationToken token = default);

    /// <summary>
    /// Odbija račun u fiskalizacijskom procesu prema zadanom identifikatoru.
    /// </summary>
    /// <param name="id">Identifikator računa koji se odbija.</param>
    /// <param name="token">Token za otkazivanje operacije.</param>
    public Task OdbijRacunAsync(string id, CancellationToken token = default);
}