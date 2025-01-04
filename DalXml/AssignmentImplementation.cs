using DalApi;
using DO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dal;

/// <summary>
/// This file contains the implementation of the IAssignment interface, 
/// which manages CRUD operations for Assignment objects stored in an XML file.
/// The implementation includes methods to create, read, update, and delete Assignment entities,
/// as well as helper methods for working with XElement representations.
/// </summary>
internal class AssignmentImplementation : IAssignment
{
    private static readonly object lockObj = new object();
    /// <summary>
    /// Implements the IAssignment interface to handle data operations for Assignment objects.
    /// The data is stored and manipulated in an XML file specified in the configuration.
    /// </summary>
    static Assignment getAssignment(XElement s)
    {
        /// <summary>
        /// Converts an XElement into an Assignment object.
        /// </summary>
        /// <param name="s">The XElement representing the Assignment.</param>
        /// <returns>The corresponding Assignment object.</returns>
        /// <exception cref="FormatException">Thrown when a required field cannot be converted.</exception>
        return new DO.Assignment()
        {
            Id = s.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            CallId = s.ToIntNullable("CallId") ?? throw new FormatException("can't convert CallId"),
            VolunteerId = s.ToIntNullable("VolunteerId") ?? throw new FormatException("can't convert VolunteerId"),
            EntryTime = s.ToDateTimeNullable("EntryTime") ?? throw new FormatException("can't convert EntryTime"),
            CompletionTime = s?.ToDateTimeNullable("CompletionTime") ?? null,
            CompletionType = s?.ToEnumNullable<AssignmentCompletionType>("CompletionType") ?? null
        };
    }
    /// <summary>
    /// Creates an XElement representing an Assignment object.
    /// </summary>
    /// <param name="item">The Assignment object to be converted.</param>
    /// <returns>The XElement representing the Assignment.</returns>
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

    /// <summary>
    /// Adds a new Assignment to the XML file.
    /// </summary>
    /// <param name="item">The Assignment object to be added.</param>
    public void Create(Assignment item)
    {
        XElement assignmentsRootElem = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml);

        int nextId = Config.NextAssignmentId;
        Assignment copy = item with { Id = nextId };
        XElement newItem = createAssignmentElement(copy);

        assignmentsRootElem.Add(newItem);
        XMLTools.SaveListToXMLElement(assignmentsRootElem, Config.s_assignments_xml);
    }

    /// <summary>
    /// Deletes an Assignment from the XML file by its ID.
    /// </summary>
    /// <param name="id">The ID of the Assignment to be deleted.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the Assignment does not exist.</exception>
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

    /// <summary>
    /// Deletes all Assignments from the XML file.
    /// </summary>
    public void DeleteAll()
    {
       
        lock (lockObj)
        {
            XElement assignmentsRootElem = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml);
            assignmentsRootElem.RemoveNodes();
            XMLTools.SaveListToXMLElement(assignmentsRootElem, Config.s_assignments_xml);

        }

    }

    /// <summary>
    /// Retrieves an Assignment by its ID.
    /// </summary>
    /// <param name="id">The ID of the Assignment to retrieve.</param>
    /// <returns>The Assignment object if found; otherwise, null.</returns>
    public Assignment? Read(int id)
    { 
        XElement? assignmentElem =
        XMLTools.LoadListFromXMLElement(Config.s_assignments_xml).Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        return assignmentElem is null ? null : getAssignment(assignmentElem);
    }

    /// <summary>
    /// Retrieves an Assignment that matches a given filter.
    /// </summary>
    /// <param name="filter">The filter function to apply.</param>
    /// <returns>The first matching Assignment if found; otherwise, null.</returns>
    public Assignment? Read(Func<Assignment, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_assignments_xml)
            .Elements()
            .Select(s => getAssignment(s))
            .FirstOrDefault(filter);
    }


    /// <summary>
    /// Retrieves all Assignments, optionally filtered by a given condition.
    /// </summary>
    /// <param name="filter">An optional filter function.</param>
    /// <returns>An IEnumerable of Assignment objects.</returns>
    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null)
    {
        IEnumerable<Assignment> assignments = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml)
               .Elements()
               .Select(s => getAssignment(s));


        return filter != null ? assignments.Where(filter) : assignments;
    }

    /// <summary>
    /// Updates an existing Assignment in the XML file.
    /// </summary>
    /// <param name="item">The Assignment object with updated data.</param>
    /// <exception cref="DalDoesNotExistException">Thrown when the Assignment does not exist.</exception>
    public void Update(Assignment item)
    {
        XElement assignmentsRootElem = XMLTools.LoadListFromXMLElement(Config.s_assignments_xml);

        (assignmentsRootElem.Elements().FirstOrDefault(st => (int?)st.Element("Id") == item.Id)
        ?? throw new DO.DalDoesNotExistException($"Student with ID={item.Id} does Not exist"))
                .Remove();

        assignmentsRootElem.Add(new XElement(createAssignmentElement(item)));

        XMLTools.SaveListToXMLElement(assignmentsRootElem, Config.s_assignments_xml);
    }
}
