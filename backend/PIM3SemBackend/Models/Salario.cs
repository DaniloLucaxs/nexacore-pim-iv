using PIM_3sem_backend.Exceptions;

namespace PIM_3sem_backend.Models
{
    public class Salario
    {
        public Guid Id { get; private set; }
        public Guid IdFuncionario { get; private set; }
        public decimal Valor { get; private set; }
        public DateOnly DataInicio { get; private set; }
        public DateOnly? DataFim { get; private set;  }
        public string? MotivoAlteracao { get; private set; }

        public Salario(Guid idFuncionario, decimal valor)
        {
            if (idFuncionario == Guid.Empty)
                throw new ModelInvalidoException("ID funcionário Inválido.");

            if (valor <= 0)
                throw new ModelInvalidoException("Valor do Salário deve ser maior que 0.");

            Id = Guid.NewGuid();
            IdFuncionario = idFuncionario;
            Valor = valor;
            DataInicio = DateOnly.FromDateTime(DateTime.UtcNow);
            DataFim = null;
            MotivoAlteracao = null;
        }
    }
}
