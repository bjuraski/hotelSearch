namespace HotelSearch.Application.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a duplicate hotel
/// </summary>
public class DuplicateHotelException : Exception
{
    public DuplicateHotelException(string message) : base(message)
    {
    }

    public DuplicateHotelException(string message, Exception innerException) : base(message, innerException)
    {
    }
}