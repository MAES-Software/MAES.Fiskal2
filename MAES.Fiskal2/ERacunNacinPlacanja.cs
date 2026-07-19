namespace MAES.Fiskal2;

/// <summary>
/// Mogući načini plaćanja za evidentiranje uplate izlaznog računa.
/// </summary>
public enum ERacunNacinPlacanja
{
    /// <summary>
    /// Transakcijski račun.
    /// </summary>
    TransakcijskiRacun = 1,

    /// <summary>
    /// Obračunsko plaćanje.
    /// </summary>
    ObračunskoPlaćanje = 2,

    /// <summary>
    /// Ostalo.
    /// </summary>
    Ostalo = 11
}