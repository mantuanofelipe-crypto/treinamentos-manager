using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages.Conformidade
{
    public class RelatorioModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RelatorioModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public decimal MediaGeral { get; set; }
        public decimal MediaInstrutor { get; set; }
        public decimal MediaCoordenacao { get; set; }
        public int TotalTurmasComConformidade { get; set; }
        public List<string> Labels { get; set; } = new();
        public List<decimal> Geral { get; set; } = new();
        public List<decimal> Instrutor { get; set; } = new();
        public List<decimal> Coordenacao { get; set; } = new();

        public async Task OnGetAsync()
        {
            var turmas = await _context.Turmas
                .Include(t => t.ConformidadeDetalhes)
                .Where(t => t.ConformidadeDetalhes != null)
                .OrderBy(t => t.Inicio)
                .ToListAsync();

            var conformidades = turmas
                .Select(t => t.ConformidadeDetalhes!)
                .Where(c => c.ConformidadeGeral.HasValue)
                .ToList();

            TotalTurmasComConformidade = conformidades.Count;
            MediaGeral = Media(conformidades.Select(c => c.ConformidadeGeral));
            MediaInstrutor = Media(conformidades.Select(c => c.ConformidadeInstrutor));
            MediaCoordenacao = Media(conformidades.Select(c => c.ConformidadeCoordenacao));

            var grupos = turmas
                .Where(t => t.ConformidadeDetalhes?.ConformidadeGeral.HasValue == true)
                .GroupBy(t => new DateTime(t.Inicio.Year, t.Inicio.Month, 1))
                .OrderBy(g => g.Key);

            foreach (var grupo in grupos)
            {
                var itens = grupo.Select(t => t.ConformidadeDetalhes!).ToList();
                Labels.Add(grupo.Key.ToString("MMM/yy"));
                Geral.Add(Media(itens.Select(i => i.ConformidadeGeral)));
                Instrutor.Add(Media(itens.Select(i => i.ConformidadeInstrutor)));
                Coordenacao.Add(Media(itens.Select(i => i.ConformidadeCoordenacao)));
            }
        }

        private static decimal Media(IEnumerable<decimal?> valores)
        {
            var validos = valores
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();

            return validos.Any()
                ? Math.Round(validos.Average(), 1)
                : 0m;
        }
    }
}
