namespace SolidarSYS.Application.Setores.DTOs;

public record PagedResultDto<T>(IList<T> Items, int Total, int Pagina, int TamanhoPagina);
