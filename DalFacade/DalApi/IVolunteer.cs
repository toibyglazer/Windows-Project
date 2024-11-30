using DO;
namespace DalApi;
/// <summary>
/// Defines the contract for operations related to the 'Volunteer' entity.
/// Inherits CRUD operations from the ICrud interface, providing methods to 
/// create, read, update, and delete 'Volunteer' objects.
/// </summary>
public interface IVolunteer: ICrud<Volunteer>{}
