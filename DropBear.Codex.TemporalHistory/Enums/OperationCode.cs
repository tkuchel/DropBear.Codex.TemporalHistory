namespace DropBear.Codex.TemporalHistory.Enums;

/// <summary>
///     Enumerates the types of operations that can be audited within the system.
/// </summary>
/// <remarks>
///     These codes are kept generic to ensure broad applicability across different domains.
///     Additions should be made with consideration to the existing codes to maintain backward compatibility.
/// </remarks>
public enum OperationCode
{
    /// <summary>
    ///     Represents the creation of a new record or entity.
    /// </summary>
    Create,

    /// <summary>
    ///     Represents an update made to an existing record or entity.
    /// </summary>
    Update,

    /// <summary>
    ///     Represents the deletion of a record or entity.
    /// </summary>
    Delete,

    /// <summary>
    ///     Represents a login attempt or session start.
    /// </summary>
    Login,

    /// <summary>
    ///     Represents a logout operation or session end.
    /// </summary>
    Logout,

    /// <summary>
    ///     Represents accessing or viewing a record or resource.
    /// </summary>
    Access,

    /// <summary>
    ///     Represents an export operation, such as exporting data to a file.
    /// </summary>
    Export,

    /// <summary>
    ///     Represents an import operation, such as importing data from a file.
    /// </summary>
    Import,

    /// <summary>
    ///     Represents a search or query operation.
    /// </summary>
    Search,

    /// <summary>
    ///     Represents an operation to approve or authorize an action or change.
    /// </summary>
    Approve

    // Further operations can be added here as needed, following the same documentation pattern.
}
