using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Turmas
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Turma Turma { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var turma = await _context.Turmas.FindAsync(id);
            if (turma == null) return NotFound();

            Turma = turma;
            CarregarViewData();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CarregarViewData();
                return Page();
            }

            var turma = await _context.Turmas.FindAsync(Turma.Id);
            if (turma == null) return NotFound();

            turma.IdAutodesk = Turma.IdAutodesk;
            turma.CargaHoraria = Turma.CargaHoraria;
            turma.Modalidade = Turma.Modalidade;
            turma.ClienteId = Turma.ClienteId;
            turma.SoftwareId = Turma.SoftwareId;
            turma.Inicio = Turma.Inicio;
            turma.Fim = Turma.Fim;
            turma.DiasDaSemana = Turma.DiasDaSemana;
            turma.InstrutorId = string.IsNullOrEmpty(Turma.InstrutorId) ? null : Turma.InstrutorId;

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        private void CarregarViewData()
        {
            ViewData["InstrutorId"] = new SelectList(_context.Instrutores.Where(i => i.Ativo).OrderBy(i => i.Nome), "Id", "Nome");
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Ativo).OrderBy(c => c.Nome), "Id", "Nome");
            ViewData["SoftwareId"] = new SelectList(_context.Softwares.OrderBy(s => s.Nome), "Id", "Nome");
        }
    }
}
