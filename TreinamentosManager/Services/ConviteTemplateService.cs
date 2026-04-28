using System.IO.Compression;
using System.Net;
using System.Text;
using TreinamentosManager.Models;

namespace TreinamentosManager.Services
{
    public class ConviteTemplateService
    {
        private readonly IWebHostEnvironment _environment;

        public ConviteTemplateService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public byte[] GerarConvitePptx(Turma turma)
        {
            var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "ConviteTreinamento.pptx");
            if (!File.Exists(templatePath))
                throw new FileNotFoundException("Template de convite não encontrado.", templatePath);

            using var output = new MemoryStream();
            using (var template = File.OpenRead(templatePath))
            {
                template.CopyTo(output);
            }

            output.Position = 0;

            using (var archive = new ZipArchive(output, ZipArchiveMode.Update, leaveOpen: true))
            {
                var replacements = CriarSubstituicoes(turma);
                var slideEntries = archive.Entries
                    .Where(e => e.FullName.StartsWith("ppt/slides/slide", StringComparison.OrdinalIgnoreCase) &&
                                e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var entry in slideEntries)
                {
                    string xml;
                    using (var reader = new StreamReader(entry.Open(), Encoding.UTF8))
                    {
                        xml = reader.ReadToEnd();
                    }

                    foreach (var item in replacements)
                    {
                        xml = xml.Replace(item.Key, EscapeXmlText(item.Value));
                        xml = xml.Replace(WebUtility.HtmlEncode(item.Key), EscapeXmlText(item.Value));
                    }

                    entry.Delete();
                    var newEntry = archive.CreateEntry(entry.FullName);
                    using var writer = new StreamWriter(newEntry.Open(), Encoding.UTF8);
                    writer.Write(xml);
                }
            }

            return output.ToArray();
        }

        public string CriarNomeArquivo(Turma turma)
        {
            var curso = turma.Software?.Nome ?? "Treinamento";
            var cliente = turma.Cliente?.Nome ?? "Cliente";
            return $"Convite - {LimparNomeArquivo(curso)} - {LimparNomeArquivo(cliente)}.pptx";
        }

        private static Dictionary<string, string> CriarSubstituicoes(Turma turma)
        {
            var datas = turma.Datas.OrderBy(d => d.Data).ToList();
            var primeiraData = datas.FirstOrDefault()?.Data ?? turma.Inicio;
            var primeiraFim = datas.FirstOrDefault()?.Fim ?? turma.Fim;
            var ultimaData = datas.LastOrDefault()?.Data ?? turma.Fim;

            return new Dictionary<string, string>
            {
                ["<Nome do curso>"] = turma.Software?.Nome ?? "",
                ["<Instrutor>"] = turma.Instrutor?.Nome ?? "",
                ["<Dias da semana>"] = turma.DiasDaSemana ?? CriarDiasDaSemana(datas),
                ["<Hora início>"] = primeiraData.ToString("HH:mm"),
                ["<Hora fim>"] = primeiraFim.ToString("HH:mm"),
                ["<Carga Horária>"] = $"{turma.CargaHoraria}h",
                ["<Data inicio>"] = primeiraData.ToString("dd/MM/yyyy"),
                ["<Data fim>"] = ultimaData.ToString("dd/MM/yyyy")
            };
        }

        private static string CriarDiasDaSemana(List<TurmaData> datas)
        {
            if (!datas.Any())
                return "";

            var cultura = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
            return string.Join(", ", datas
                .Select(d => cultura.DateTimeFormat.GetDayName(d.Data.DayOfWeek))
                .Distinct());
        }

        private static string EscapeXmlText(string value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string LimparNomeArquivo(string value)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '-');

            return value.Trim();
        }
    }
}
