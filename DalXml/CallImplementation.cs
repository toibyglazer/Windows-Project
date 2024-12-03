/// <summary>
/// This file contains the implementation of the `CallImplementation` class, 
/// which provides data access layer functionality for managing `Call` entities 
/// using XML serialization for persistence.
/// </summary>
using DalApi;
using DO;

namespace Dal;

/// <summary>
/// The `CallImplementation` class implements the `ICall` interface.
/// It provides CRUD operations for `Call` entities stored in an XML file.
/// </summary>
internal class CallImplementation : ICall
{
    /// <summary>
    /// Adds a new `Call` entity to the XML storage.
    /// Generates a unique ID for the new `Call` and saves it.
    /// </summary>
    /// <param name="item">The `Call` entity to create.</param>
    public void Create(Call item)
    {
        int nextId = Config.NextCallId;
        List <Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        Call copy = item with { Id = nextId };

        Calls.Add(copy);
        XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml);
    }

    /// <summary>
    /// Deletes a `Call` entity by its ID from the XML storage.
    /// Throws an exception if the `Call` does not exist.
    /// </summary>
    /// <param name="id">The ID of the `Call` to delete.</param>
    public void Delete(int id)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        if (Calls.RemoveAll(v => v.Id == id) == 0)
            throw new DalDoesNotExistException($"Call with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml);
    }

    /// <summary>
    /// Deletes all `Call` entities from the XML storage.
    /// </summary>
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Call>(), Config.s_calls_xml);
    }

    /// <summary>
    /// Reads a `Call` entity by its ID from the XML storage.
    /// Throws an exception if the `Call` does not exist.
    /// </summary>
    /// <param name="id">The ID of the `Call` to read.</param>
    /// <returns>The `Call` entity with the specified ID.</returns>
    public Call? Read(int id)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        var matchingVolunteer = Calls.FirstOrDefault(v => v.Id == id);

        if (matchingVolunteer == null)
            throw new DalDoesNotExistException($"Call with ID={id} does Not exist");
        return matchingVolunteer;
    }

    /// <summary>
    /// Reads the first `Call` entity matching a given filter from the XML storage.
    /// Throws an exception if no matching `Call` is found.
    /// </summary>
    /// <param name="filter">The filter condition to apply.</param>
    /// <returns>The first matching `Call` entity.</returns>
    public Call? Read(Func<Call, bool> filter)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        var matchingVolunteer = Calls.FirstOrDefault(v => filter(v));

        if (matchingVolunteer == null)
            throw new DalDoesNotExistException($"No matching Call found");
        return matchingVolunteer;
    }

    /// <summary>
    /// Reads all `Call` entities from the XML storage that match a given filter.
    /// If no filter is provided, all entities are returned.
    /// </summary>
    /// <param name="filter">An optional filter condition to apply.</param>
    /// <returns>An enumerable of matching `Call` entities.</returns>
    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        return filter != null
            ? from call in Calls
              where filter(call)
              select call
             : from call in Calls
               select call;
    }

    /// <summary>
    /// Updates an existing `Call` entity in the XML storage.
    /// Replaces the old entity with the updated one. Throws an exception if the entity does not exist.
    /// </summary>
    /// <param name="item">The updated `Call` entity.</param>
    public void Update(Call item)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        if (Calls.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Call with ID={item.Id} does Not exist");
        Calls.Add(item);
        XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml);
    }
}

