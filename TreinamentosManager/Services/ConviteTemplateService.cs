using System.Globalization;
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

        public string CriarHtmlEmail(Turma turma)
        {
            var dados = CriarDados(turma);

            return $"""
                <div style="margin:0;padding:0;background:#ececec;">
                  <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="border-collapse:collapse;background:#ececec;margin:0;padding:0;">
                    <tr>
                      <td align="center" style="padding:18px 10px;">
                        <table role="presentation" cellpadding="0" cellspacing="0" width="760" style="border-collapse:collapse;width:760px;max-width:100%;background:#ffffff;font-family:Arial,Helvetica,sans-serif;color:#555555;border:1px solid #dedede;">
                          <tr>
                            <td style="padding:28px 38px 8px 38px;background:#ffffff;">
                              <img src="cid:convite-logo" alt="Deskgraphics Consulting" width="310" style="display:block;width:310px;max-width:74%;height:auto;border:0;margin:0 auto 22px auto;">
                              <img src="cid:convite-apresenta" alt="Apresenta" width="220" style="display:block;width:220px;max-width:60%;height:auto;border:0;margin:0 auto;">
                            </td>
                          </tr>
                          <tr>
                            <td style="padding:22px 38px 10px 38px;background:#ffffff;">
                              <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="border-collapse:collapse;">
                                <tr>
                                  <td valign="top" width="64%" style="padding:0 28px 0 0;">
                                    <div style="font-size:34px;line-height:36px;font-weight:700;color:#565656;letter-spacing:0;text-transform:uppercase;margin:0 0 18px 0;">Convite<br>de treinamento</div>
                                    <div style="font-size:15px;line-height:23px;color:#555555;margin:0 0 18px 0;">
                                      Caro(a) aluno(a), você foi inscrito(a) para participar do treinamento de
                                      <strong style="color:#222222;">{Html(dados.Curso)}</strong>.
                                      Venha participar dessa jornada de conhecimento com a <strong>Deskgraphics</strong>.
                                      Para esclarecimento de dúvidas contatar:
                                      <a href="mailto:treinamento@deskgraphics.com.br" style="color:#5d5d5d;text-decoration:underline;">treinamento@deskgraphics.com.br</a>.
                                    </div>
                                  </td>
                                  <td valign="top" width="36%" style="padding:3px 0 0 0;">
                                    <img src="cid:convite-hero" alt="" width="245" style="display:block;width:245px;max-width:100%;height:auto;border:0;">
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                          <tr>
                            <td style="padding:2px 38px 0 38px;">
                              <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="border-collapse:collapse;">
                                <tr>
                                  <td style="background:#f2d600;height:10px;line-height:10px;font-size:1px;">&nbsp;</td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                          <tr>
                            <td style="padding:26px 38px 8px 38px;">
                              <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="border-collapse:collapse;">
                                <tr>
                                  <td valign="top" width="75%" style="padding:0 24px 0 0;">
                                    <div style="font-size:18px;line-height:22px;font-weight:700;color:#555555;text-transform:uppercase;margin:0 0 16px 0;">Treinamento ao vivo on-line</div>
                                    <div style="font-size:15px;line-height:22px;color:#555555;margin:0 0 14px 0;">
                                      <strong style="color:#111111;">{Html(dados.Instrutor)}</strong>
                                      <span style="color:#8a8a8a;">........................................</span>
                                      Instrutor Técnico Especialista
                                    </div>
                                    <div style="font-size:15px;line-height:22px;color:#555555;margin:0 0 14px 0;">
                                      Aulas gravadas disponíveis em até 24 horas na nossa plataforma de vídeos <strong>DeskHub</strong>.
                                      Acesse: <a href="https://deskhub.deskgraphics.com.br" style="color:#5d5d5d;text-decoration:underline;">https://deskhub.deskgraphics.com.br</a>
                                    </div>
                                  </td>
                                  <td valign="top" width="25%" align="right" style="padding:0;">
                                    <img src="cid:convite-agenda" alt="Agenda" width="80" style="display:block;width:80px;max-width:100%;height:auto;border:0;">
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                          <tr>
                            <td style="padding:6px 38px 0 38px;">
                              <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="border-collapse:collapse;background:#5c5c5c;">
                                <tr>
                                  <td style="padding:17px 20px;color:#ffffff;">
                                    <div style="font-size:14px;line-height:18px;font-weight:700;text-transform:uppercase;margin:0 0 8px 0;">Será transmitido pela plataforma Microsoft Teams</div>
                                    <div style="font-size:18px;line-height:24px;font-weight:700;margin:0;">{Html(dados.DiasSemana)} das {Html(dados.HoraInicio)} às {Html(dados.HoraFim)}</div>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                          <tr>
                            <td style="padding:22px 38px 12px 38px;">
                              <div style="font-size:18px;line-height:22px;font-weight:700;color:#565656;text-transform:uppercase;margin:0 0 10px 0;">Disponibilidade das gravações</div>
                              <div style="font-size:15px;line-height:23px;color:#555555;margin:0 0 16px 0;">
                                Carga horária total do treinamento: <strong>{Html(dados.CargaHoraria)}</strong><br>
                                {Html(dados.DataInicio)} a {Html(dados.DataFim)}
                              </div>
                              <div style="font-size:14px;line-height:21px;color:#555555;margin:0 0 16px 0;">
                                <strong>Datas das aulas:</strong><br>
                                {CriarListaDatasHtml(dados.Datas, dados.InicioPadrao)}
                              </div>
                            </td>
                          </tr>
                          <tr>
                            <td style="padding:6px 38px 28px 38px;">
                              <img src="cid:convite-autodesk" alt="Autodesk Authorized Training Center" width="690" style="display:block;width:100%;max-width:690px;height:auto;border:0;">
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </div>
                """;
        }

        public List<EmailAttachment> CriarImagensInlineEmail()
        {
            return new List<EmailAttachment>
            {
                CriarImagemInline("image1.jpg", "image/jpeg", "convite-hero"),
                CriarImagemInline("image2.png", "image/png", "convite-logo"),
                CriarImagemInline("image3.png", "image/png", "convite-autodesk"),
                CriarImagemInline("image4.png", "image/png", "convite-apresenta"),
                CriarImagemInline("image5.png", "image/png", "convite-agenda")
            };
        }

        private static Dictionary<string, string> CriarSubstituicoes(Turma turma)
        {
            var dados = CriarDados(turma);

            return new Dictionary<string, string>
            {
                ["<Nome do curso>"] = dados.Curso,
                ["<Instrutor>"] = dados.Instrutor,
                ["<Dias da semana>"] = dados.DiasSemana,
                ["<Hora início>"] = dados.HoraInicio,
                ["<Hora inicio>"] = dados.HoraInicio,
                ["<Hora in\u00edcio>"] = dados.HoraInicio,
                ["<Hora fim>"] = dados.HoraFim,
                ["<Carga Horária>"] = dados.CargaHoraria,
                ["<Carga Horaria>"] = dados.CargaHoraria,
                ["<Carga Hor\u00e1ria>"] = dados.CargaHoraria,
                ["<Data inicio>"] = dados.DataInicio,
                ["<Data início>"] = dados.DataInicio,
                ["<Data fim>"] = dados.DataFim
            };
        }

        private static ConviteDados CriarDados(Turma turma)
        {
            var datas = turma.Datas.OrderBy(d => d.Data).ToList();
            var primeiroEncontro = datas.FirstOrDefault();
            var primeiraData = NormalizarHorario(primeiroEncontro?.Data ?? turma.Inicio, turma.Inicio);
            var primeiraFim = primeiroEncontro != null
                ? primeiraData.AddHours((double)primeiroEncontro.DuracaoHoras)
                : turma.Fim;
            var ultimaData = datas.LastOrDefault()?.Data ?? turma.Fim;

            return new ConviteDados
            {
                Curso = turma.Software?.Nome ?? "",
                Instrutor = turma.Instrutor?.Nome ?? "",
                DiasSemana = !string.IsNullOrWhiteSpace(turma.DiasDaSemana) ? turma.DiasDaSemana : CriarDiasDaSemana(datas),
                HoraInicio = primeiraData.ToString("HH:mm"),
                HoraFim = primeiraFim.ToString("HH:mm"),
                CargaHoraria = $"{turma.CargaHoraria}h",
                DataInicio = primeiraData.ToString("dd/MM/yyyy"),
                DataFim = ultimaData.ToString("dd/MM/yyyy"),
                Datas = datas,
                InicioPadrao = turma.Inicio
            };
        }

        private static string CriarDiasDaSemana(List<TurmaData> datas)
        {
            if (!datas.Any())
                return "";

            var cultura = CultureInfo.GetCultureInfo("pt-BR");
            return string.Join(", ", datas
                .Select(d => cultura.DateTimeFormat.GetDayName(d.Data.DayOfWeek))
                .Distinct());
        }

        private static string EscapeXmlText(string value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private EmailAttachment CriarImagemInline(string fileName, string contentType, string contentId)
        {
            var path = Path.Combine(_environment.ContentRootPath, "wwwroot", "images", "convite-template", fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException("Imagem do convite não encontrada.", path);

            return new EmailAttachment
            {
                FileName = fileName,
                ContentType = contentType,
                ContentId = contentId,
                Content = File.ReadAllBytes(path)
            };
        }

        private static string CriarListaDatasHtml(List<TurmaData> datas, DateTime inicioPadrao)
        {
            if (!datas.Any())
                return "- Datas a confirmar";

            var cultura = CultureInfo.GetCultureInfo("pt-BR");
            return string.Join("<br>", datas.Select(d =>
            {
                var inicio = NormalizarHorario(d.Data, inicioPadrao);
                var fim = inicio.AddHours((double)d.DuracaoHoras);
                return $"{Html(inicio.ToString("dd/MM/yyyy HH:mm"))} às {Html(fim.ToString("HH:mm"))} ({Html(d.DuracaoHoras.ToString("0.##", cultura))}h)";
            }));
        }

        private static string Html(string? value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static DateTime NormalizarHorario(DateTime data, DateTime inicioPadrao)
        {
            if (data.TimeOfDay == TimeSpan.Zero && inicioPadrao.TimeOfDay != TimeSpan.Zero)
                return data.Date.Add(inicioPadrao.TimeOfDay);

            return data;
        }

        private static string LimparNomeArquivo(string value)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '-');

            return value.Trim();
        }

        private class ConviteDados
        {
            public string Curso { get; set; } = string.Empty;
            public string Instrutor { get; set; } = string.Empty;
            public string DiasSemana { get; set; } = string.Empty;
            public string HoraInicio { get; set; } = string.Empty;
            public string HoraFim { get; set; } = string.Empty;
            public string CargaHoraria { get; set; } = string.Empty;
            public string DataInicio { get; set; } = string.Empty;
            public string DataFim { get; set; } = string.Empty;
            public List<TurmaData> Datas { get; set; } = new();
            public DateTime InicioPadrao { get; set; }
        }
    }
}
