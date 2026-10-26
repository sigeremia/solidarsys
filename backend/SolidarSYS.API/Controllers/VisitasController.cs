using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolidarSYS.Application.Visitas.DTOs;
using SolidarSYS.Application.Visitas.Interfaces;

namespace SolidarSYS.API.Controllers;

[ApiController]
[Route("api/visitas")]
[Authorize(Roles = "ADMINISTRADOR,GESTOR,OPERADOR")]
public class VisitasController(IVisitaService visitaService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        [FromQuery] string? busca = null,
        CancellationToken ct = default)
    {
        var resultado = await visitaService.ListarAsync(pagina, tamanhoPagina, busca, ct);
        return Ok(resultado);
    }

    [HttpGet("beneficiarios")]
    public async Task<IActionResult> ListarBeneficiarios(CancellationToken ct)
    {
        var beneficiarios = await visitaService.ListarBeneficiariosAsync(ct);
        return Ok(beneficiarios);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var visita = await visitaService.ObterPorIdAsync(id, ct);
        return Ok(visita);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarVisitaDto dto, CancellationToken ct)
    {
        var visita = await visitaService.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = visita.IdVisita }, visita);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarVisitaDto dto, CancellationToken ct)
    {
        var visita = await visitaService.AtualizarAsync(id, dto, ct);
        return Ok(visita);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await visitaService.ExcluirAsync(id, ct);
        return NoContent();
    }
}
