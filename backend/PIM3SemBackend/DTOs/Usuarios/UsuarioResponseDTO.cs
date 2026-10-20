namespace PIM_3sem_backend.DTOs.Usuarios
{
    public class UsuarioResponseDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public string Perfil { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
