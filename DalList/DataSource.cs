namespace Dal;


/// <summary>
/// Static class serving as an in-memory data source for storing calls, assignments, and volunteers.
/// Provides static lists to manage these entities.
/// </summary>
internal static class DataSource
{
    internal static List<DO.Call> Calls { get; } = new();
    internal static List<DO.Assignment> Assignments { get; } = new();
    internal static List<DO.Volunteer> Volunteers { get; } = new();
}
