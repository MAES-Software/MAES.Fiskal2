using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MAES.Fiskal2.Posrednici;

/// <summary>
/// Implementacija posrednika za AdeoPOS. https://adeopos.hr/api-za-fiskalizaciju/
/// </summary>
public class AdeoPOS : Posrednik
{
    /// <summary>
    /// Konstruktor za inicijalizaciju AdeoPOS posrednika.
    /// </summary>
    public AdeoPOS()
    {
        //UriProd = "https://api.adeopos.hr";
        //UriDev = "https://api-test.adeopos.hr";
    }
}