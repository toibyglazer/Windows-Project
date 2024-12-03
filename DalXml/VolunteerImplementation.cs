using DalApi;
using DO;
using System;

namespace Dal;

internal class VolunteerImplementation : IVolunteer
{
    public void Create(Volunteer item)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        if (volunteers.Any(v => v.Id == item.Id))
                throw new DalDoesNotExistException($"volunteer with ID={item.Id} does Not exist");
        volunteers.Add(item);
        XMLTools.SaveListToXMLSerializer(volunteers, Config.s_volunteers_xml);
    }

    public void Delete(int id)
    {
       List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
       if(volunteers.RemoveAll(v => v.Id == id) == 0)
            throw new DalDoesNotExistException($"volunteer with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(volunteers, Config.s_volunteers_xml);
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Volunteer>(), Config.s_volunteers_xml);
    }

    public Volunteer? Read(int id)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        var matchingVolunteer = volunteers.FirstOrDefault(v => v.Id==id);
        if(matchingVolunteer == null)
            throw new DalDoesNotExistException($"volunteer with ID={id} does Not exist");
        return matchingVolunteer;
    }

    public Volunteer? Read(Func<Volunteer, bool> filter)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        var matchingVolunteer = volunteers.FirstOrDefault(v => filter(v));

        if (matchingVolunteer==null)
            throw new DalDoesNotExistException($"No matching volunteer found");
        return matchingVolunteer;
    }

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

    public void Update(Volunteer item)
    {
        List<Volunteer> volunteers = XMLTools.LoadListFromXMLSerializer<Volunteer>(Config.s_volunteers_xml);
        if (volunteers.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"volunteer with ID={item.Id} does Not exist");
        volunteers.Add(item);
        XMLTools.SaveListToXMLSerializer(volunteers, Config.s_volunteers_xml);
    }
}
