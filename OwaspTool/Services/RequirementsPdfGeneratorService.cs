using Humanizer;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using OwaspTool.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using System.Text;

namespace OwaspTool.Services
{
    public class RequirementsPdfGeneratorService : IRequirementsPdfGeneratorService
    {
        private readonly bool _isDevelopment;

        public RequirementsPdfGeneratorService(IWebHostEnvironment env)
        {
            _isDevelopment = env?.EnvironmentName == Environments.Development || env?.IsDevelopment() == true;
        }

        private static string GetImplementationStatusText(int? status)
        {
            if (!status.HasValue)
                return "Missing information";

            return status.Value switch
            {
                1 => "Implemented",
                0 => "Not implemented",
                2 => "Not applicable",
                _ => "Unknown"
            };
        }

        private static Color GetStatusBackgroundColor(int? status)
        {
            // colori pastello tenui
            if (!status.HasValue)
                return Colors.Yellow.Lighten3; // missing information -> giallo tenue

            return status.Value switch
            {
                1 => Colors.Green.Lighten3,  // implemented -> verde tenue
                0 => Colors.Red.Lighten3,    // not implemented -> rosso tenue
                2 => Colors.Orange.Lighten3, // not applicable -> arancione tenue
                _ => Colors.Grey.Lighten3
            };
        }

        // Renderizza il markdown di una singola conversazione usando Markdig + QuestPDF
        // INSTANCE method: rispetta _isDevelopment
        private void RenderAiConversation(QuestPDF.Infrastructure.IContainer container, string aiNotes)
        {
            if (!_isDevelopment)
                return; // non includere AI conversation in produzione

            // parametri di stile per le note (uguali alle note normali)
            const float noteFontSize = 10f;
            var noteColor = Colors.Grey.Darken2;
            var labelColor = Colors.Blue.Medium; // "You:" / "AI:" in azzurro

            // colore di sfondo chiaro azzurro per il riquadro
            var aiBackground = Colors.Blue.Lighten5;

            // pipeline standard Markdig
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var document = Markdown.Parse(aiNotes ?? string.Empty, pipeline);

            container
                .Background(aiBackground)
                .CornerRadius(8)
                .Padding(8)
                .Column(col =>
                {
                    // intestazione riquadro
                    col.Item().Text("AI Conversation:").SemiBold().FontSize(noteFontSize);

                    foreach (var block in document)
                    {
                        // Paragrafo (possibile "You:" / "AI:" all'inizio)
                        if (block is ParagraphBlock p)
                        {
                            col.Item().PaddingTop(6).Text(text =>
                            {
                                RenderParagraphWithOptionalLabel(text, p.Inline, noteFontSize, noteColor, labelColor);
                            });
                        }
                        // Liste
                        else if (block is ListBlock lb)
                        {
                            foreach (ListItemBlock item in lb)
                            {
                                // bullet + content
                                col.Item().PaddingTop(4).Row(row =>
                                {
                                    row.ConstantItem(14).Text("•").FontSize(noteFontSize).FontColor(noteColor);
                                    row.RelativeItem().Column(itemCol =>
                                    {
                                        foreach (var sub in item)
                                        {
                                            if (sub is ParagraphBlock subPara)
                                            {
                                                itemCol.Item().Text(t =>
                                                {
                                                    RenderParagraphWithOptionalLabel(t, subPara.Inline, noteFontSize, noteColor, labelColor);
                                                });
                                            }
                                            else
                                            {
                                                // fallback: render child block come testo semplice con stile note
                                                itemCol.Item().Text(sub.ToString()).FontSize(noteFontSize).FontColor(noteColor);
                                            }
                                        }
                                    });
                                });
                            }
                        }
                        // Code fence / blocco codice
                        else if (block is FencedCodeBlock fcb)
                        {
                            var codeBuilder = new StringBuilder();
                            foreach (var line in fcb.Lines.Lines)
                            {
                                var slice = line.Slice;
                                codeBuilder.AppendLine(slice.ToString());
                            }
                            var codeText = codeBuilder.ToString().TrimEnd();

                            col.Item().PaddingTop(6).Element(c =>
                            {
                                c.Background(Colors.Grey.Lighten3)
                                 .CornerRadius(6)
                                 .Padding(6)
                                 .Text(t =>
                                 {
                                     t.Span(codeText).FontFamily("Courier New").FontSize(9).FontColor(Colors.Black);
                                 });
                            });
                        }
                        // Quote / citazione
                        else if (block is QuoteBlock qb)
                        {
                            col.Item().PaddingTop(6).Element(c =>
                            {
                                c.PaddingLeft(8).Column(qcol =>
                                {
                                    foreach (var qchild in qb)
                                    {
                                        if (qchild is ParagraphBlock qpara)
                                        {
                                            qcol.Item().Text(t =>
                                            {
                                                // simbolo e contenuto in stile note
                                                t.Span("❝ ").FontSize(noteFontSize).FontColor(noteColor).Italic();
                                                RenderParagraphWithOptionalLabel(t, qpara.Inline, noteFontSize, noteColor, labelColor);
                                            });
                                        }
                                        else
                                        {
                                            qcol.Item().Text(qchild.ToString()).Italic();
                                        }
                                    }
                                });
                            });
                        }
                        else
                        {
                            // fallback generico: rendi ToString()
                            var raw = block.ToString();
                            if (!string.IsNullOrWhiteSpace(raw))
                                col.Item().PaddingTop(4).Text(raw).FontSize(noteFontSize).FontColor(noteColor);
                        }
                    }
                });
        }

        // Renderizza un paragrafo controllando se inizia con "You:" o "AI:" per colorare il label
        private static void RenderParagraphWithOptionalLabel(TextDescriptor textDescriptor, ContainerInline containerInline, float noteFontSize, Color noteColor, Color labelColor)
        {
            if (containerInline == null)
                return;

            var first = containerInline.FirstChild;
            bool handledFirstLiteralRemainder = false;

            if (first is LiteralInline lit)
            {
                var txt = lit.Content.ToString();
                var idx = txt.IndexOf(':');
                if (idx > 0)
                {
                    var prefix = txt.Substring(0, idx).Trim();
                    if (string.Equals(prefix, "You", System.StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(prefix, "User", System.StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(prefix, "AI", System.StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(prefix, "Assistant", System.StringComparison.OrdinalIgnoreCase))
                    {
                        // render label (azzurro) con i due punti
                        var label = txt.Substring(0, idx).Trim();
                        // label + colon and a space
                        textDescriptor.Span(label + ": ").FontSize(noteFontSize).FontColor(labelColor);

                        // render remainder of the first literal (after colon), se presente, con stile note
                        var remainder = txt.Substring(idx + 1);
                        if (!string.IsNullOrWhiteSpace(remainder))
                        {
                            textDescriptor.Span(remainder).FontSize(noteFontSize).FontColor(noteColor);
                        }

                        handledFirstLiteralRemainder = true;
                    }
                }
            }

            // render remaining inlines:
            for (var inline = containerInline.FirstChild; inline != null; inline = inline.NextSibling)
            {
                // if we already consumed part of the first literal, skip emitting it again:
                if (handledFirstLiteralRemainder && inline is LiteralInline && inline == containerInline.FirstChild)
                {
                    // we've printed the remainder; continue to next sibling
                    continue;
                }

                RenderInlineRecursiveWithNoteStyle(textDescriptor, inline, noteFontSize, noteColor);
            }
        }

        // Render ricorsivo delle inline applicando lo stile "note" per testo normale
        private static void RenderInlineRecursiveWithNoteStyle(TextDescriptor textDescriptor, Inline inline, float noteFontSize, Color noteColor, bool isBold = false, bool isItalic = false)
        {
            if (inline == null)
                return;

            switch (inline)
            {
                case LiteralInline lit:
                    {
                        var content = lit.Content.ToString();
                        var span = textDescriptor.Span(content).FontSize(noteFontSize).FontColor(noteColor);
                        if (isBold) span.Bold();
                        if (isItalic) span.Italic();
                        break;
                    }
                case EmphasisInline em:
                    {
                        var newBold = isBold || em.DelimiterCount == 2;
                        var newItalic = isItalic || em.DelimiterCount == 1;
                        for (var child = em.FirstChild; child != null; child = child.NextSibling)
                            RenderInlineRecursiveWithNoteStyle(textDescriptor, child, noteFontSize, noteColor, newBold, newItalic);
                        break;
                    }
                case CodeInline ci:
                    {
                        if (!string.IsNullOrEmpty(ci.Content))
                        {
                            // inline code: monospace, mantenendo dimensione simile (leggermente più piccola)
                            textDescriptor.Span(ci.Content).FontFamily("Courier New").FontSize(noteFontSize - 1).FontColor(Colors.Blue.Darken2);
                        }
                        break;
                    }
                case LineBreakInline lb:
                    {
                        textDescriptor.Span("\n").FontSize(noteFontSize).FontColor(noteColor);
                        break;
                    }
                case LinkInline link:
                    {
                        var buffer = new StringBuilder();
                        if (!link.IsImage)
                        {
                            for (var child = link.FirstChild; child != null; child = child.NextSibling)
                            {
                                if (child is LiteralInline li)
                                    buffer.Append(li.Content.ToString());
                                else
                                    RenderInlineRecursiveWithNoteStyle(textDescriptor, child, noteFontSize, noteColor, isBold, isItalic);
                            }

                            if (buffer.Length > 0)
                                textDescriptor.Span(buffer.ToString()).FontSize(noteFontSize).FontColor(Colors.Blue.Darken2);
                        }
                        else
                        {
                            textDescriptor.Span(link.Url ?? string.Empty).FontSize(noteFontSize).FontColor(noteColor);
                        }
                        break;
                    }
                default:
                    {
                        if (inline is ContainerInline container)
                        {
                            for (var child = container.FirstChild; child != null; child = child.NextSibling)
                                RenderInlineRecursiveWithNoteStyle(textDescriptor, child, noteFontSize, noteColor, isBold, isItalic);
                        }
                        else
                        {
                            var raw = inline.ToString();
                            if (!string.IsNullOrWhiteSpace(raw))
                                textDescriptor.Span(raw).FontSize(noteFontSize).FontColor(noteColor);
                        }
                        break;
                    }
            }
        }

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

                                        // status (mostrato subito sotto il testo)
                                        var statusText = GetImplementationStatusText(req.ImplementationStatus);
                                        var bgColor = GetStatusBackgroundColor(req.ImplementationStatus);
                                        reqCol.Item()
                                            .PaddingTop(2)
                                            .Element(container =>
                                            {
                                                container
                                                    .Background(bgColor)
                                                    .CornerRadius(6)
                                                    .Padding(6)
                                                    .Row(rw =>
                                                    {
                                                        rw.RelativeItem().Text(t =>
                                                        {
                                                            t.Span("Implementation status: ").SemiBold().FontSize(10);
                                                            t.Span(statusText).FontSize(10).FontColor(Colors.Grey.Darken2);
                                                        });
                                                    });
                                            });

                                        // note dell'utente (mostra solo se presente)
                                        if (!string.IsNullOrWhiteSpace(req.Notes))
                                        {
                                            reqCol.Item()
                                                .PaddingTop(6)
                                                .Element(container =>
                                                {
                                                    container
                                                        .Background(Colors.Grey.Lighten3) // sfondo tenue per la nota
                                                        .CornerRadius(6)
                                                        .Padding(8)
                                                        .Column(ncol =>
                                                        {
                                                            ncol.Item().Text("Note:").SemiBold().FontSize(10);
                                                            ncol.Item().PaddingTop(4)
                                                                .Text(req.Notes)
                                                                .FontSize(10)
                                                                .FontColor(Colors.Grey.Darken2);
                                                        });
                                                });
                                        }

                                        // AI conversation (mostra solo se siamo in sviluppo)
                                        if (_isDevelopment && !string.IsNullOrWhiteSpace(req.AiNotes))
                                        {
                                            reqCol.Item()
                                                .PaddingTop(8)
                                                .Element(container =>
                                                {
                                                    // Usa il helper per renderizzare la conversazione con background azzurro chiaro
                                                    RenderAiConversation(container, req.AiNotes);
                                                });
                                        }

                                        // linea con padding sopra
                                        reqCol.Item()
                                            .PaddingTop(8)
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
                            columns.ConstantColumn(140);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Number").Bold();
                            header.Cell().Text("Requirement").Bold();
                            header.Cell().Text("Status").Bold();
                        });

                        foreach (var req in ordered)
                        {
                            table.Cell().Text(req.Number ?? "N/A");
                            table.Cell().Column(col =>
                            {
                                col.Item().Text(req.Text ?? "N/A");
                                if (!string.IsNullOrWhiteSpace(req.Notes))
                                {
                                    col.Item().PaddingTop(4).Element(container =>
                                    {
                                        container.Background(Colors.Grey.Lighten3)
                                                 .CornerRadius(6)
                                                 .Padding(6)
                                                 .Text(t =>
                                                 {
                                                     t.Span("Note: ").SemiBold().FontSize(10).FontColor(Colors.Black);
                                                     t.Span(req.Notes).FontSize(10).FontColor(Colors.Grey.Darken2);
                                                 });
                                    });
                                }

                                if (_isDevelopment && !string.IsNullOrWhiteSpace(req.AiNotes))
                                {
                                    col.Item().PaddingTop(6).Element(container =>
                                    {
                                        RenderAiConversation(container, req.AiNotes);
                                    });
                                }
                            });

                            var statusText = GetImplementationStatusText(req.ImplementationStatus);
                            table.Cell().Text(statusText);
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

