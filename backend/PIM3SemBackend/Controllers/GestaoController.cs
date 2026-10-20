using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PIM_3sem_backend.DTOs.Gestao;
using PIM_3sem_backend.Services.Gestao;
namespace PIM_3sem_backend.Controllers;
[ApiController, Authorize(Roles="Administrador,RH,Gestor"), Route("api")]
public class GestaoController(GestaoService service) : ControllerBase
{
    [HttpGet("dashboard")] public async Task<IActionResult> Dashboard()=>Ok(await service.Dashboard());
    [HttpPost("solicitacoes/simular")] public async Task<IActionResult> Simular(NovaSolicitacao dto)=>Ok(await service.Simular(dto));
    [HttpPost("solicitacoes")] public async Task<IActionResult> Criar(NovaSolicitacao dto)=>StatusCode(201,new {id=await service.Solicitar(dto)});
    [HttpGet("solicitacoes")] public async Task<IActionResult> Listar(string? status,Guid? funcionario,int pagina=1)=>Ok(await service.Solicitações(status,funcionario,pagina));
    [HttpPost("solicitacoes/{id:guid}/aprovar"),Authorize(Roles="Administrador,RH")]
    public async Task<IActionResult> Aprovar(Guid id){await service.Aprovar(id);return NoContent();}
    [HttpPost("solicitacoes/{id:guid}/rejeitar"),Authorize(Roles="Administrador,RH")]
    public async Task<IActionResult> Rejeitar(Guid id,Decisao dto){await service.Rejeitar(id,dto.Justificativa);return NoContent();}
    [HttpGet("auditoria")] public async Task<IActionResult> Auditoria(string? busca,string? operacao,Guid? funcionario,DateTimeOffset? inicio,DateTimeOffset? fim,int pagina=1)=>Ok(await service.Auditorias(busca,operacao,funcionario,inicio,fim,pagina));
}
