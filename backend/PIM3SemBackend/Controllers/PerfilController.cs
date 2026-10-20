using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PIM_3sem_backend.Services.Perfis;

namespace PIM_3sem_backend.Controllers
{
    [ApiController]
    [Authorize(Roles="Administrador,RH,Gestor")]
    [Route("api/[controller]")]
    public class PerfilController : ControllerBase
    {
        private readonly IPerfilService _service;
        public PerfilController(IPerfilService service)
        {
            _service = service;
        }

        [HttpGet("{perfilId}")]
        public async Task<IActionResult> ObterPorId(Guid perfilId)
        {
            var perfil = await _service.ObterPorId(perfilId);
            return Ok(perfil);
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos()
        {
            var perfis = await _service.ObterTodos();
            return Ok(perfis);
        }
    }
}
