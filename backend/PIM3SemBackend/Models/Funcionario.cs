using PIM_3sem_backend.Exceptions;

namespace PIM_3sem_backend.Models
{
    public class Funcionario
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public decimal Salario { get; private set; }
        public string Cargo { get; private set; }
        public Guid IdDepartamento { get; private set; }
        public Departamento Departamento { get; private set; } = null!;
        public Guid? IdGerente { get; private set; }
        public Funcionario Gerente { get; private set; } = null!;
        public bool Ativo { get; private set; } = true;
        public DateOnly DataAdmissao { get; private set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public Guid IdUsuario { get; private set; }
        public Usuario Usuario { get; private set; } = null!;

        public Funcionario(string nome, decimal salario, string cargo, Guid idDepartamento, Guid? idGerente, Guid idUsuario)
        {
            if (string.IsNullOrEmpty(nome))
                throw new ModelInvalidoException("Nome não pode estar vazio.");

            if (salario <= 0)
                throw new ModelInvalidoException("Salario deve ser maior que 0.");

            if (string.IsNullOrEmpty(cargo))
                throw new ModelInvalidoException("Cargo não pode estar vazio.");

            if (Guid.Empty == idDepartamento)
                throw new ModelInvalidoException("ID do departamento não pode estar vazio.");

            if (idGerente.HasValue && idGerente.Value == Guid.Empty)
                throw new ModelInvalidoException("ID do Gerente inválido.");

            if (idUsuario == Guid.Empty)
                throw new ModelInvalidoException("ID do Usuário inválido.");

            Id = Guid.NewGuid();
            Nome = nome;
            Salario = salario;
            Cargo = cargo;
            IdDepartamento = idDepartamento;
            IdGerente = idGerente;
            IdUsuario = idUsuario;
        }

        public void Editar(string nome, string cargo, Guid departamento, bool ativo, DateOnly data)
        {
            if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length > 50 || string.IsNullOrWhiteSpace(cargo) || cargo.Trim().Length > 50)
                throw new ModelInvalidoException("Nome e cargo devem conter entre 1 e 50 caracteres.");
            if (data > DateOnly.FromDateTime(DateTime.UtcNow)) throw new ModelInvalidoException("Admissão não pode ser futura.");
            Nome = nome.Trim(); Cargo = cargo.Trim(); IdDepartamento = departamento; Ativo = ativo; DataAdmissao = data;
        }
        public void TrocarGerente(Guid id) => IdGerente = id;
        public void AtualizarSalario(decimal novoSalario)
        {
            if (novoSalario <= 0)
                throw new ModelInvalidoException("Salario deve ser maior que 0.");

            Salario = novoSalario;
        }

        public void AtualizarCargo(string novoCargo)
        {
            if (string.IsNullOrEmpty(novoCargo))
                throw new ModelInvalidoException("Cargo não pode estar vazio.");

            Cargo = novoCargo;
        }
    }
}
