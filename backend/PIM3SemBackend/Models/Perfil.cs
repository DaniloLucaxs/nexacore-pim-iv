using PIM_3sem_backend.Exceptions;

namespace PIM_3sem_backend.Models
{
    public class Perfil
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }

        public Perfil(string nome)
        {
            if (string.IsNullOrEmpty(nome))
                throw new ModelInvalidoException("Nome do Perfil não pode estar vazio.");

            Id = Guid.NewGuid();
            Nome = nome;
        }
    }
}
