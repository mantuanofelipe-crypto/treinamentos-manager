using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TreinamentosManager.Models;
using TreinamentosManager.Services;

namespace TreinamentosManager.Pages.Turmas
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly TeamsService _teamsService;
        private readonly EmailService _emailService;

        public CreateModel(ApplicationDbContext context, TeamsService teamsService, EmailService emailService)
        {
            _context = context;
            _teamsService = teamsService;
            _emailService = emailService;
        }

        public IActionResult OnGet()
        {
            ViewData["InstrutorId"] = new SelectList(_context.Instrutores, "Id", "Nome");
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nome");
            ViewData["SoftwareId"] = new SelectList(_context.Softwares, "Id", "Nome");
            return Page();
        }

        [BindProperty]
        public Turma Turma { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ViewData["InstrutorId"] = new SelectList(_context.Instrutores, "Id", "Nome");
                ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nome");
                ViewData["SoftwareId"] = new SelectList(_context.Softwares, "Id", "Nome");
                return Page();
            }

            // Criar reunião no Teams
            Turma.TeamsMeetingUrl = await _teamsService.CriarReuniaoTeams(Turma);

            _context.Turmas.Add(Turma);
            await _context.SaveChangesAsync();

            // Enviar email para instrutor
            if (Turma.Instrutor != null)
            {
                await _emailService.EnviarEmailInstrutor(Turma);
            }

            return RedirectToPage("./Index");
        }
    }
}