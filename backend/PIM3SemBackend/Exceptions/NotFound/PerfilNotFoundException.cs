namespace PIM_3sem_backend.Exceptions.NotFound
{
    public class PerfilNotFoundException : NotFoundException
    {
        public PerfilNotFoundException()
            : base("Perfil não Encontrado.") { }
    }
}
