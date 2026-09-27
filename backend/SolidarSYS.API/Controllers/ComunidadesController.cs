using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolidarSYS.Application.Comunidades.DTOs;
using SolidarSYS.Application.Comunidades.Interfaces;

namespace SolidarSYS.API.Controllers;

[ApiController]
[Route("api/comunidades")]
[Authorize]
public class ComunidadesController(IComunidadeService comunidadeService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        [FromQuery] string? busca = null,
        [FromQuery] bool? ativa = null,
        CancellationToken ct = default)
    {
        var resultado = await comunidadeService.ListarAsync(pagina, tamanhoPagina, busca, ativa, ct);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var comunidade = await comunidadeService.ObterPorIdAsync(id, ct);
        return Ok(comunidade);
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Criar([FromBody] CriarComunidadeDto dto, CancellationToken ct)
    {
        var criada = await comunidadeService.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = criada.IdComunidade }, criada);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarComunidadeDto dto, CancellationToken ct)
    {
        var atualizada = await comunidadeService.AtualizarAsync(id, dto, ct);
        return Ok(atualizada);
    }

    [HttpPatch("{id:int}/ativo")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> AlternarAtivo(int id, CancellationToken ct)
    {
        await comunidadeService.AlternarAtivoAsync(id, ct);
        return NoContent();
    }
}
