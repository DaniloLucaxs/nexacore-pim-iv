using PIM_3sem_backend.Exceptions;

namespace PIM_3sem_backend.Models
{
    public class PontoRegistro
    {
        public Guid Id { get; private set; }
        public Guid IdFuncionario { get; private set; }
        public DateOnly Data { get; private set; }
        public DateTimeOffset HoraEntrada { get; private set; }
        public DateTimeOffset? HoraSaida { get; private set; }
        public string Status { get; private set; }

        public PontoRegistro(Guid idFuncionario, string status)
        {
            if (idFuncionario == Guid.Empty)
                throw new ModelInvalidoException("ID Funcionário Inválido.");

            if (string.IsNullOrEmpty(status))
                throw new ModelInvalidoException("Status não pode estar vazio.");

            Id = Guid.NewGuid();
            IdFuncionario = idFuncionario;
            Data = DateOnly.FromDateTime(DateTime.UtcNow);
            HoraEntrada = DateTimeOffset.UtcNow;
            HoraSaida = null;
            Status = status;
        }
        
    }
}
