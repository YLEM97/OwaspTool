using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwaspTool.DTOs;
using Microsoft.AspNetCore.Hosting;

namespace OwaspTool.Services
{
    public class TestPdfGeneratorService : ITestsPdfGeneratorService
    {
        private readonly MarkdownPipeline _mdPipeline;
        private readonly bool _isDevelopment;

        public TestPdfGeneratorService(IWebHostEnvironment env)
        {
            _mdPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            _isDevelopment = env?.EnvironmentName == Environments.Development || env?.IsDevelopment() == true;
        }

        public byte[] CreatePdf(Dictionary<WSTGChapterDTO, List<WSTGTestDTO>> groupedTests, string applicationName)
        {
            var now = DateTime.UtcNow;

            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // Header (solo prima pagina)
                    page.Header().Element(ComposeHeader);

                    void ComposeHeader(QuestPDF.Infrastructure.IContainer headerContainer)
                    {
                        headerContainer
                            .ShowOnce()
                            .Column(col =>
                            {
                                col.Item().Text($"OWASP WSTG Tests – {applicationName}")
                                    .FontSize(18)
                                    .Bold()
                                    .AlignCenter();

                                col.Item().PaddingBottom(10);

                                col.Item()
                                    .LineHorizontal(1)
                                    .LineColor(Colors.Grey.Medium);
                            });
                    }

                    // Content
                    page.Content().Column(col =>
                    {
                        if (groupedTests == null || !groupedTests.Any())
                        {
                            col.Item().Text("No WSTG tests available for this application.")
                                .Italic()
                                .FontColor(Colors.Grey.Darken1);
                            return;
                        }

                        foreach (var chapterEntry in groupedTests.OrderBy(g => g.Key.WSTGChapterID))
                        {
                            var chapter = chapterEntry.Key;
                            var tests = chapterEntry.Value;

                            // Chapter title
                            col.Item().PaddingBottom(10)
                                .Text($"{chapter.Number} – {chapter.Title}")
                                .FontSize(15)
                                .Bold()
                                .FontColor(Colors.Blue.Medium);

                            // Test count subtitle
                            col.Item().PaddingLeft(10)
                                .PaddingBottom(8)
                                .Text($"{tests.Count} {(tests.Count == 1 ? "test" : "tests")}")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken1);

                            // Tests
                            foreach (var t in tests.OrderBy(t => t.WSTGTestID))
                            {
                                col.Item().PaddingLeft(18).Column(testCol =>
                                {
                                    // Title + id
                                    testCol.Item()
                                        .PaddingVertical(6)
                                        .Text(text =>
                                        {
                                            text.Span($"{t.NumberWSTG ?? t.Number}").Bold().FontSize(12);
                                            text.Span($" — {t.Title}").Bold().FontSize(12);
                                        });

                                    // Text (rendered from markdown)
                                    if (!string.IsNullOrWhiteSpace(t.Text))
                                    {
                                        testCol.Item().PaddingTop(4).Element(container =>
                                        {
                                            RenderMarkdownContent(container, t.Text);
                                        });
                                    }

                                    // Link (if present)
                                    if (!string.IsNullOrWhiteSpace(t.Link))
                                    {
                                        testCol.Item()
                                               .PaddingTop(4)
                                               .Hyperlink(t.Link)
                                               .Text("Documentation Link")
                                               .FontSize(9)
                                               .FontColor(Colors.Blue.Darken1);

                                    }

                                    // Status badge
                                    var statusText = GetStatusText(t.TestStatus);
                                    var bgColor = GetStatusBackgroundColor(t.TestStatus);
                                    testCol.Item()
                                        .PaddingTop(6)
                                        .Element(container =>
                                        {
                                            container
                                                .Background(bgColor)
                                                .CornerRadius(6)
                                                .Padding(6)
                                                .Row(rw =>
                                                {
                                                    rw.RelativeItem().Text(txt =>
                                                    {
                                                        txt.Span("Status: ").SemiBold().FontSize(10);
                                                        txt.Span(statusText).FontSize(10).FontColor(Colors.Grey.Darken2);
                                                    });
                                                });
                                        });

                                    // Notes (if present)
                                    if (!string.IsNullOrWhiteSpace(t.Notes))
                                    {
                                        testCol.Item()
                                            .PaddingTop(6)
                                            .Element(container =>
                                            {
                                                container
                                                    .Background(Colors.Grey.Lighten3)
                                                    .CornerRadius(6)
                                                    .Padding(8)
                                                    .Column(ncol =>
                                                    {
                                                        ncol.Item().Text("Notes:").SemiBold().FontSize(10);
                                                        ncol.Item().PaddingTop(4).Element(c =>
                                                        {
                                                            RenderMarkdownContent(c, t.Notes);
                                                        });
                                                    });
                                            });
                                    }

                                    // AI Notes (if present and in development)
                                    if (_isDevelopment && !string.IsNullOrWhiteSpace(t.AiNotes))
                                    {
                                        testCol.Item()
                                            .PaddingTop(8)
                                            .Element(container =>
                                            {
                                                RenderAiConversation(container, t.AiNotes);
                                            });
                                    }

                                    // Separator line
                                    testCol.Item()
                                        .PaddingTop(8)
                                        .LineHorizontal(1)
                                        .LineColor(Colors.Grey.Lighten2);
                                });
                            }

                            col.Item().PaddingBottom(10);
                            col.Item().PageBreak(); // Nuova pagina dopo ogni Chapter
                        }
                    });

                    // Footer
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("Generated on ");
                            text.Span(now.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                        });

                        row.ConstantItem(120).AlignRight().Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                    });
                });
            }).GeneratePdf();

            return bytes;
        }

        // Renderizza markdown completo (paragrafi, liste, tabelle, codice)
        private void RenderMarkdownContent(QuestPDF.Infrastructure.IContainer container, string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return;

            const float textFontSize = 10f;
            var textColor = Colors.Grey.Darken2;

            var document = Markdown.Parse(markdown, _mdPipeline);

            container.Column(col =>
            {
                foreach (var block in document)
                {
                    // Paragrafo
                    if (block is ParagraphBlock p)
                    {
                        col.Item().PaddingTop(2).Text(text =>
                        {
                            RenderInlineContent(text, p.Inline, textFontSize, textColor);
                        });
                    }
                    // Liste
                    else if (block is ListBlock lb)
                    {
                        RenderList(col, lb, textFontSize, textColor);
                    }
                    // Tabelle
                    else if (block is Markdig.Extensions.Tables.Table table)
                    {
                        col.Item().PaddingTop(6).Element(c =>
                        {
                            RenderTable(c, table);
                        });
                    }
                    // Code fence
                    else if (block is FencedCodeBlock fcb)
                    {
                        var codeBuilder = new StringBuilder();
                        foreach (var line in fcb.Lines.Lines)
                        {
                            codeBuilder.AppendLine(line.Slice.ToString());
                        }
                        var codeText = codeBuilder.ToString().TrimEnd();

                        col.Item().PaddingTop(6).Element(c =>
                        {
                            c.Background(Colors.Grey.Lighten3)
                             .CornerRadius(6)
                             .Padding(6)
                             .Text(codeText)
                             .FontFamily("Courier New")
                             .FontSize(9)
                             .FontColor(Colors.Black);
                        });
                    }
                    // Quote
                    else if (block is QuoteBlock qb)
                    {
                        col.Item().PaddingTop(6).Element(c =>
                        {
                            c.PaddingLeft(8)
                             .BorderLeft(Colors.Blue.Lighten1)
                             .PaddingLeft(8)
                             .Column(qcol =>
                             {
                                 foreach (var qchild in qb)
                                 {
                                     if (qchild is ParagraphBlock qpara)
                                     {
                                         qcol.Item().Text(t =>
                                         {
                                             t.DefaultTextStyle(s => s.Italic());
                                             RenderInlineContent(t, qpara.Inline, textFontSize, textColor, isItalic: true);
                                         });
                                     }
                                 }
                             });
                        });
                    }
                    // Heading
                    else if (block is HeadingBlock hb)
                    {
                        var headingSize = hb.Level switch
                        {
                            1 => 14f,
                            2 => 13f,
                            3 => 12f,
                            _ => 11f
                        };

                        col.Item().PaddingTop(8).Text(text =>
                        {
                            text.DefaultTextStyle(s => s.Bold().FontSize(headingSize).FontColor(Colors.Grey.Darken3));
                            RenderInlineContent(text, hb.Inline, headingSize, Colors.Grey.Darken3, isBold: true);
                        });
                    }
                }
            });
        }

        // Renderizza una lista (ordinata o non ordinata)
        private void RenderList(ColumnDescriptor col, ListBlock listBlock, float fontSize, Color color, int level = 0)
        {
            var indent = level * 16;
            var bullet = listBlock.IsOrdered ? "" : "•";
            var counter = 1;

            foreach (ListItemBlock item in listBlock)
            {
                var prefix = listBlock.IsOrdered ? $"{counter}." : bullet;

                col.Item().PaddingTop(2).PaddingLeft(indent).Row(row =>
                {
                    row.ConstantItem(20).Text(prefix).FontSize(fontSize).FontColor(color);
                    row.RelativeItem().Column(itemCol =>
                    {
                        foreach (var sub in item)
                        {
                            if (sub is ParagraphBlock subPara)
                            {
                                itemCol.Item().Text(t =>
                                {
                                    RenderInlineContent(t, subPara.Inline, fontSize, color);
                                });
                            }
                            else if (sub is ListBlock nestedList)
                            {
                                RenderList(itemCol, nestedList, fontSize, color, level + 1);
                            }
                        }
                    });
                });

                counter++;
            }
        }

        // Renderizza una tabella Markdown
        private void RenderTable(QuestPDF.Infrastructure.IContainer container, Markdig.Extensions.Tables.Table table)
        {
            container.Border(1, Colors.Grey.Lighten1).Table(questTable =>
            {
                // Calcola numero di colonne
                var columnCount = 0;
                if (table.FirstOrDefault() is Markdig.Extensions.Tables.TableRow firstRow)
                {
                    columnCount = firstRow.Count;
                }

                if (columnCount == 0) return;

                // Definizione colonne (tutte relative)
                questTable.ColumnsDefinition(columns =>
                {
                    for (int i = 0; i < columnCount; i++)
                    {
                        columns.RelativeColumn();
                    }
                });

                bool isFirstRow = true;

                foreach (var row in table.OfType<Markdig.Extensions.Tables.TableRow>())
                {
                    foreach (var cell in row.OfType<Markdig.Extensions.Tables.TableCell>())
                    {
                        questTable.Cell().Element(cellContainer =>
                        {
                            cellContainer
                                .Padding(4)
                                .Background(isFirstRow ? Colors.Grey.Lighten3 : Colors.White)
                                .Column(cellCol =>
                                {
                                    foreach (var cellBlock in cell)
                                    {
                                        if (cellBlock is ParagraphBlock cellPara)
                                        {
                                            cellCol.Item().Text(text =>
                                            {
                                                if (isFirstRow)
                                                    text.DefaultTextStyle(s => s.Bold());
                                                RenderInlineContent(text, cellPara.Inline, 9f, Colors.Grey.Darken2);
                                            });
                                        }
                                        else if (cellBlock is ListBlock cellList)
                                        {
                                            RenderList(cellCol, cellList, 9f, Colors.Grey.Darken2);
                                        }
                                    }
                                });
                        });
                    }

                    isFirstRow = false;
                }
            });
        }

        // Renderizza contenuto inline (bold, italic, code, link, ecc.)
        private void RenderInlineContent(TextDescriptor textDescriptor, ContainerInline containerInline, float fontSize, Color color, bool isBold = false, bool isItalic = false)
        {
            if (containerInline == null) return;

            for (var inline = containerInline.FirstChild; inline != null; inline = inline.NextSibling)
            {
                RenderInlineRecursive(textDescriptor, inline, fontSize, color, isBold, isItalic);
            }
        }

        private void RenderInlineRecursive(TextDescriptor textDescriptor, Inline inline, float fontSize, Color color, bool isBold = false, bool isItalic = false)
        {
            if (inline == null) return;

            switch (inline)
            {
                case LiteralInline lit:
                    {
                        var content = lit.Content.ToString();
                        var span = textDescriptor.Span(content).FontSize(fontSize).FontColor(color);
                        if (isBold) span.Bold();
                        if (isItalic) span.Italic();
                        break;
                    }
                case EmphasisInline em:
                    {
                        var newBold = isBold || em.DelimiterCount == 2;
                        var newItalic = isItalic || em.DelimiterCount == 1;
                        for (var child = em.FirstChild; child != null; child = child.NextSibling)
                            RenderInlineRecursive(textDescriptor, child, fontSize, color, newBold, newItalic);
                        break;
                    }
                case CodeInline ci:
                    {
                        if (!string.IsNullOrEmpty(ci.Content))
                        {
                            textDescriptor.Span(ci.Content)
                                .FontFamily("Courier New")
                                .FontSize(fontSize - 1)
                                .FontColor(Colors.Blue.Darken2);
                        }
                        break;
                    }
                case LineBreakInline:
                    {
                        textDescriptor.Span("\n").FontSize(fontSize).FontColor(color);
                        break;
                    }
                case LinkInline link:
                    {
                        for (var child = link.FirstChild; child != null; child = child.NextSibling)
                        {
                            if (child is LiteralInline li)
                            {
                                textDescriptor.Span(li.Content.ToString())
                                    .FontSize(fontSize)
                                    .FontColor(Colors.Blue.Darken2);
                            }
                            else
                            {
                                RenderInlineRecursive(textDescriptor, child, fontSize, Colors.Blue.Darken2, isBold, isItalic);
                            }
                        }
                        break;
                    }
                default:
                    {
                        if (inline is ContainerInline container)
                        {
                            for (var child = container.FirstChild; child != null; child = child.NextSibling)
                                RenderInlineRecursive(textDescriptor, child, fontSize, color, isBold, isItalic);
                        }
                        break;
                    }
            }
        }

        // Renderizza conversazione AI (solo development)
        private void RenderAiConversation(QuestPDF.Infrastructure.IContainer container, string aiNotes)
        {
            if (!_isDevelopment)
                return;

            const float noteFontSize = 10f;
            var noteColor = Colors.Grey.Darken2;
            var labelColor = Colors.Blue.Medium;
            var aiBackground = Colors.Blue.Lighten5;

            var document = Markdown.Parse(aiNotes ?? string.Empty, _mdPipeline);

            container
                .Background(aiBackground)
                .CornerRadius(8)
                .Padding(8)
                .Column(col =>
                {
                    col.Item().Text("AI Conversation:").SemiBold().FontSize(noteFontSize);

                    foreach (var block in document)
                    {
                        if (block is ParagraphBlock p)
                        {
                            col.Item().PaddingTop(6).Text(text =>
                            {
                                RenderParagraphWithLabel(text, p.Inline, noteFontSize, noteColor, labelColor);
                            });
                        }
                        else if (block is ListBlock lb)
                        {
                            RenderList(col, lb, noteFontSize, noteColor);
                        }
                        else if (block is FencedCodeBlock fcb)
                        {
                            var codeBuilder = new StringBuilder();
                            foreach (var line in fcb.Lines.Lines)
                            {
                                codeBuilder.AppendLine(line.Slice.ToString());
                            }
                            var codeText = codeBuilder.ToString().TrimEnd();

                            col.Item().PaddingTop(6).Element(c =>
                            {
                                c.Background(Colors.Grey.Lighten3)
                                 .CornerRadius(6)
                                 .Padding(6)
                                 .Text(codeText)
                                 .FontFamily("Courier New")
                                 .FontSize(9)
                                 .FontColor(Colors.Black);
                            });
                        }
                    }
                });
        }

        private void RenderParagraphWithLabel(TextDescriptor textDescriptor, ContainerInline containerInline, float fontSize, Color color, Color labelColor)
        {
            if (containerInline == null) return;

            var first = containerInline.FirstChild;
            bool handledFirstLiteral = false;

            if (first is LiteralInline lit)
            {
                var txt = lit.Content.ToString();
                var idx = txt.IndexOf(':');
                if (idx > 0)
                {
                    var prefix = txt.Substring(0, idx).Trim();
                    if (string.Equals(prefix, "You", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(prefix, "User", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(prefix, "AI", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(prefix, "Assistant", StringComparison.OrdinalIgnoreCase))
                    {
                        textDescriptor.Span(prefix + ": ").FontSize(fontSize).FontColor(labelColor);
                        var remainder = txt.Substring(idx + 1);
                        if (!string.IsNullOrWhiteSpace(remainder))
                        {
                            textDescriptor.Span(remainder).FontSize(fontSize).FontColor(color);
                        }
                        handledFirstLiteral = true;
                    }
                }
            }

            for (var inline = containerInline.FirstChild; inline != null; inline = inline.NextSibling)
            {
                if (handledFirstLiteral && inline is LiteralInline && inline == containerInline.FirstChild)
                    continue;

                RenderInlineRecursive(textDescriptor, inline, fontSize, color);
            }
        }

        private string GetStatusText(int? status)
        {
            if (!status.HasValue)
                return "Missing information";

            return status.Value switch
            {
                0 => "Not started",
                1 => "Pass",
                2 => "Issues",
                3 => "Not applicable",
                _ => "Unknown"
            };
        }

        private Color GetStatusBackgroundColor(int? status)
        {
            if (!status.HasValue)
                return Colors.Yellow.Lighten3; // Missing information -> giallo tenue

            return status.Value switch
            {
                0 => Colors.Grey.Lighten3,      // Not started -> grigio tenue
                1 => Colors.Green.Lighten3,     // Pass -> verde tenue
                2 => Colors.Red.Lighten3,       // Issues -> rosso tenue
                3 => Colors.Blue.Lighten3,      // Not applicable -> azzurro tenue
                _ => Colors.Grey.Lighten3
            };
        }
    }
}
