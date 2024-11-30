namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// Implementation of IVolunteer interface, providing operations for managing Volunteer objects in the system.
/// 
/// Available methods:
/// - Create(Volunteer item): Adds a new Volunteer to the data source if the ID does not already exist. Throws an exception if a volunteer with the same ID exists.
/// - Delete(int id): Removes a Volunteer from the data source by their ID. Throws an exception if the volunteer does not exist.
/// - DeleteAll(): Clears all Volunteers from the data source.
/// - Read(int id): Retrieves a Volunteer by their ID from the data source. Throws an exception if the volunteer does not exist. Returns null if the volunteer is not found.
/// - ReadAll(): Retrieves a list of all Volunteers from the data source.
/// - Update(Volunteer item): Updates an existing Volunteer in the data source. Throws an exception if the volunteer does not exist.
/// </summary>
internal class VolunteerImplementation : IVolunteer
{
    public void Create(Volunteer item)
    {
        if (DataSource.Volunteers.Any(v => v.Id == item.Id))
        {
            throw new DalAlreadyExistsException($"Volunteer with ID {item.Id} already exists.");
        }

        DataSource.Volunteers.Add(item);
    }
    public void Delete(int id)
    {
        var volunteer = DataSource.Volunteers.FirstOrDefault(v => v.Id == id);
        if (volunteer == null)
        {
            throw new DalDoesNotExistException($"Volunteer with ID {id} does not exist.");
        }
        DataSource.Volunteers.Remove(volunteer);
    }
    public void DeleteAll()
    {
        if (DataSource.Volunteers.Count == 0)
           Console.WriteLine("Warning: The Volunteers list is already empty.");
        DataSource.Volunteers.Clear();
    }
    public Volunteer? Read(int id)
    {


        if (DataSource.Volunteers.FirstOrDefault(a => a.Id == id) == null)
        {
            throw new DalDoesNotExistException($"Call with ID {id} does not exist.");
        }

        foreach (var v in DataSource.Volunteers)
        {
            if (v.Id.Equals(id))
            {
                return v;
            }
        }

        return null;
    }
    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null)
    {
        return filter != null
        ? from item in DataSource.Volunteers
          where filter(item)
          select item
        : from item in DataSource.Volunteers
          select item;
    }
    public void Update(Volunteer item)
    {
        var volunteerIndex = DataSource.Volunteers.FindIndex(v => v.Id == item.Id);
        if (volunteerIndex == -1)
        {
            throw new DalDoesNotExistException($"Volunteer with ID {item.Id} does not exist.");
        }

        DataSource.Volunteers[volunteerIndex] = item;
    }
    public Volunteer? Read(Func<Volunteer, bool> filter)
    {
        return DataSource.Volunteers.FirstOrDefault(v => filter(v));
    }
}

