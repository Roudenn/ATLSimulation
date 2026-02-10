namespace Content.Shared.Constants;

/// <summary>
///     Contains physical constants used in calculations.
/// </summary>
public static class PhysicalConstants
{
    /// <summary>
    ///     The universal gas constant, in kPa*L/(K*mol)
    /// </summary>
    public const float R = 8.314462618f;
    
    /// <summary>
    ///     1 ATM in kPA.
    /// </summary>
    public const float OneAtmosphere = 101.325f;
    
    /// <summary>
    ///     -270.3ºC in K. CMB stands for Cosmic Microwave Background.
    /// </summary>
    public const float TCMB = 2.7f;
    
    public const float ZERO_CELCIUS = 273.15f;
    public const float ROOM_TEMPERATURE = ZERO_CELCIUS + 20f;
}
