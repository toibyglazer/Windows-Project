namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
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
    public void Create(Assignment item)
    {
        int id = Config.NextAssignmentId;
        Assignment copy = item with { Id = id };
        DataSource.Assignments.Add(copy);
    }

    public void Delete(int id)
    {
        var item = DataSource.Assignments.FirstOrDefault(a => a.Id == id);

        if (item == null)
            throw new DalDoesNotExistException($"Assignment with ID {id} does not exist.");

        DataSource.Assignments.Remove(item);
    }

    public void DeleteAll()
    {
        //if (DataSource.Assignments.Count == 0)
        //   Console.WriteLine("Warning: The Assignments list is already empty.");
        DataSource.Assignments.Clear();
    }

    public Assignment? Read(int id)
    {
        if (DataSource.Assignments.FirstOrDefault(a => a.Id == id) == null)
        {
            throw new DalDoesNotExistException($"Assignment with ID {id} does not exist.");
        }

        return DataSource.Assignments.FirstOrDefault(a => a.Id == id);
    }

    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null)
    {
          return filter != null
            ? from item in DataSource.Assignments
              where filter(item)
              select item
            : from item in DataSource.Assignments
              select item;
    }
      
    

    public void Update(Assignment item)
    {
       
            var existing = DataSource.Assignments.FirstOrDefault(a => a.Id == item.Id);

            if (existing == null)
                throw new DalDoesNotExistException($"Assignment with ID {item.Id} does not exist.");

            // מחיקת האובייקט הקיים והוספת החדש
            DataSource.Assignments.Remove(existing);
            DataSource.Assignments.Add(item);
        
    }

    public Assignment? Read(Func<Assignment, bool> filter)
    {
        return DataSource.Assignments.FirstOrDefault(v => filter(v));
    }
}
