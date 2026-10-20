namespace PIM_3sem_backend.DTOs.Usuarios
{
    public class UsuarioCreateDTO
    {
        public string Email { get; set; } = "";
        public string Senha { get; set; } = "";
        public Guid IdPerfil { get; set; }
    }
}
