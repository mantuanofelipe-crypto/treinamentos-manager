using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TreinamentosManager.Models;

namespace TreinamentosManager.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _context;

    public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public int TurmasAtivas { get; set; }
    public int TurmasEmAndamento { get; set; }
    public int TurmasFuturas { get; set; }
    public int TotalClientes { get; set; }
    public decimal PrevisaoAtual { get; set; }
    public decimal PrevisaoFutura { get; set; }

    public List<string> GraficoLabels { get; set; } = new();
    public List<decimal> GraficoHistorico { get; set; } = new();
    public List<decimal?> GraficoTendencia { get; set; } = new();
    public List<decimal> GraficoPresencial { get; set; } = new();
    public List<decimal?> GraficoPresencialTendencia { get; set; } = new();
    public List<decimal> GraficoOnline { get; set; } = new();
    public List<decimal?> GraficoOnlineTendencia { get; set; } = new();
    public List<decimal> GraficoGerenciavel { get; set; } = new();
    public List<decimal?> GraficoGerenciavelTendencia { get; set; } = new();
    public List<decimal> GraficoAbertas { get; set; } = new();
    public List<decimal?> GraficoAbertasTendencia { get; set; } = new();

    public async Task OnGetAsync()
    {
        var now = DateTime.Now;
        var turmas = await _context.Turmas
            .Include(t => t.Datas)
            .ToListAsync();

        TurmasAtivas = turmas.Count(t => t.Fim < now);
        TurmasEmAndamento = turmas.Count(t => t.Inicio <= now && t.Fim >= now);
        TurmasFuturas = turmas.Count(t => t.Inicio > now);
        TotalClientes = await _context.Clientes.CountAsync();

        PrevisaoAtual = CalcularAlocacaoMes(turmas, now.Year, now.Month);

        var proximo = now.AddMonths(1);
        PrevisaoFutura = CalcularAlocacaoMes(turmas, proximo.Year, proximo.Month);

        var mesAtual = new DateTime(now.Year, now.Month, 1);
        var inicio = mesAtual.AddMonths(-6);
        var fim = mesAtual.AddMonths(3);

        for (var m = inicio; m <= fim; m = m.AddMonths(1))
        {
            var mes = m;
            var isFuturo = mes > mesAtual;

            var carga = CalcularAlocacaoMes(turmas, mes.Year, mes.Month);
            var presencial = CalcularAlocacaoMes(turmas.Where(t => t.Modalidade == "Presencial"), mes.Year, mes.Month);
            var online = CalcularAlocacaoMes(turmas.Where(t => t.Modalidade == "Online"), mes.Year, mes.Month);
            var gerenciavel = CalcularAlocacaoMes(turmas.Where(t => t.TipoCliente == "Turmas Gerenciável"), mes.Year, mes.Month);
            var abertas = CalcularAlocacaoMes(turmas.Where(t => t.TipoCliente == "Turmas Abertas"), mes.Year, mes.Month);

            GraficoLabels.Add(mes.ToString("MMM/yy"));

            GraficoHistorico.Add(isFuturo ? 0 : carga);
            GraficoTendencia.Add(isFuturo ? carga : (decimal?)null);
            GraficoPresencial.Add(isFuturo ? 0 : presencial);
            GraficoPresencialTendencia.Add(isFuturo ? presencial : (decimal?)null);
            GraficoOnline.Add(isFuturo ? 0 : online);
            GraficoOnlineTendencia.Add(isFuturo ? online : (decimal?)null);
            GraficoGerenciavel.Add(isFuturo ? 0 : gerenciavel);
            GraficoGerenciavelTendencia.Add(isFuturo ? gerenciavel : (decimal?)null);
            GraficoAbertas.Add(isFuturo ? 0 : abertas);
            GraficoAbertasTendencia.Add(isFuturo ? abertas : (decimal?)null);
        }

        var pivot = GraficoLabels.Count - 4;
        if (pivot >= 0 && pivot < GraficoLabels.Count)
        {
            GraficoTendencia[pivot] = GraficoHistorico[pivot];
            GraficoPresencialTendencia[pivot] = GraficoPresencial[pivot];
            GraficoOnlineTendencia[pivot] = GraficoOnline[pivot];
            GraficoGerenciavelTendencia[pivot] = GraficoGerenciavel[pivot];
            GraficoAbertasTendencia[pivot] = GraficoAbertas[pivot];
        }
    }

    private static decimal CalcularAlocacaoMes(IEnumerable<Turma> turmas, int ano, int mes)
    {
        return turmas.Sum(t =>
        {
            if (t.Datas.Any())
            {
                return t.Datas
                    .Where(d => d.Data.Year == ano && d.Data.Month == mes)
                    .Sum(d => d.DuracaoHoras);
            }

            return t.Inicio.Year == ano && t.Inicio.Month == mes
                ? t.CargaHoraria
                : 0m;
        });
    }
}
