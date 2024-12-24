using BlApi;
using BlImplementation;
namespace BlImplementation;
internal class Bl : IBl
{ 
    public IVolunteer Volunteer { get; } = new VolunteerImplementation();
    public ICall Call { get; } = new CallImplementation();
    public IAdmin Admin { get; } = new AdminImplementation();
}
