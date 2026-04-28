using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;
using TreinamentosManager.Services;

namespace TreinamentosManager.Pages.Turmas
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly TeamsService _teamsService;
        private readonly EmailService _emailService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            ApplicationDbContext context,
            TeamsService teamsService,
            EmailService emailService,
            ILogger<CreateModel> logger)
        {
            _context = context;
            _teamsService = teamsService;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public Turma Turma { get; set; } = default!;

        [BindProperty]
        public List<DateTime?> DatasTurma { get; set; } = new();

        [BindProperty]
        public List<decimal?> DuracoesTurma { get; set; } = new();

        public IActionResult OnGet()
        {
            CarregarViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var encontros = MontarEncontros();

            if (!encontros.Any())
                ModelState.AddModelError(nameof(DatasTurma), "Informe pelo menos uma data da turma.");

            if (encontros.Any(e => e.DuracaoHoras <= 0))
                ModelState.AddModelError(nameof(DuracoesTurma), "A duração de cada aula deve ser maior que zero.");

            if (!ModelState.IsValid)
            {
                CarregarViewData();
                return Page();
            }

            Turma.Datas = encontros;

            _context.Turmas.Add(Turma);
            await _context.SaveChangesAsync();

            await _context.Entry(Turma).Reference(t => t.Cliente).LoadAsync();
            await _context.Entry(Turma).Reference(t => t.Software).LoadAsync();
            await _context.Entry(Turma).Reference(t => t.Instrutor).LoadAsync();

            await _teamsService.CriarReunioesTeams(Turma);
            await _context.SaveChangesAsync();

            if (Turma.Instrutor != null)
            {
                try
                {
                    await _emailService.EnviarEmailInstrutor(Turma);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao enviar email para o instrutor da turma {TurmaId}.", Turma.Id);
                }
            }

            return RedirectToPage("./Index");
        }

        private void CarregarViewData()
        {
            ViewData["InstrutorId"] = new SelectList(_context.Instrutores.Where(i => i.Ativo).OrderBy(i => i.Nome), "Id", "Nome");
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Ativo).OrderBy(c => c.Nome), "Id", "Nome");
            ViewData["SoftwareId"] = new SelectList(_context.Softwares.OrderBy(s => s.Nome), "Id", "Nome");
        }

        private List<TurmaData> MontarEncontros()
        {
            return DatasTurma
                .Select((data, index) => new
                {
                    Data = data,
                    Duracao = index < DuracoesTurma.Count ? DuracoesTurma[index] : null
                })
                .Where(item => item.Data.HasValue)
                .GroupBy(item => item.Data!.Value)
                .Select(group =>
                {
                    var item = group.First();
                    return new TurmaData
                    {
                        Data = item.Data!.Value,
                        DuracaoHoras = item.Duracao.GetValueOrDefault(1)
                    };
                })
                .OrderBy(data => data.Data)
                .ToList();
        }
    }
}
