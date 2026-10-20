using PIM_3sem_backend.Exceptions.BadRequest;

namespace PIM_3sem_backend.Exceptions
{
    public class ModelInvalidoException : BadRequestException
    {
        public ModelInvalidoException(string mensagem)
            : base(mensagem) { }
    }
}
