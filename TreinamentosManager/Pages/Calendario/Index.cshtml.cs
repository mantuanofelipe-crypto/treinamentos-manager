using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Calendario
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public DateTime MesAtual { get; set; }
        public DateTime MesAnterior => MesAtual.AddMonths(-1);
        public DateTime ProximoMes => MesAtual.AddMonths(1);

        public int? FiltroClienteId { get; set; }
        public string? FiltroTipoCliente { get; set; }
        public string? FiltroModalidade { get; set; }
        public string? FiltroInstrutorId { get; set; }

        public IList<Cliente> Clientes { get; set; } = new List<Cliente>();
        public IList<Instrutor> Instrutores { get; set; } = new List<Instrutor>();
        public List<CalendarioDia> Dias { get; set; } = new();
        public string[] DiasSemana { get; } = { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };

        public async Task OnGetAsync(int? ano, int? mes, int? clienteId, string? tipoCliente, string? modalidade, string? instrutorId)
        {
            var hoje = DateTime.Today;
            MesAtual = new DateTime(ano ?? hoje.Year, mes ?? hoje.Month, 1);
            FiltroClienteId = clienteId;
            FiltroTipoCliente = tipoCliente;
            FiltroModalidade = modalidade;
            FiltroInstrutorId = instrutorId;

            Clientes = await _context.Clientes.Where(c => c.Ativo).OrderBy(c => c.Nome).ToListAsync();
            Instrutores = await _context.Instrutores.Where(i => i.Ativo).OrderBy(i => i.Nome).ToListAsync();

            var inicioCalendario = MesAtual.AddDays(-(int)MesAtual.DayOfWeek);
            var fimMes = MesAtual.AddMonths(1).AddDays(-1);
            var fimCalendario = fimMes.AddDays(6 - (int)fimMes.DayOfWeek);

            var query = _context.TurmaDatas
                .Include(d => d.Turma)
                    .ThenInclude(t => t.Cliente)
                .Include(d => d.Turma)
                    .ThenInclude(t => t.Software)
                .Include(d => d.Turma)
                    .ThenInclude(t => t.Instrutor)
                .Where(d => d.Data >= inicioCalendario && d.Data <= fimCalendario.AddDays(1))
                .AsQueryable();

            if (clienteId.HasValue)
                query = query.Where(d => d.Turma.ClienteId == clienteId.Value);

            if (!string.IsNullOrWhiteSpace(tipoCliente))
                query = query.Where(d => d.Turma.TipoCliente == tipoCliente);

            if (!string.IsNullOrWhiteSpace(modalidade))
                query = query.Where(d => d.Turma.Modalidade == modalidade);

            if (!string.IsNullOrWhiteSpace(instrutorId))
                query = query.Where(d => d.Turma.InstrutorId == instrutorId);

            var datas = await query
                .OrderBy(d => d.Data)
                .ToListAsync();

            var eventos = datas
                .Select(d => new CalendarioEvento
                {
                    TurmaId = d.TurmaId,
                    Inicio = d.Data,
                    Fim = d.Fim,
                    Cliente = d.Turma.Cliente != null ? d.Turma.Cliente.Nome : "-",
                    Software = d.Turma.Software != null ? d.Turma.Software.Nome : "-",
                    Instrutor = d.Turma.Instrutor != null ? d.Turma.Instrutor.Nome : "-",
                    Modalidade = d.Turma.Modalidade,
                    TipoCliente = d.Turma.TipoCliente
                })
                .ToList();

            for (var data = inicioCalendario; data <= fimCalendario; data = data.AddDays(1))
            {
                Dias.Add(new CalendarioDia
                {
                    Data = data,
                    EhMesAtual = data.Month == MesAtual.Month,
                    Eventos = eventos.Where(e => e.Inicio.Date == data.Date).ToList()
                });
            }
        }
    }

    public class CalendarioDia
    {
        public DateTime Data { get; set; }
        public bool EhMesAtual { get; set; }
        public List<CalendarioEvento> Eventos { get; set; } = new();
    }

    public class CalendarioEvento
    {
        public int TurmaId { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Software { get; set; } = string.Empty;
        public string Instrutor { get; set; } = string.Empty;
        public string Modalidade { get; set; } = string.Empty;
        public string TipoCliente { get; set; } = string.Empty;
    }
}
