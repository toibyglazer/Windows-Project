/// <summary>
/// Implementation of the Volunteer data access layer. 
/// Provides methods for CRUD operations on Volunteer entities 
/// using XML serialization for data storage.
/// </summary>
using DalApi;
using DO;
using System;

namespace Dal;

/// <summary>
/// Handles operations related to volunteers, including creating, reading, updating, 
/// and deleting volunteers from the XML database.
/// </summary>
internal class VolunteerImplementation : IVolunteer
{
    private static readonly object lockObj = new object();
    public void Create(Volunteer item)
    {
        // Load the current list of volunteers from the XML file
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);

        if (volunteers.Any(v => v.Id == item.Id))
            throw new DalAlreadyExistsException($"Volunteer with ID={item.Id} already exists.");


        // Add the new volunteer to the list and save it back to the XML file
        volunteers.Add(item);
        XMLTools.SaveListToXMLSerializer(volunteers, Config.s_volunteers_xml);
    }


    /// <summary>
    /// Deletes a volunteer by ID from the database.
    /// Throws an exception if the volunteer does not exist.
    /// </summary>
    public void Delete(int id)
    {
       List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);

        // Attempt to remove the volunteer by ID
        if (volunteers.RemoveAll(v => v.Id == id) == 0)
            throw new DalDoesNotExistException($"volunteer with ID={id} does Not exist");

        // Save the updated list back to the XML file
        XMLTools.SaveListToXMLSerializer(volunteers, Config.s_volunteers_xml);
    }

    /// <summary>
    /// Deletes all volunteers from the database.
    /// </summary>
    public void DeleteAll()
    {
        lock (lockObj)
        {
            XMLTools.SaveListToXMLSerializer(new List<Volunteer>(), Config.s_volunteers_xml);
        }
           
    }

    /// <summary>
    /// Reads a specific volunteer by ID. Throws an exception if the volunteer does not exist.
    /// </summary>
    /// <param name="id">The ID of the volunteer to read.</param>
    /// <returns>The volunteer object if found; otherwise, null.</returns>
    public Volunteer? Read(int id)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        var matchingVolunteer = volunteers.FirstOrDefault(v => v.Id==id);
        if(matchingVolunteer == null )
            throw new DalDoesNotExistException($"volunteer with ID={id} does Not exist");
        return matchingVolunteer;
    }

    /// <summary>
    /// Reads a volunteer based on a filter function.
    /// Throws an exception if no matching volunteer is found.
    /// </summary>
    public Volunteer? Read(Func<Volunteer, bool> filter)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        // Find a matching volunteer using the filter
        var matchingVolunteer = volunteers.FirstOrDefault(v => filter(v));

        if (matchingVolunteer==null)
            throw new DalDoesNotExistException($"No matching volunteer found");
        return matchingVolunteer;
    }

    /// <summary>
    /// Reads all volunteers, optionally filtered by a condition.
    /// </summary>
    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        // Apply the filter if provided
        return filter != null
            ? from v in volunteers
              where filter(v)
              select v
             : from v in volunteers
               select v;
    }

    /// <summary>
    /// Updates an existing volunteer in the database. Throws an exception if the volunteer does not exist.
    /// </summary>
    /// <param name="item">The updated volunteer object.</param>
    public void Update(Volunteer item)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        if (volunteers.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"volunteer with ID={item.Id} does Not exist");
        volunteers.Add(item);
        XMLTools.SaveListToXMLSerializer(volunteers, Config.s_volunteers_xml);
    }
}
