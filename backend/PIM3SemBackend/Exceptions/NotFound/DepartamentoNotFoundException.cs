namespace PIM_3sem_backend.Exceptions.NotFound
{
    public class DepartamentoNotFoundException : NotFoundException
    {
        public DepartamentoNotFoundException()
            : base("Departamento não encontrado") { }
    }
}
