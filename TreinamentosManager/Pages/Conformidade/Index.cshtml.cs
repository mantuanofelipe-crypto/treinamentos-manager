using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Conformidade
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Turma> Turmas { get; set; } = new List<Turma>();

        public string[] StatusOptions => ConformidadeStatus.Todos;

        public string? FiltroStatus { get; set; }
        public string? FiltroConformidade { get; set; }

        [TempData]
        public string? Mensagem { get; set; }

        [BindProperty]
        public TurmaConformidade Registro { get; set; } = default!;

        public async Task OnGetAsync(string? status, string? conformidade)
        {
            FiltroStatus = status;
            FiltroConformidade = conformidade;

            var turmas = await _context.Turmas
                .Include(t => t.Cliente)
                .Include(t => t.Software)
                .Include(t => t.Instrutor)
                .Include(t => t.ConformidadeDetalhes)
                .OrderByDescending(t => t.Inicio)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(status))
                turmas = turmas.Where(t => t.Status == status).ToList();

            if (conformidade == "100")
            {
                turmas = turmas
                    .Where(t => t.ConformidadeDetalhes?.ConformidadeGeral == 100m)
                    .ToList();
            }
            else if (conformidade == "menor100")
            {
                turmas = turmas
                    .Where(t => (t.ConformidadeDetalhes?.ConformidadeGeral ?? 0m) < 100m)
                    .ToList();
            }

            Turmas = turmas;
        }

        public async Task<IActionResult> OnPostSalvarAsync()
        {
            var turmaExiste = await _context.Turmas.AnyAsync(t => t.Id == Registro.TurmaId);
            if (!turmaExiste)
                return NotFound();

            NormalizarStatus(Registro);

            var conformidade = await _context.TurmaConformidades
                .FirstOrDefaultAsync(c => c.TurmaId == Registro.TurmaId);

            if (conformidade == null)
            {
                conformidade = new TurmaConformidade { TurmaId = Registro.TurmaId };
                _context.TurmaConformidades.Add(conformidade);
            }

            conformidade.FichaInscricao = Registro.FichaInscricao;
            conformidade.Pauta = Registro.Pauta;
            conformidade.Convite = Registro.Convite;
            conformidade.ConfiguracaoHub = Registro.ConfiguracaoHub;
            conformidade.ScriptAula = Registro.ScriptAula;
            conformidade.MaterialAula = Registro.MaterialAula;
            conformidade.BancoQuestoes = Registro.BancoQuestoes;
            conformidade.Nuvem = Registro.Nuvem;
            conformidade.PreenchimentoPauta = Registro.PreenchimentoPauta;
            conformidade.Certificado = Registro.Certificado;
            conformidade.JustificativaInstrutor = Registro.JustificativaInstrutor;
            conformidade.JustificativaCoordenacao = Registro.JustificativaCoordenacao;

            await _context.SaveChangesAsync();

            Mensagem = "Conformidade salva com sucesso.";
            return RedirectToPage();
        }

        public static string FormatarPercentual(decimal? value)
        {
            return value.HasValue ? $"{value.Value:0.#}%" : "—";
        }

        private static void NormalizarStatus(TurmaConformidade registro)
        {
            registro.FichaInscricao = Normalizar(registro.FichaInscricao);
            registro.Pauta = Normalizar(registro.Pauta);
            registro.Convite = Normalizar(registro.Convite);
            registro.ConfiguracaoHub = Normalizar(registro.ConfiguracaoHub);
            registro.ScriptAula = Normalizar(registro.ScriptAula);
            registro.MaterialAula = Normalizar(registro.MaterialAula);
            registro.BancoQuestoes = Normalizar(registro.BancoQuestoes);
            registro.Nuvem = Normalizar(registro.Nuvem);
            registro.PreenchimentoPauta = Normalizar(registro.PreenchimentoPauta);
            registro.Certificado = Normalizar(registro.Certificado);
        }

        private static string Normalizar(string? status)
        {
            return ConformidadeStatus.Todos.Contains(status) ? status! : ConformidadeStatus.Pendente;
        }
    }
}
