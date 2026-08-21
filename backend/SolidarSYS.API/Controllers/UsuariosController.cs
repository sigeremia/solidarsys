using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolidarSYS.Application.Usuarios.DTOs;
using SolidarSYS.Application.Usuarios.Interfaces;

namespace SolidarSYS.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController(IUsuarioService usuarioService) : ControllerBase
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
        var resultado = await usuarioService.ListarAsync(pagina, tamanhoPagina, busca, ativo, ct);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMINISTRADOR,GESTOR")]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var usuario = await usuarioService.ObterPorIdAsync(id, ct);
        return Ok(usuario);
    }

    [HttpPost]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Criar([FromBody] CriarUsuarioDto dto, CancellationToken ct)
    {
        var criado = await usuarioService.CriarAsync(dto, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarUsuarioDto dto, CancellationToken ct)
    {
        var atualizado = await usuarioService.AtualizarAsync(id, dto, ct);
        return Ok(atualizado);
    }

    [HttpPatch("{id:int}/ativo")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> AlternarAtivo(int id, CancellationToken ct)
    {
        await usuarioService.AlternarAtivoAsync(id, ct);
        return NoContent();
    }
}
