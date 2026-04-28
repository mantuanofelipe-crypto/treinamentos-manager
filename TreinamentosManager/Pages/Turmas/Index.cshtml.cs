using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;
using TreinamentosManager.Services;

namespace TreinamentosManager.Pages.Turmas
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly TeamsService _teamsService;

        public IndexModel(ApplicationDbContext context, TeamsService teamsService)
        {
            _context = context;
            _teamsService = teamsService;
        }

        public IList<Turma> Turmas { get; set; } = default!;
        public IList<Cliente> Clientes { get; set; } = default!;
        public IList<Instrutor> Instrutores { get; set; } = default!;

        public string? FiltroStatus { get; set; }
        public int? FiltroClienteId { get; set; }
        public string? FiltroInstrutorId { get; set; }
        public string? FiltroModalidade { get; set; }

        [TempData]
        public string? Mensagem { get; set; }

        [TempData]
        public string? TipoMensagem { get; set; }

        public async Task OnGetAsync(string? status, int? clienteId, string? instrutorId, string? modalidade)
        {
            FiltroStatus = status;
            FiltroClienteId = clienteId;
            FiltroInstrutorId = instrutorId;
            FiltroModalidade = modalidade;

            Clientes = await _context.Clientes.OrderBy(c => c.Nome).ToListAsync();
            Instrutores = await _context.Instrutores.OrderBy(i => i.Nome).ToListAsync();

            var query = _context.Turmas
                .Include(t => t.Instrutor)
                .Include(t => t.Cliente)
                .Include(t => t.Software)
                .Include(t => t.Datas)
                .AsQueryable();

            if (clienteId.HasValue)
                query = query.Where(t => t.ClienteId == clienteId.Value);

            if (!string.IsNullOrEmpty(instrutorId))
                query = query.Where(t => t.InstrutorId == instrutorId);

            if (!string.IsNullOrEmpty(modalidade))
                query = query.Where(t => t.Modalidade == modalidade);

            var turmas = await query.ToListAsync();

            if (!string.IsNullOrEmpty(status))
                turmas = turmas.Where(t => t.Status == status).ToList();

            Turmas = turmas;
        }

        public async Task<IActionResult> OnPostCriarTeamsAsync(int id)
        {
            var turma = await _context.Turmas
                .Include(t => t.Cliente)
                .Include(t => t.Software)
                .Include(t => t.Instrutor)
                .Include(t => t.Datas)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turma == null)
                return NotFound();

            if (!turma.Datas.Any())
            {
                TipoMensagem = "warning";
                Mensagem = "Cadastre ao menos uma data antes de criar as reuniões no Teams.";
                return RedirectToPage();
            }

            if (!_teamsService.EstaConfigurado)
            {
                var pendentes = _teamsService.ObterVariaveisPendentes();
                TipoMensagem = "warning";
                Mensagem = pendentes.Any()
                    ? $"Microsoft Teams ainda não está configurado. Verifique no Railway: {string.Join(", ", pendentes)}."
                    : "Microsoft Teams ainda não está configurado nas variáveis do Railway.";
                return RedirectToPage();
            }

            await _teamsService.CriarReunioesTeams(turma);
            await _context.SaveChangesAsync();

            var criadas = turma.Datas.Count(d => !string.IsNullOrWhiteSpace(d.TeamsMeetingUrl));
            TipoMensagem = criadas > 0 ? "success" : "warning";
            Mensagem = criadas > 0
                ? $"Reuniões do Teams atualizadas para {criadas} data(s)."
                : "Nenhuma reunião foi criada. Verifique os logs do Railway/Microsoft Graph.";

            return RedirectToPage();
        }
    }
}
