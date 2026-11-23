namespace SolidarSYS.Application.Dashboard.DTOs;

public record EvolucaoMensalDto(int Ano, int Mes, int Beneficiarios, int? Doadores);

public record ComunidadeResumoDto(string Nome, int TotalBeneficiarios);

public record DashboardDto(
    int TotalBeneficiarios,
    int? TotalDoadores,
    int VisitasMes,
    int TotalComunidades,
    IList<EvolucaoMensalDto> EvolucaoMensal,
    IList<ComunidadeResumoDto> Comunidades);
