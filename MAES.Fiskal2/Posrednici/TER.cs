using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MAES.Fiskal2.Posrednici;

/// <summary>
/// Implementacija posrednika za Tvoj eRačun. https://ter.hr/api-za-fiskalizaciju/
/// </summary>
public class TER : Posrednik
{
    /// <summary>
    /// Konstruktor za inicijalizaciju TER posrednika.
    /// </summary>
    public TER()
    {
        //UriProd = "https://api.ter.hr";
        //UriDev = "https://api-test.ter.hr";
    }
}