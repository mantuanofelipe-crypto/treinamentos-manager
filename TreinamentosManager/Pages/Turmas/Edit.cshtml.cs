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

        [BindProperty]
        public List<DateTime?> DatasTurma { get; set; } = new();

        [BindProperty]
        public List<decimal?> DuracoesTurma { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var turma = await _context.Turmas
                .Include(t => t.Datas)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turma == null)
                return NotFound();

            Turma = turma;
            var datas = turma.Datas.OrderBy(d => d.Data).ToList();
            DatasTurma = datas.Select(d => (DateTime?)d.Data).ToList();
            DuracoesTurma = datas.Select(d => (decimal?)d.DuracaoHoras).ToList();
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

            var turma = await _context.Turmas
                .Include(t => t.Datas)
                .FirstOrDefaultAsync(t => t.Id == Turma.Id);

            if (turma == null)
                return NotFound();

            turma.IdAutodesk = Turma.IdAutodesk;
            turma.Nome = Turma.Nome;
            turma.CargaHoraria = Turma.CargaHoraria;
            turma.Modalidade = Turma.Modalidade;
            turma.TipoCliente = Turma.TipoCliente;
            turma.Conformidade = Turma.Conformidade;
            turma.ClienteId = Turma.ClienteId;
            turma.SoftwareId = Turma.SoftwareId;
            turma.Inicio = Turma.Inicio;
            turma.Fim = Turma.Fim;
            turma.DiasDaSemana = Turma.DiasDaSemana;
            turma.InstrutorId = string.IsNullOrEmpty(Turma.InstrutorId) ? null : Turma.InstrutorId;

            var encontrosAtualizados = encontros.Select(encontro =>
            {
                var existente = turma.Datas.FirstOrDefault(data =>
                    data.Data == encontro.Data &&
                    data.DuracaoHoras == encontro.DuracaoHoras);

                if (existente == null)
                    return encontro;

                encontro.TeamsMeetingId = existente.TeamsMeetingId;
                encontro.TeamsMeetingUrl = existente.TeamsMeetingUrl;
                return encontro;
            }).ToList();

            _context.TurmaDatas.RemoveRange(turma.Datas);
            turma.Datas = encontrosAtualizados;

            await _context.SaveChangesAsync();

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
