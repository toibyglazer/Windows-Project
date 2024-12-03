namespace DO;


/// <summary>
/// An exception will be thrown when we want to add an element that already exists in the list.
/// </summary>
[Serializable]
public class DalAlreadyExistsException : Exception
{
    public DalAlreadyExistsException(string? message) : base(message) { }
}

/// <summary>
/// An exception will occur when we want to update/delete/read a non-existent element in the list.
/// </summary>
[Serializable]
public class DalDoesNotExistException : Exception
{
    public DalDoesNotExistException(string? message) : base(message) { }

}

/// <summary>
/// I haven't used this exception yet, hopefully later.
/// </summary>
[Serializable]
public class DalDalDeletionImpossible : Exception
{
    public DalDalDeletionImpossible(string? message) : base(message) { }

}

public class DalXMLFileLoadCreateException : Exception
{
    public DalXMLFileLoadCreateException(string? message) : base(message) { }

}







