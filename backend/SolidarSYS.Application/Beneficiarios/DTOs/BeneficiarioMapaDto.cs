namespace SolidarSYS.Application.Beneficiarios.DTOs;

public record BeneficiarioMapaDto(
    int IdBeneficiario,
    string Nome,
    string Endereco,
    double Latitude,
    double Longitude);
