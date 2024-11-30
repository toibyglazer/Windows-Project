namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Xml;
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
    public void Create(Call item)
    {

        int id = Config.NextCallId;
        Call copy = item with { Id = id };
        DataSource.Calls.Add(copy);
    }

    public void Delete(int id)
    { 
            var existingCall = DataSource.Calls.FirstOrDefault(c => c.Id == id);

            if (existingCall == null)
            {
                throw new DalDoesNotExistException($"Call with ID {id} not found.");
            }

        DataSource.Calls.Remove(existingCall);
    }
    public void DeleteAll()
    {
        if (DataSource.Calls.Count == 0)
           Console.WriteLine("Warning: The Calls list is already empty.");
    
        DataSource.Calls.Clear();
    }
    public Call? Read(int id)
    {
        if (DataSource.Calls.FirstOrDefault(a => a.Id == id) == null)
        {
            throw new DalDoesNotExistException($"Call with ID {id} does not exist.");
        }
        return DataSource.Calls.FirstOrDefault(c => c.Id == id);
    }
    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null)
    {
       return filter != null
        ? from item in DataSource.Calls
          where filter(item)
          select item
        : from item in DataSource.Calls
          select item;
    }
    public void Update(Call item)
    {
        var existingCall = DataSource.Calls.FirstOrDefault(c => c.Id == item.Id);

        if (existingCall == null)
        {
            throw new DalDoesNotExistException($"Call with ID {item.Id} not found.");
        }

      
        DataSource.Calls.Remove(existingCall);
        DataSource.Calls.Add(item);
    }
    public Call? Read(Func<Call, bool> filter)
    {
        return DataSource.Calls.FirstOrDefault(v => filter(v));
    }

}
