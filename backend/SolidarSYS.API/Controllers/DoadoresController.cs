using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolidarSYS.Application.Doadores.DTOs;
using SolidarSYS.Application.Doadores.Interfaces;

namespace SolidarSYS.API.Controllers;

[ApiController]
[Route("api/doadores")]
[Authorize(Roles = "ADMINISTRADOR,GESTOR")]
public sealed class DoadoresController(IDoadorService doadorService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        [FromQuery] string? busca = null,
        [FromQuery] bool? ativo = true,
        CancellationToken ct = default)
    {
        var resultado = await doadorService.ListarAsync(pagina, tamanhoPagina, busca, ativo, ct);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var doador = await doadorService.ObterPorIdAsync(id, ct);
        return Ok(doador);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarDoadorDto dto, CancellationToken ct)
    {
        var criado = await doadorService.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.IdDoador }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarDoadorDto dto, CancellationToken ct)
    {
        var atualizado = await doadorService.AtualizarAsync(id, dto, ct);
        return Ok(atualizado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
    {
        await doadorService.InativarAsync(id, ct);
        return NoContent();
    }
}