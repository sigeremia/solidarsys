using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolidarSYS.Application.Relatorios.Interfaces;

namespace SolidarSYS.API.Controllers;

[ApiController]
[Route("api/relatorios")]
[Authorize(Roles = "ADMINISTRADOR,GESTOR")]
public class RelatoriosController(IRelatorioService relatorioService) : ControllerBase
{
    /// <param name="periodo">mensal, trimestral, semestral (padrão) ou anual.</param>
    [HttpGet]
    public async Task<IActionResult> Obter([FromQuery] string? periodo = null, CancellationToken ct = default)
    {
        var resultado = await relatorioService.ObterAsync(periodo, ct);
        return Ok(resultado);
    }
}
