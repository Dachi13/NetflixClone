using Shared.Models;

namespace Shared.Library;

public class Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }
    
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public Error(string code, string message, ErrorType errorType)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message;
        ErrorType = errorType;
    }
}