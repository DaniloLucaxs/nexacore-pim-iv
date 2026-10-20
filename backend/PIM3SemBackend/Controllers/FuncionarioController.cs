using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PIM_3sem_backend.DTOs.Funcionarios;
using PIM_3sem_backend.DTOs.Gestao;
using PIM_3sem_backend.Services.Gestao;
namespace PIM_3sem_backend.Controllers;
[ApiController, Route("api/Funcionario"), Authorize(Roles="Administrador,RH,Gestor")]
public class FuncionarioController(GestaoService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Listar([FromQuery] ConsultaFuncionarios consulta) => Ok(await service.Listar(consulta));
    [HttpGet("{id:guid}")] public async Task<IActionResult> Detalhes(Guid id) => Ok(await service.Detalhes(id));
    [HttpGet("Gerentes")] public async Task<IActionResult> Gerentes() => Ok(await service.Gerentes());
    [HttpGet("Gerentes/{id:guid}")] public async Task<IActionResult> Gerente(Guid id) => Ok(await service.Gerentes(id));
    [HttpPost, Authorize(Roles="Administrador,RH")]
    public async Task<IActionResult> Criar(FuncionarioCreateDTO dto) {var id=await service.Criar(dto);return CreatedAtAction(nameof(Detalhes),new {id},new {id});}
    [HttpPatch("{id:guid}"), Authorize(Roles="Administrador,RH")]
    public async Task<IActionResult> Editar(Guid id,EditarFuncionario dto) {await service.Editar(id,dto);return NoContent();}
    [HttpDelete("{id:guid}"), Authorize(Roles="Administrador,RH")]
    public async Task<IActionResult> Inativar(Guid id) {await service.Inativar(id);return NoContent();}
}
