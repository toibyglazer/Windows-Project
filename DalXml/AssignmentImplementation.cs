using DalApi;
using DO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dal;

internal class AssignmentImplementation : IAssignment
{

    static Assignment getAssignment(XElement s)
    {
        return new DO.Assignment()
        {
            Id = s.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            CallId = s.ToIntNullable("CallId") ?? throw new FormatException("can't convert CallId"),
            VolunteerId = s.ToIntNullable("VolunteerId") ?? throw new FormatException("can't convert VolunteerId"),
            EntryTime = s.ToDateTimeNullable("EntryTime") ?? throw new FormatException("can't convert EntryTime"),
            CompletionTime = s?.ToDateTimeNullable("CompletionTime") ?? null,
            CompletionType= s?.ToEnumNullable<AssignmentCompletionType>("CompletionType") ?? null
        };
    }
    private static XElement createAssignmentElement(Assignment item)
    {
        return new XElement("Assignment",
            new XElement("Id", item.Id),
            new XElement("CallId", item.CallId),
            new XElement("VolunteerId", item.VolunteerId),
            new XElement("EntryTime", item.EntryTime),
            new XElement("CompletionTime", item.CompletionTime),
            new XElement("CompletionType", item.CompletionType)
        );
    }

    public void Create(Assignment item)
    {
        XElement assignmentsRootElem = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml);
        
        int nextId = Config.NextAssignmentId;
        Assignment copy = item with { Id = nextId };
        XElement newItem = createAssignmentElement(copy);

        assignmentsRootElem.Add(newItem);
        XMLTools.SaveListToXMLElement(assignmentsRootElem, Config.s_assignments_xml);
    }

    public void Delete(int id)
    {
        XElement assignmentsRootElem = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml);
        XElement? assignmentToRemove =
            assignmentsRootElem.Elements()
            .FirstOrDefault(st => (int?)st.Element("Id") == id);

        if (assignmentToRemove == null)
            throw new DalDoesNotExistException($"Assignment with ID={id} does not exist.");

        assignmentToRemove.Remove();
        XMLTools.SaveListToXMLElement(assignmentsRootElem, Config.s_assignments_xml);
    }

    public void DeleteAll()
    {
        XElement assignmentsRootElem = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml);
        assignmentsRootElem.RemoveNodes();
        XMLTools.SaveListToXMLElement(assignmentsRootElem, Config.s_assignments_xml);
    }

    public Assignment? Read(int id)
    { 
        XElement? assignmentElem =
        XMLTools.LoadListFromXMLElement(Config.s_assignments_xml).Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        return assignmentElem is null ? null : getAssignment(assignmentElem);
    }

    public Assignment? Read(Func<Assignment, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_assignments_xml)
            .Elements()
            .Select(s => getAssignment(s))
            .FirstOrDefault(filter);
    }

    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null)
    {
        IEnumerable<Assignment> assignments = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml)
               .Elements()
               .Select(s => getAssignment(s));


        return filter != null ? assignments.Where(filter) : assignments;
    }

    public void Update(Assignment item)
    {
        XElement assignmentsRootElem = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml);

        (assignmentsRootElem.Elements().FirstOrDefault(st => (int?)st.Element("Id") == item.Id)
        ?? throw new DO.DalDoesNotExistException($"Student with ID={item.Id} does Not exist"))
                .Remove();

        assignmentsRootElem.Add(new XElement(/*"Assignment",*/createAssignmentElement(item)));

        XMLTools.SaveListToXMLElement(assignmentsRootElem, Config.s_assignments_xml);
    }
}
