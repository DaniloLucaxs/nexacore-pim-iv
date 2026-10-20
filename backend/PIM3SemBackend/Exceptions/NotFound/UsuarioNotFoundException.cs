namespace PIM_3sem_backend.Exceptions.NotFound
{
    public class UsuarioNotFoundException : NotFoundException
    {
        public UsuarioNotFoundException()
            : base("Usuario não encontrado") { }
    }
}
