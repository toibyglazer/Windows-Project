namespace Dal;

using DalApi;
using DO;
using System.Collections.Generic;
using System.Linq; // נדרש עבור LINQ
using System.Runtime.CompilerServices; // נדרש לאנוטציה

/// <summary>
/// Implementation of IAssignment interface, providing operations for managing Assignment objects in the system.
/// 
/// Available methods:
/// - Create(Assignment item): Adds a new Assignment object to the data source with a unique ID.
/// - Delete(int id): Deletes an Assignment object from the data source by its ID.
/// - DeleteAll(): Deletes all Assignment objects from the data source.
/// - Read(int id): Retrieves a specific Assignment object by its ID.
/// - ReadAll(): Retrieves all Assignment objects from the data source.
/// - Update(Assignment item): Updates an existing Assignment object in the data source with new information.
/// </summary>
internal class AssignmentImplementation : IAssignment
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Assignment item)
    {
        int id = Config.NextAssignmentId;
        Assignment copy = item with { Id = id };
        DataSource.Assignments.Add(copy);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        var item = DataSource.Assignments.FirstOrDefault(a => a.Id == id);

        if (item == null)
            throw new DalDoesNotExistException($"Assignment with ID {id} does not exist.");

        DataSource.Assignments.Remove(item);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        DataSource.Assignments.Clear();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Assignment? Read(int id)
    {
        var match = DataSource.Assignments.FirstOrDefault(a => a.Id == id);
        if (match == null)
            throw new DalDoesNotExistException($"Assignment with ID {id} does not exist.");

        return match;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null)
    {
        return filter != null
            ? from item in DataSource.Assignments
              where filter(item)
              select item
            : from item in DataSource.Assignments
              select item;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Assignment item)
    {
        var existing = DataSource.Assignments.FirstOrDefault(a => a.Id == item.Id);

        if (existing == null)
            throw new DalDoesNotExistException($"Assignment with ID {item.Id} does not exist.");

        DataSource.Assignments.Remove(existing);
        DataSource.Assignments.Add(item);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Assignment? Read(Func<Assignment, bool> filter)
    {
        return DataSource.Assignments.FirstOrDefault(v => filter(v));
    }
}
