namespace DO;
/// <summary>
/// Represents an assignment entity that links a volunteer to a call they chose to handle.
/// </summary>
/// <param name="Id">A unique running identifier for the assignment entity.</param>
/// <param name="CallId">The unique identifier of the call assigned to the volunteer.</param>
/// <param name="VolunteerId">The ID of the volunteer handling the call.</param>
/// <param name="EntryTime">The date and time when the assignment was created, marking when the volunteer started handling the call.</param>
/// <param name="CompletionTime">The date and time when the call was completed by the volunteer. If the call is not yet completed, this value is null.</param>
/// <param name="CompletionType">
/// Indicates the type of completion for the assignment. Possible values include:
/// - Treated: The call was successfully handled on time.
/// - VolunteerCancelled: The volunteer chose to cancel the assignment.
/// - AdminCancelled: The administrator cancelled the assignment.
/// - Expired: The call expired because it was not handled by its maximum completion time.
/// This value is null if the call is not yet completed.
/// </param>
public record Assignment
(
    int Id,
    int CallId,
    int VolunteerId,
    DateTime EntryTime,
    DateTime? CompletionTime = null,
    AssignmentCompletionType? CompletionType = null
)
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public Assignment(int Id, int CallId) : this(0, 0, 0, DateTime.Now) { }
}


