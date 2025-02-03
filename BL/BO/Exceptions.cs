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
public class DatabaseInitializationException : Exception
{
    public DatabaseInitializationException()
        : base("Failed to initialize the database.")
    { }

    public DatabaseInitializationException(string message)
        : base(message)
    { }

    public DatabaseInitializationException(string message, Exception innerException)
        : base(message, innerException)
    { }
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

[Serializable]
public class BlVolunteerCannotBeDeletedException : Exception
{
    public BlVolunteerCannotBeDeletedException(string message)
        : base(message) { }
}

[Serializable]
public class BlAlreadyExistsException : Exception
{
    public BlAlreadyExistsException(string message)
        : base(message) { }
    public BlAlreadyExistsException(string message, Exception innerException)
        : base(message, innerException)
    { }

}

[Serializable]
public class InvalidIdException : Exception
{
    public InvalidIdException(string? message) : base(message) { }
}

[Serializable]
public class InvalidFullNameException : Exception
{
    public InvalidFullNameException(string? message) : base(message) { }
}

[Serializable]
public class InvalidPhoneNumberException : Exception
{
    public InvalidPhoneNumberException(string? message) : base(message) { }
}

[Serializable]
public class InvalidEmailException : Exception
{
    public InvalidEmailException(string? message) : base(message) { }
}

[Serializable]
public class PasswordValidationException : Exception
{
    public PasswordValidationException(string? message) : base(message) { }
}

[Serializable]
public class AddressValidationException : Exception
{
    public AddressValidationException(string? message) : base(message) { }
}

[Serializable]
public class InvalidCoordinatesException : Exception
{
    public InvalidCoordinatesException(string? message) : base(message) { }
}

[Serializable]
public class InvalidRoleException : Exception
{
    public InvalidRoleException(string? message) : base(message) { }
}

[Serializable]
public class InvalidMaxDistanceException : Exception
{
    public InvalidMaxDistanceException(string? message) : base(message) { }
}

[Serializable]
public class InvalidDistanceTypeException : Exception
{
    public InvalidDistanceTypeException(string? message) : base(message) { }
}
public class BlArgumentException : Exception
{
    public BlArgumentException(string? message) : base(message) { }

}
public class InvalidCallTypeException : Exception
{
    public InvalidCallTypeException(string message) : base(message)
    {
    }

  
    public InvalidCallTypeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
public class InvalidStatusException : Exception
{
    public InvalidStatusException(string message)
        : base(message)
    {
    }

    
    public InvalidStatusException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
public class BlCallAssignedException : Exception
{
    public BlCallAssignedException(string message) : base(message) { }
    public BlCallAssignedException(string message, Exception inner) : base(message, inner) { }
}

public class BlInvalidStatusException : Exception
{
    public BlInvalidStatusException(string message) : base(message) { }
    public BlInvalidStatusException(string message, Exception inner) : base(message, inner) { }
}
public class BlGeneralException : Exception
{
    public BlGeneralException(string message) : base(message) { }
    public BlGeneralException(string message, Exception inner) : base(message, inner) { }
}
public class BlInvalidRoleException : Exception
{
    public BlInvalidRoleException(string message) : base(message) { }
}

    /// <summary>
    /// Exception thrown when a provided value is invalid for an operation.
    /// </summary>
    public class BlInvalidValueException : Exception
    {
        public BlInvalidValueException() { }

        public BlInvalidValueException(string message) : base(message) { }

        public BlInvalidValueException(string message, Exception innerException) : base(message, innerException) { }
    }

    
    /// <summary>
    /// Exception thrown when a requested entity is not found in the system.
    /// </summary>
    public class BlNotFoundException : Exception
    {
        public BlNotFoundException() { }

        public BlNotFoundException(string message) : base(message) { }

        public BlNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when an operation is not allowed due to system rules.
    /// </summary>
    public class BlOperationNotAllowedException : Exception
    {
        public BlOperationNotAllowedException() { }

        public BlOperationNotAllowedException(string message) : base(message) { }

        public BlOperationNotAllowedException(string message, Exception innerException) : base(message, innerException) { }
    }
public class IncorrectPasswordException : Exception
{
    public IncorrectPasswordException() : base() { }

    public IncorrectPasswordException(string message) : base(message) { }

    public IncorrectPasswordException(string message, Exception innerException)
        : base(message, innerException) { }
}
public class BlLockoutException : Exception
{
    public BlLockoutException() : base() { }
    public BlLockoutException(string message) : base(message) { }
    public BlLockoutException(string message, Exception innerException) : base(message, innerException) { }
}
public class BlUpdateException : Exception
{
    // Constructor עם הודעה בלבד
    public BlUpdateException(string message) : base(message)
    {
    }

    // Constructor עם הודעה וחריגה פנימית
    public BlUpdateException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}