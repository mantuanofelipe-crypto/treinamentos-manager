using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Softwares
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Software Software { get; set; } = default!;

        public int TotalTurmas { get; set; }
        public int TotalCertificacoes { get; set; }
        public string? Erro { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var software = await _context.Softwares
                .Include(s => s.Turmas)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (software == null)
                return NotFound();

            Software = software;
            await CarregarDependencias(id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var software = await _context.Softwares
                .Include(s => s.Turmas)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (software == null)
                return NotFound();

            Software = software;
            await CarregarDependencias(id);

            if (TotalTurmas > 0 || TotalCertificacoes > 0)
                return Page();

            _context.Softwares.Remove(software);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        private async Task CarregarDependencias(int softwareId)
        {
            TotalTurmas = Software.Turmas.Count;
            TotalCertificacoes =
                await _context.InstrutorACPs.CountAsync(a => a.SoftwareId == softwareId) +
                await _context.InstrutorProficiencias.CountAsync(p => p.SoftwareId == softwareId);

            if (TotalTurmas > 0 || TotalCertificacoes > 0)
                Erro = "Este software possui turmas ou certificações vinculadas. Remova esses vínculos antes de excluir.";
        }
    }
}
