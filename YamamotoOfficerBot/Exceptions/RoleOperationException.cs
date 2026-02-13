namespace YamamotoOfficerBot.Exceptions;

public enum RoleOperationErrorType
{
    RoleNotFound,
    MissingPermissions,
    NetworkError,
    RateLimited,
    Unknown
}

public class RoleOperationException(RoleOperationErrorType errorType) : Exception(GetMessageForErrorType(errorType))
{
    public RoleOperationErrorType ErrorType { get; } = errorType;
    public string UserMessage { get; } = GetMessageForErrorType(errorType);

    private static string GetMessageForErrorType(RoleOperationErrorType errorType)
    {
        return errorType switch
        {
            RoleOperationErrorType.RoleNotFound => Messages.RoleNotFound,
            RoleOperationErrorType.MissingPermissions => Messages.MissingPermissions,
            RoleOperationErrorType.NetworkError => Messages.NetworkError,
            RoleOperationErrorType.RateLimited => Messages.RateLimited,
            _ => Messages.Unknown
        };
    }
}
