/// <summary>
/// This file contains the implementation of the `CallImplementation` class, 
/// which provides data access layer functionality for managing `Call` entities 
/// using XML serialization for persistence.
/// </summary>
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq; // LINQ
using System.Runtime.CompilerServices; // MethodImpl

namespace Dal;

/// <summary>
/// The `CallImplementation` class implements the `ICall` interface.
/// It provides CRUD operations for `Call` entities stored in an XML file.
/// </summary>
internal class CallImplementation : ICall
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Call item)
    {
        int nextId = Config.NextCallId;
        List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        Call copy = item with { Id = nextId };

        calls.Add(copy);
        XMLTools.SaveListToXMLSerializer(calls, Config.s_calls_xml);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);

        if (calls.RemoveAll(v => v.Id == id) == 0)
            throw new DalDoesNotExistException($"Call with ID={id} does not exist");

        XMLTools.SaveListToXMLSerializer(calls, Config.s_calls_xml);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        // Exclusive rewrite of the list
        XMLTools.SaveListToXMLSerializer(new List<Call>(), Config.s_calls_xml);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Call? Read(int id)
    {
        List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        var match = calls.FirstOrDefault(v => v.Id == id);

        if (match == null)
            throw new DalDoesNotExistException($"Call with ID={id} does not exist");

        return match;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Call? Read(Func<Call, bool> filter)
    {
        List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        var match = calls.FirstOrDefault(v => filter(v));

        if (match == null)
            throw new DalDoesNotExistException("No matching Call found");

        return match;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null)
    {
        List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);

        return filter != null
            ? from call in calls where filter(call) select call
            : from call in calls select call;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Call item)
    {
        List<Call> calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);

        if (calls.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Call with ID={item.Id} does not exist");

        calls.Add(item);
        XMLTools.SaveListToXMLSerializer(calls, Config.s_calls_xml);
    }
}
