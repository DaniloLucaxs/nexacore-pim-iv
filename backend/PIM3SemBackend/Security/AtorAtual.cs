using System.Security.Claims;
namespace PIM_3sem_backend.Security;
public class AtorAtual(IHttpContextAccessor accessor)
{
    public Guid? Id => Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public bool Gestor => accessor.HttpContext?.User.IsInRole("Gestor") == true;
    public bool PodeGerir => accessor.HttpContext?.User.IsInRole("Administrador") == true || accessor.HttpContext?.User.IsInRole("RH") == true;
}
