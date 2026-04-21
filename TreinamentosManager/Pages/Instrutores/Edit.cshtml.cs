using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Instrutores
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Instrutor Instrutor { get; set; } = default!;

        public List<Software> Softwares { get; set; } = new();
        public List<InstrutorProficiencia> Proficiencias { get; set; } = new();
        public List<InstrutorACP> ACPs { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var instrutor = await _context.Instrutores
                .Include(i => i.Proficiencias)
                .Include(i => i.ACPs)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instrutor == null) return NotFound();

            Instrutor = instrutor;
            Softwares = await _context.Softwares.OrderBy(s => s.Nome).ToListAsync();
            Proficiencias = instrutor.Proficiencias.ToList();
            ACPs = instrutor.ACPs.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(
            List<int> ProficienciasSoftwareIds, List<int> ProficienciasEstrelas,
            List<int> ACPsSoftwareIds, List<string?> ACPsDatas)
        {
            if (!ModelState.IsValid) return Page();

            var instrutor = await _context.Instrutores
                .Include(i => i.Proficiencias)
                .Include(i => i.ACPs)
                .FirstOrDefaultAsync(i => i.Id == Instrutor.Id);

            if (instrutor == null) return NotFound();

            instrutor.Nome = Instrutor.Nome;
            instrutor.Email = Instrutor.Email;
            instrutor.Ativo = Instrutor.Ativo;
            instrutor.DataExpiracaoACI = Instrutor.DataExpiracaoACI;

            // Proficiências
            _context.InstrutorProficiencias.RemoveRange(instrutor.Proficiencias);
            for (int i = 0; i < ProficienciasSoftwareIds.Count; i++)
            {
                if (i < ProficienciasEstrelas.Count && ProficienciasEstrelas[i] > 0)
                {
                    _context.InstrutorProficiencias.Add(new InstrutorProficiencia
                    {
                        InstrutorId = instrutor.Id,
                        SoftwareId = ProficienciasSoftwareIds[i],
                        Estrelas = ProficienciasEstrelas[i]
                    });
                }
            }

            // ACPs
            _context.InstrutorACPs.RemoveRange(instrutor.ACPs);
            for (int i = 0; i < ACPsSoftwareIds.Count; i++)
            {
                if (i < ACPsDatas.Count && !string.IsNullOrEmpty(ACPsDatas[i]) && DateTime.TryParse(ACPsDatas[i], out var data))
                {
                    _context.InstrutorACPs.Add(new InstrutorACP
                    {
                        InstrutorId = instrutor.Id,
                        SoftwareId = ACPsSoftwareIds[i],
                        DataExpiracao = data
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
