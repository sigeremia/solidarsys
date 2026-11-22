namespace SolidarSYS.Application.Beneficiarios.Interfaces;

public interface IGeocodificacaoService
{
    Task<(double Latitude, double Longitude)? > GeocodificarAsync(string endereco, CancellationToken ct = default);
}
