using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SolidarSYS.Application.Beneficiarios.Interfaces;

namespace SolidarSYS.Infrastructure.Beneficiarios;

public class NominatimGeocodificacaoService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<NominatimGeocodificacaoService> logger) : IGeocodificacaoService
{
    private sealed record NominatimResult(string lat, string lon);

    public async Task<(double Latitude, double Longitude)? > GeocodificarAsync(
        string endereco,
        CancellationToken ct = default)
    {
        try
        {
            var baseUrl = configuration["Nominatim:BaseUrl"] ?? "https://nominatim.openstreetmap.org";
            var userAgent = configuration["Nominatim:UserAgent"] ?? "SolidarSYS/1.0";
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{baseUrl.TrimEnd('/')}/search?format=jsonv2&limit=1&countrycodes=br&addressdetails=0&q={Uri.EscapeDataString(endereco)}");
            request.Headers.TryAddWithoutValidation("User-Agent", userAgent);

            using var response = await httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Nominatim returned HTTP {StatusCode} for address geocoding.", (int)response.StatusCode);
                return null;
            }

            var resultados = await response.Content.ReadFromJsonAsync<NominatimResult[]>(cancellationToken: ct);
            var resultado = resultados?.FirstOrDefault();
            if (resultado is null
                || !double.TryParse(resultado.lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
                || !double.TryParse(resultado.lon, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
            {
                return null;
            }

            return (latitude, longitude);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not geocode beneficiary address.");
            return null;
        }
    }
}
