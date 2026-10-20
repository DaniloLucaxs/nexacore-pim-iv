using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PIM_3sem_backend.DTOs.Usuarios;
using PIM_3sem_backend.Services.Usuarios;

namespace PIM_3sem_backend.Controllers
{
    [ApiController]
    [Authorize(Roles="Administrador")]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _service;
        public UsuarioController(IUsuarioService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CriarNovoUsuario([FromBody] UsuarioCreateDTO novoUsuario)
        {
            var usuarioCriado = await _service.CriarUsuario(novoUsuario);
            return CreatedAtAction(nameof(ObterPorId), new { usuarioId = usuarioCriado.Id }, usuarioCriado);
        }

        [HttpGet("{usuarioId}")]
        public async Task<IActionResult> ObterPorId(Guid usuarioId)
        {
            var usuario = await _service.ObterPorId(usuarioId);
            return Ok(usuario);
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos()
        {
            var usuarios = await _service.ObterTodos();
            return Ok(usuarios);
        }

        [HttpPatch("{usuarioId}")]
        public async Task<IActionResult> AlterarAcesso(Guid usuarioId)
        {
            await _service.AlterarAcesso(usuarioId);
            return NoContent();
        }

        [HttpDelete("{usuarioId}")]
        public async Task<IActionResult> RemoverUsuario(Guid usuarioId)
        {
            await _service.RemoverUsuario(usuarioId);
            return NoContent();
        }
    }
}
