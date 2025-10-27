/// <summary>
/// Implementation of the Volunteer data access layer. 
/// Provides methods for CRUD operations on Volunteer entities 
/// using XML serialization for data storage.
/// </summary>
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq; // LINQ
using System.Runtime.CompilerServices; // MethodImpl

namespace Dal;

/// <summary>
/// Handles operations related to volunteers, including creating, reading, updating, 
/// and deleting volunteers from the XML database.
/// </summary>
internal class VolunteerImplementation : IVolunteer
{
    // (Optional) This extra lock is not strictly needed when using [MethodImpl(Synchronized)].
    // You may remove lockObj and the lock usage to avoid double-serialization.
    private static readonly object lockObj = new object();

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Volunteer item)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);

        if (volunteers.Any(v => v.Id == item.Id))
            throw new DalAlreadyExistsException($"Volunteer with ID={item.Id} already exists.");

        volunteers.Add(item);
        XMLTools.SaveListToXMLSerializer(volunteers, Config.s_volunteers_xml);
    }

    /// <summary>
    /// Deletes a volunteer by ID from the database.
    /// Throws an exception if the volunteer does not exist.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);

        if (volunteers.RemoveAll(v => v.Id == id) == 0)
            throw new DalDoesNotExistException($"Volunteer with ID={id} does not exist.");

        XMLTools.SaveListToXMLSerializer(volunteers, Config.s_volunteers_xml);
    }

    /// <summary>
    /// Deletes all volunteers from the database.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        // אפשר להסיר את ה-lock הזה אם נשארים רק עם MethodImpl(Synchronized)
        lock (lockObj)
        {
            XMLTools.SaveListToXMLSerializer(new List<Volunteer>(), Config.s_volunteers_xml);
        }
    }

    /// <summary>
    /// Reads a specific volunteer by ID. Throws an exception if the volunteer does not exist.
    /// </summary>
    /// <param name="id">The ID of the volunteer to read.</param>
    /// <returns>The volunteer object if found; otherwise, throws.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Volunteer? Read(int id)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        var matchingVolunteer = volunteers.FirstOrDefault(v => v.Id == id);

        if (matchingVolunteer == null)
            throw new DalDoesNotExistException($"Volunteer with ID={id} does not exist.");

        return matchingVolunteer;
    }

    /// <summary>
    /// Reads a volunteer based on a filter function.
    /// Throws an exception if no matching volunteer is found.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Volunteer? Read(Func<Volunteer, bool> filter)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        var matchingVolunteer = volunteers.FirstOrDefault(v => filter(v));

        if (matchingVolunteer == null)
            throw new DalDoesNotExistException("No matching volunteer found.");

        return matchingVolunteer;
    }

    /// <summary>
    /// Reads all volunteers, optionally filtered by a condition.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);

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
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Volunteer item)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);

        if (volunteers.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Volunteer with ID={item.Id} does not exist.");

        volunteers.Add(item);
        XMLTools.SaveListToXMLSerializer(volunteers, Config.s_volunteers_xml);
    }
}
