namespace IntegrationGateway.Api.Modules.MancoReporting.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}
