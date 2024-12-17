namespace BO;

[Serializable]
public class BlDoesNotExistException : Exception
{
    public BlDoesNotExistException(string? message) : base(message) { }
    public BlDoesNotExistException(string message, Exception innerException)
                : base(message, innerException) { }
}
public class BlInvalidPasswordException : Exception
{
    public BlInvalidPasswordException(string? message) : base(message) { }
    public BlInvalidPasswordException(string message, Exception innerException)
                : base(message, innerException) { }
}

[Serializable]
public class BlNullPropertyException : Exception
{
    public BlNullPropertyException(string? message) : base(message) { }
}

[Serializable]
public class BlInternalLogicException : Exception
{
    public BlInternalLogicException(string? message) : base(message) { }
    public BlInternalLogicException(string message, Exception innerException)
        : base(message, innerException) { }
}

    [Serializable]
public class BlAccessDeniedException : Exception
{
    public BlAccessDeniedException(string? message) : base(message) { }
    public BlAccessDeniedException(string message, Exception innerException)
        : base(message, innerException) { }
}
public class InvalidIdException : Exception
{
    public InvalidIdException(string message) : base(message) { }
}

public class InvalidFullNameException : Exception
{
    public InvalidFullNameException(string message) : base(message) { }
}

public class InvalidPhoneNumberException : Exception
{
    public InvalidPhoneNumberException(string message) : base(message) { }
}

public class InvalidEmailException : Exception
{
    public InvalidEmailException(string message) : base(message) { }
}

public class InvalidAddressException : Exception
{
    public InvalidAddressException(string message) : base(message) { }
}

public class InvalidCoordinatesException : Exception
{
    public InvalidCoordinatesException(string message) : base(message) { }
}

public class InvalidPasswordException : Exception
{
    public InvalidPasswordException(string message) : base(message) { }
}

public class InvalidRoleException : Exception
{
    public InvalidRoleException(string message) : base(message) { }
}

public class InvalidMaxDistanceException : Exception
{
    public InvalidMaxDistanceException(string message) : base(message) { }
}
public class AddressValidationException : Exception
{
    public AddressValidationException(string message) : base(message) { }
}

[Serializable]
public class PasswordValidationException : Exception
{
    public PasswordValidationException(string? message) : base(message) { }
    public PasswordValidationException(string message, Exception innerException)
                : base(message, innerException) { }
}

    [Serializable]
public class InvalidDistanceTypeException : Exception
{
    public InvalidDistanceTypeException(string? message) : base(message) { }
    public InvalidDistanceTypeException(string message, Exception innerException)
                : base(message, innerException) { }
}
