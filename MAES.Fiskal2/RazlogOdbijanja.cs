namespace MAES.Fiskal2;

/// <summary>
/// Mogući razlozi odbijanja ulaznog računa u procesu fiskalizacije.
/// </summary>
public enum RazlogOdbijanja
{
    /// <summary>
    /// Neusklađenost koja ne utječe na obračun poreza.
    /// </summary>
    NeusklađenostKojaNeUtjeceNaObracunPoreza = 1,

    /// <summary>
    /// Neusklađenost koja utječe na obračun poreza.
    /// </summary>
    NeusklađenostKojaUtjeceNaObracunPoreza = 2,

    /// <summary>
    /// Ostalo.
    /// </summary>
    Ostalo = 11
}