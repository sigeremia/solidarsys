using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolidarSYS.Application.Beneficiarios.DTOs;
using SolidarSYS.Application.Beneficiarios.Interfaces;

namespace SolidarSYS.API.Controllers;

[ApiController]
[Route("api/beneficiarios")]
[Authorize(Roles = "ADMINISTRADOR,GESTOR,OPERADOR")]
public class BeneficiariosController(IBeneficiarioService beneficiarioService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        [FromQuery] string? busca = null,
        [FromQuery] bool? ativo = null,
        CancellationToken ct = default)
    {
        var resultado = await beneficiarioService.ListarAsync(pagina, tamanhoPagina, busca, ativo, ct);
        return Ok(resultado);
    }

    // Comunidades ativas para o select do formulário (acessível a quem cadastra beneficiários).
    [HttpGet("comunidades")]
    public async Task<IActionResult> ListarComunidades(CancellationToken ct)
    {
        var comunidades = await beneficiarioService.ListarComunidadesAsync(ct);
        return Ok(comunidades);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var beneficiario = await beneficiarioService.ObterPorIdAsync(id, ct);
        return Ok(beneficiario);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarBeneficiarioDto dto, CancellationToken ct)
    {
        var criado = await beneficiarioService.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.IdBeneficiario }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarBeneficiarioDto dto, CancellationToken ct)
    {
        var atualizado = await beneficiarioService.AtualizarAsync(id, dto, ct);
        return Ok(atualizado);
    }

    // "Remover" = inativar (exclusão lógica, preserva histórico).
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
    {
        await beneficiarioService.InativarAsync(id, ct);
        return NoContent();
    }
}