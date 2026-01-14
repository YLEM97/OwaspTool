using OwaspTool.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.ComponentModel;

namespace OwaspTool.Services
{

    public class RequirementsPdfGeneratorService : IRequirementsPdfGeneratorService
    {
        public byte[] CreatePdfV2(Dictionary<ChapterDTO, Dictionary<SectionDTO, List<RequirementDTO>>> groupedRequirements, string applicationName)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(x => x.FontSize(11));

                    // █████ HEADER (solo prima pagina)
                    page.Header().Element(ComposeHeader);

                    void ComposeHeader(QuestPDF.Infrastructure.IContainer container)
                    {
                        container
                            .ShowOnce()                 // header solo nella prima pagina
                            .Column(col =>
                            {
                                // LOGO opzionale
                                // col.Item().AlignCenter().Image("wwwroot/img/logo.png", ImageScaling.FitWidth, 60);

                                col.Item().Text($"OWASP ASVS Requirements – {applicationName}")
                                    .FontSize(18)
                                    .Bold()
                                    .AlignCenter();

                                col.Item().PaddingBottom(10);

                                col.Item()
                                    .LineHorizontal(1)
                                    .LineColor(Colors.Grey.Medium);
                            });
                    }

                    // █████ CONTENUTO (tutto il documento)
                    page.Content().Column(col =>
                    {
                        foreach (var chapter in groupedRequirements.OrderBy(c => c.Key.ChapterID))
                        {
                            col.Item().PaddingBottom(10)
                                .Text($"{chapter.Key.Number} – {chapter.Key.Title}")
                                .FontSize(15)
                                .Bold()
                                .FontColor(Colors.Blue.Medium);

                            foreach (var section in chapter.Value.OrderBy(s => s.Key.SectionID))
                            {
                                col.Item().PaddingLeft(10)
                                    .PaddingBottom(4)
                                    .Text($"{section.Key.Number} – {section.Key.Title}")
                                    .FontSize(13)
                                    .SemiBold()
                                    .FontColor(Colors.Grey.Darken2);

                                // Requirements con padding
                                foreach (var req in section.Value.OrderBy(r => r.ASVSRequirementID))
                                {
                                    col.Item().PaddingLeft(18).Column(reqCol =>
                                    {
                                        // testo
                                        reqCol.Item()
                                            .PaddingVertical(6)
                                            .Text(text =>
                                            {
                                                text.Span(req.Number + ": ").Bold();
                                                text.Span(req.Text);
                                            });

                                        // linea con padding sopra
                                        reqCol.Item()
                                            .PaddingTop(6)
                                            .LineHorizontal(1)
                                            .LineColor(Colors.Grey.Lighten2);
                                    });
                                }

                                col.Item().PaddingBottom(10);
                            }

                            col.Item().PageBreak(); // Nuova pagina dopo ogni Chapter
                        }
                    });

                    // █████ FOOTER (tutte le pagine)
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("Generated on ");
                            text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                        });

                        row.ConstantItem(120).AlignRight().Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
        public byte[] CreatePdf(List<RequirementDTO> requirements, string applicationName)
        {
            var ordered = requirements
                .OrderBy(r => r.ChapterID)
                .ThenBy(r => r.SectionID)
                .ThenBy(r => r.ASVSRequirementID)
                .ToList();

            var document = Document.Create(headercontainer =>
            {
                headercontainer.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));

                    page.Header().Element(ComposeHeader);

                    void ComposeHeader(QuestPDF.Infrastructure.IContainer container)
                    {
                        container.ShowOnce().Column(col =>
                        {
                            col.Item().Text($"OWASP ASVS Requirements – {applicationName}")
                                .FontSize(18)
                                .Bold()
                                .AlignCenter();
                            col.Item().PaddingBottom(10);
                        });
                    }

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn();   
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Number").Bold();
                            header.Cell().Text("Requirement").Bold();
                        });

                        foreach (var req in ordered)
                        {
                            table.Cell().Text(req.Number ?? "N/A");
                            table.Cell().Text(req.Text ?? "N/A");
                        }
                    });
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on ");
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
