using BlApi;
using BlImplementation;
namespace BlImplementation;
/// <summary>
/// Main implementation of the BL (Business Logic) layer.
/// This class acts as a central point for accessing the three core components:
/// - Volunteer operations (IVolunteer)
/// - Call operations (ICall)
/// - Administrative functionalities (IAdmin)
/// Each component is initialized and ready to handle its respective operations.
/// </summary>
internal class Bl : IBl
{ 
    public IVolunteer Volunteer { get; } = new VolunteerImplementation();
    public ICall Call { get; } = new CallImplementation();
    public IAdmin Admin { get; } = new AdminImplementation();
}
