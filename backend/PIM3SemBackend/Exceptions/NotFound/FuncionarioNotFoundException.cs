namespace PIM_3sem_backend.Exceptions.NotFound
{
    public class FuncionarioNotFoundException : NotFoundException
    {
        public FuncionarioNotFoundException()
            : base("Funcionario não encontrado.") { }
    }
}
