namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Xml;

public class CallImplementation : ICall
{
    public void Create(Call item)
    {

        int id = Config.NextCallId;
        Call copy = item with { Id =id  };
        DataSource.Calls.Add(copy);
    }

    public void Delete(int id)
    {
        
        
            var existingCall = DataSource.Calls.FirstOrDefault(c => c.Id == id);

            if (existingCall == null)
            {
                throw new Exception($"Call with ID {id} not found.");
            }

            DataSource.Calls.Remove(existingCall);
        


    }

    public void DeleteAll()
    {
        DataSource.Calls.Clear();
    }

    public Call? Read(int id)
    {
        if (DataSource.Calls.FirstOrDefault(a => a.Id == id) == null)
        {
            throw new Exception($"Call with ID {id} does not exist.");
        }
        return DataSource.Calls.FirstOrDefault(c => c.Id == id);
    }

    public List<Call> ReadAll()
    {
        return new List<Call>(DataSource.Calls);
}

public void Update(Call item)
    {
       
            var existingCall = DataSource.Calls.FirstOrDefault(c => c.Id == item.Id);

            if (existingCall == null)
            {
                throw new Exception($"Call with ID {item.Id} not found.");
            }

            // מחיקת הקריאה הישנה והוספת החדשה
            DataSource.Calls.Remove(existingCall);
            DataSource.Calls.Add(item);
     }
}
