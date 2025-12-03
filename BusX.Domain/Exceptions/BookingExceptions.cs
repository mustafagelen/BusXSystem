namespace BusX.Domain.Exceptions;

public class SeatUnavailableException : Exception
{
    public SeatUnavailableException(string message) : base(message) { }
}

public class GenderMismatchException : Exception
{
    public GenderMismatchException(string message) : base(message) { }
}

public class PaymentFailedException : Exception
{
    public PaymentFailedException(string message) : base(message) { }
}