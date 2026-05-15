using System.Text.Json.Serialization;
using MAES.Fiskal2.Posrednici;

namespace MAES.Fiskal2;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Super), "Super")]
[JsonDerivedType(typeof(EPoslovanje), "EPoslovanje")]
public interface IPosrednik
{
    public Task<IEnumerable<UlazniERacun>> UlazniAsync(DateTime from, DateTime to);
    public Task<IEnumerable<IzlazniERacun>> IzlazniAsync(DateTime from, DateTime to);
    public Task<Guid> EvidentirajUBLAsync(string ubl);
    public Task<byte[]> DohvatiPdfAsync(string id, CancellationToken token);
}