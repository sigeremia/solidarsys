using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolidarSYS.Application.Dashboard.Interfaces;

namespace SolidarSYS.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "ADMINISTRADOR,GESTOR,OPERADOR")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obter(CancellationToken ct)
    {
        // Doadores só para quem já tem acesso ao módulo (mesma regra do DoadoresController)
        var incluirDoadores = User.IsInRole("ADMINISTRADOR") || User.IsInRole("GESTOR");
        var resultado = await dashboardService.ObterAsync(incluirDoadores, ct);
        return Ok(resultado);
    }
}
