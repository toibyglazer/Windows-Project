namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class AssignmentImplementation : IAssignment
{
    public void Create(Assignment item)
    {
        int id = Config.NextAssignmentId;
        Assignment copy = item with { Id=id };
        DataSource.Assignments.Add(copy);
    }

    public void Delete(int id)
    {
        var item = DataSource.Assignments.FirstOrDefault(a => a.Id == id);

        if (item == null)
            throw new KeyNotFoundException($"Assignment with ID {id} does not exist.");

        DataSource.Assignments.Remove(item);
    }

    public void DeleteAll()
    {
        DataSource.Assignments.Clear();
    }

    public Assignment? Read(int id)
    {
        if (DataSource.Assignments.FirstOrDefault(a => a.Id == id) == null)
        {
            throw new KeyNotFoundException($"Assignment with ID {id} does not exist.");
        }

        return DataSource.Assignments.FirstOrDefault(a => a.Id == id);
    }

    public List<Assignment> ReadAll()
    {
        return new List<Assignment>(DataSource.Assignments);
    }

    public void Update(Assignment item)
    {
       
            var existing = DataSource.Assignments.FirstOrDefault(a => a.Id == item.Id);

            if (existing == null)
                throw new KeyNotFoundException($"Assignment with ID {item.Id} does not exist.");

            // מחיקת האובייקט הקיים והוספת החדש
            DataSource.Assignments.Remove(existing);
            DataSource.Assignments.Add(item);
        
    }
}
