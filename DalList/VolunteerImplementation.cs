namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class VolunteerImplementation : IVolunteer
{
    public void Create(Volunteer item)
    {
        if (DataSource.Volunteers.Any(v => v.Id == item.Id) )
        {
            throw new Exception($"Volunteer with ID {item.Id} already exists.");
        }

        DataSource.Volunteers.Add( item );
    }

    public void Delete(int id)
    {
        var volunteer = DataSource.Volunteers.FirstOrDefault(v => v.Id == id);
        if (volunteer == null)
        {
            throw new Exception($"Volunteer with ID {id} does not exist.");
        }
        DataSource.Volunteers.Remove( volunteer );
    }

    public void DeleteAll()
    {
        DataSource.Volunteers.Clear();
    }

    public Volunteer? Read(int id)
    {
       
        
        if (DataSource.Volunteers.FirstOrDefault(a => a.Id == id) == null)
        {
            throw new Exception($"Call with ID {id} does not exist.");
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


    public List<Volunteer> ReadAll()
    { 
       return new List<Volunteer> (DataSource.Volunteers);    
    }

    public void Update(Volunteer item)
    {
        var volunteerIndex = DataSource.Volunteers.FindIndex(v => v.Id == item.Id);
        if (volunteerIndex == -1)
        {
            throw new Exception($"Volunteer with ID {item.Id} does not exist.");
        }

        DataSource.Volunteers[volunteerIndex] = item; 
    }
}

