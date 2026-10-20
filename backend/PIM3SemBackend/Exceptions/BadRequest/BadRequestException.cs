namespace PIM_3sem_backend.Exceptions.BadRequest
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message)
            : base(message) { }
    }
}
