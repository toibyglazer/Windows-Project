namespace Dal;

using DO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

static class XMLTools
{
    const string s_xmlDir = @"..\xml\";
    static XMLTools()
    {
        if (!Directory.Exists(s_xmlDir))
            Directory.CreateDirectory(s_xmlDir);
    }

    #region SaveLoadWithXMLSerializer
    //public static void SaveListToXMLSerializer<T>(List<T> list, string xmlFileName) where T : class
    //{
    //    string xmlFilePath = s_xmlDir + xmlFileName;

    //    try
    //    {
    //        using FileStream file = new(xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
    //        new XmlSerializer(typeof(List<T>)).Serialize(file, list);
    //    }
    //    catch (DalXMLFileLoadCreateException ex)
    //    {
    //        throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
    //    }
    //}
    public static void SaveListToXMLSerializer<T>(List<T> list, string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            // Use FileShare.None to ensure exclusive access to the file
            using FileStream file = new(xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            new XmlSerializer(typeof(List<T>)).Serialize(file, list);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {xmlFilePath}, {ex.Message}");
        }
    }

    //public static List<T> LoadListFromXMLSerializer<T>(string xmlFileName) where T : class
    //{
    //    string xmlFilePath = s_xmlDir + xmlFileName;

    //    try
    //    {
    //        if (!File.Exists(xmlFilePath)) return new();
    //        using FileStream file = new(xmlFilePath, FileMode.Open);
    //        XmlSerializer x = new(typeof(List<T>));
    //        return x.Deserialize(file) as List<T> ?? new();
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new DalXMLFileLoadCreateException($"fail to load xml file: {xmlFilePath}, {ex.Message}");
    //    }
    //}
    public static List<T> LoadListFromXMLSerializer<T>(string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            if (!File.Exists(xmlFilePath)) return new();


            using FileStream file = new(xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            XmlSerializer x = new(typeof(List<T>));
            return x.Deserialize(file) as List<T> ?? new();
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {xmlFilePath}, {ex.Message}");
        }
    }
    #endregion

    #region SaveLoadWithXElement
    public static void SaveListToXMLElement(XElement rootElem, string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            rootElem.Save(xmlFilePath);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    public static XElement LoadListFromXMLElement(string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            if (File.Exists(xmlFilePath))
                return XElement.Load(xmlFilePath);
            XElement rootElem = new(xmlFileName);
            rootElem.Save(xmlFilePath);
            return rootElem;
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    #endregion

    #region XmlConfig
    public static int GetAndIncreaseConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int nextId = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        root.Element(elemName)?.SetValue((nextId + 1).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
        return nextId;
    }
    public static int GetConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int num = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }
    public static DateTime GetConfigDateVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        DateTime dt = root.ToDateTimeNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }
    public static void SetConfigIntVal(string xmlFileName, string elemName, int elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    public static void SetConfigDateVal(string xmlFileName, string elemName, DateTime elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Added this method to handle <see cref="TimeSpan"/> values as they were required in the program,
    /// but were not included at this provided helper file.
    /// </summary>
    
    /// <summary>
    /// Retrieves a <see cref="TimeSpan"/> value from the specified XML element.
    /// Throws a <see cref="FormatException"/> if the value cannot be parsed.
    /// </summary>
    /// <param name="xmlFileName">The XML file to load the element from.</param>
    /// <param name="elemName">The name of the element containing the TimeSpan value.</param>
    /// <returns>A <see cref="TimeSpan"/> value.</returns>
    public static TimeSpan GetConfigRiskRange(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        TimeSpan dt = root.ToTimeSpanNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }

    /// <summary>
    /// Sets an integer value for the specified element in the given XML file.
    /// </summary>
    /// <param name="xmlFileName">The XML file to modify.</param>
    /// <param name="elemName">The name of the element to set the value for.</param>
    /// <param name="elemVal">The integer value to set.</param>
    public static void SetConfigIntTimeSpan(string xmlFileName, string elemName, int elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Sets a <see cref="TimeSpan"/> value for the specified element in the given XML file.
    /// </summary>
    /// <param name="xmlFileName">The XML file to modify.</param>
    /// <param name="elemName">The name of the element to set the value for.</param>
    /// <param name="elemVal">The <see cref="TimeSpan"/> value to set.</param>
    public static void SetConfigTimeSpan(string xmlFileName, string elemName, TimeSpan elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    #endregion

    #region ExtensionFuctions
    public static T? ToEnumNullable<T>(this XElement element, string name) where T : struct, Enum =>
        Enum.TryParse<T>((string?)element.Element(name), out var result) ? (T?)result : null;
    public static DateTime? ToDateTimeNullable(this XElement element, string name) =>
        DateTime.TryParse((string?)element.Element(name), out var result) ? (DateTime?)result : null;
    public static double? ToDoubleNullable(this XElement element, string name) =>
        double.TryParse((string?)element.Element(name), out var result) ? (double?)result : null;
    public static int? ToIntNullable(this XElement element, string name) =>
        int.TryParse((string?)element.Element(name), out var result) ? (int?)result : null;

    /// <summary>
    /// Tries to parse the value of a child element as a nullable <see cref="TimeSpan"/>.
    /// Returns null if parsing fails.
    /// </summary>
    /// <param name="element">The parent <see cref="XElement"/>.</param>
    /// <param name="name">The name of the child element to parse.</param>
    /// <returns>A nullable <see cref="TimeSpan"/> or null.</returns>
    public static TimeSpan? ToTimeSpanNullable(this XElement element, string name) =>
       TimeSpan.TryParse((string?)element.Element(name), out var result) ? (TimeSpan?)result : null;
    #endregion


}