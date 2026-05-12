namespace Domain.Errors;

public class NotFoundException(string message) : Exception(message);
