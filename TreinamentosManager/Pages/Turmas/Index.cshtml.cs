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

            var resultado = await _teamsService.CriarReunioesTeams(turma);
            await _context.SaveChangesAsync();

            var totalComLink = turma.Datas.Count(d => !string.IsNullOrWhiteSpace(d.TeamsMeetingUrl));
            TipoMensagem = resultado.Criadas > 0 ? "success" : "warning";

            if (resultado.Criadas > 0)
            {
                Mensagem = $"Reuniões do Teams criadas: {resultado.Criadas}. Total com link: {totalComLink}.";
            }
            else if (resultado.Erros.Any())
            {
                Mensagem = $"Nenhuma reunião foi criada. Erro: {string.Join(" | ", resultado.Erros.Take(2))}";
            }
            else
            {
                Mensagem = totalComLink > 0
                    ? $"Todas as reuniões já tinham link. Total com link: {totalComLink}."
                    : "Nenhuma reunião foi criada pelo Microsoft Graph.";
            }

            return RedirectToPage();
        }
    }
}
