namespace BlApi;
/// <summary>
/// Factory class for obtaining BL instance.
/// </summary>
/// 
/// Explanations for the method:
/// - Get: Creates and returns an instance of the BL layer (Business Logic).
public static class Factory
{
    public static IBl Get() => new BlImplementation.Bl();

}   