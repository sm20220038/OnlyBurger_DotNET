namespace OnlyBurger.Api.Common.Exceptions;

/// <summary>
/// Base type for expected, business-level failures. Each carries the HTTP status code
/// that the exception-handling middleware should translate it into.
/// </summary>
public abstract class AppException : Exception
{
    public abstract int StatusCode { get; }

    protected AppException(string message) : base(message)
    {
    }
}

/// <summary>400 — the request was understood but is invalid.</summary>
public class ValidationException : AppException
{
    public override int StatusCode => StatusCodes.Status400BadRequest;
    public ValidationException(string message) : base(message) { }
}

/// <summary>401 — authentication failed (bad credentials).</summary>
public class UnauthorizedAppException : AppException
{
    public override int StatusCode => StatusCodes.Status401Unauthorized;
    public UnauthorizedAppException(string message) : base(message) { }
}

/// <summary>403 — the caller is authenticated but not allowed to perform this action.</summary>
public class ForbiddenException : AppException
{
    public override int StatusCode => StatusCodes.Status403Forbidden;
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>404 — the requested resource does not exist.</summary>
public class NotFoundException : AppException
{
    public override int StatusCode => StatusCodes.Status404NotFound;
    public NotFoundException(string message) : base(message) { }
}

/// <summary>409 — the request conflicts with current state (e.g. duplicate username).</summary>
public class ConflictException : AppException
{
    public override int StatusCode => StatusCodes.Status409Conflict;
    public ConflictException(string message) : base(message) { }
}
