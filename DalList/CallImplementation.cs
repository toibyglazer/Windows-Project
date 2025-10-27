namespace Dal;

using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq; // <= added (LINQ: FirstOrDefault, query syntax)
using System.Runtime.CompilerServices; // <= added

/// <summary>
/// Implementation of ICall interface, providing operations for managing Call objects in the system.
/// 
/// Available methods:
/// - Create(Call item): Adds a new Call object to the data source with a unique ID.
/// - Delete(int id): Deletes a Call object from the data source by its ID.
/// - DeleteAll(): Deletes all Call objects from the data source.
/// - Read(int id): Retrieves a specific Call object by its ID.
/// - ReadAll(): Retrieves all Call objects from the data source.
/// - Update(Call item): Updates an existing Call object in the data source with new information.
/// </summary>
internal class CallImplementation : ICall
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Call item)
    {
        int id = Config.NextCallId;
        Call copy = item with { Id = id };
        DataSource.Calls.Add(copy);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        var existingCall = DataSource.Calls.FirstOrDefault(c => c.Id == id);
        if (existingCall == null)
            throw new DalDoesNotExistException($"Call with ID {id} not found.");

        DataSource.Calls.Remove(existingCall);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        DataSource.Calls.Clear();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Call? Read(int id)
    {
        var match = DataSource.Calls.FirstOrDefault(a => a.Id == id);
        if (match == null)
            throw new DalDoesNotExistException($"Call with ID {id} does not exist.");

        return match;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null)
    {
        return filter != null
            ? from item in DataSource.Calls
              where filter(item)
              select item
            : from item in DataSource.Calls
              select item;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Call item)
    {
        var existingCall = DataSource.Calls.FirstOrDefault(c => c.Id == item.Id);
        if (existingCall == null)
            throw new DalDoesNotExistException($"Call with ID {item.Id} not found.");

        DataSource.Calls.Remove(existingCall);
        DataSource.Calls.Add(item);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Call? Read(Func<Call, bool> filter)
    {
        return DataSource.Calls.FirstOrDefault(v => filter(v));
    }
}
