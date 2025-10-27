using DalApi;
using DO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Runtime.CompilerServices; // sync attribute

namespace Dal;

/// <summary>
/// IAssignment XML implementation: CRUD over Assignments stored in an XML file.
/// </summary>
internal class AssignmentImplementation : IAssignment
{
    // === Helpers (private) ===
    static Assignment getAssignment(XElement s) =>
        new DO.Assignment()
        {
            Id = s.ToIntNullable("Id") ?? throw new FormatException("can't convert Id"),
            CallId = s.ToIntNullable("CallId") ?? throw new FormatException("can't convert CallId"),
            VolunteerId = s.ToIntNullable("VolunteerId") ?? throw new FormatException("can't convert VolunteerId"),
            EntryTime = s.ToDateTimeNullable("EntryTime") ?? throw new FormatException("can't convert EntryTime"),
            CompletionTime = s.ToDateTimeNullable("CompletionTime"),
            CompletionType = s.ToEnumNullable<AssignmentCompletionType>("CompletionType")
        };

    private static XElement createAssignmentElement(Assignment item) =>
        new XElement("Assignment",
            new XElement("Id", item.Id),
            new XElement("CallId", item.CallId),
            new XElement("VolunteerId", item.VolunteerId),
            new XElement("EntryTime", item.EntryTime),
            new XElement("CompletionTime", item.CompletionTime),
            new XElement("CompletionType", item.CompletionType)
        );

    // === Public CRUD ===

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Assignment item)
    {
        XElement root = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml);

        int nextId = Config.NextAssignmentId;
        Assignment copy = item with { Id = nextId };
        XElement newItem = createAssignmentElement(copy);

        root.Add(newItem);
        XMLTools.SaveListToXMLElement(root, Config.s_assignments_xml);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        XElement root = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml);
        XElement? toRemove = root.Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);

        if (toRemove is null)
            throw new DalDoesNotExistException($"Assignment with ID={id} does not exist.");

        toRemove.Remove();
        XMLTools.SaveListToXMLElement(root, Config.s_assignments_xml);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        XElement root = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml);
        root.RemoveNodes();
        XMLTools.SaveListToXMLElement(root, Config.s_assignments_xml);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Assignment? Read(int id)
    {
        XElement root = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml);
        XElement? elem = root.Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);

        if (elem is null)
            throw new DalDoesNotExistException($"Assignment with ID={id} does not exist.");

        return getAssignment(elem);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Assignment? Read(Func<Assignment, bool> filter)
    {
        var match = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml)
            .Elements()
            .Select(getAssignment)
            .FirstOrDefault(filter);

        if (match is null)
            throw new DalDoesNotExistException("No matching Assignment found.");

        return match;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null)
    {
        IEnumerable<Assignment> all = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml)
            .Elements()
            .Select(getAssignment);

        return filter is not null ? all.Where(filter) : all;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Assignment item)
    {
        XElement root = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml);

        (root.Elements().FirstOrDefault(st => (int?)st.Element("Id") == item.Id)
            ?? throw new DalDoesNotExistException($"Assignment with ID={item.Id} does not exist."))
            .Remove();

        root.Add(createAssignmentElement(item));
        XMLTools.SaveListToXMLElement(root, Config.s_assignments_xml);
    }
}
