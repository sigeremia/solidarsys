using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolidarSYS.Application.Setores.DTOs;
using SolidarSYS.Application.Setores.Interfaces;

namespace SolidarSYS.API.Controllers;

[ApiController]
[Route("api/setores")]
[Authorize]
public class SetoresController(ISetoresService setoresService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        [FromQuery] string? busca = null,
        [FromQuery] bool? ativo = null,
        CancellationToken ct = default)
    {
        var resultado = await setoresService.ListarAsync(pagina, tamanhoPagina, busca, ativo, ct);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var setor = await setoresService.ObterPorIdAsync(id, ct);
        return Ok(setor);
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Criar([FromBody] CriarSetoresDto dto, CancellationToken ct)
    {
        var criada = await setoresService.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = criada.IdSetores }, criada);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarSetoresDto dto, CancellationToken ct)
    {
        var atualizada = await setoresService.AtualizarAsync(id, dto, ct);
        return Ok(atualizada);
    }

    [HttpPatch("{id:int}/ativo")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> AlternarAtivo(int id, CancellationToken ct)
    {
        await setoresService.AlternarAtivoAsync(id, ct);
        return NoContent();
    }
}
