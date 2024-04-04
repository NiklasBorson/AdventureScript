using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Collections.Generic;

namespace OxbowCastle
{
    static class MarkdownParser
    {
        static FontFamily m_monoFont = new FontFamily("Consolas");
        const float m_monoSize = 14;

        public static void AddCommandParagraph(string commandString, RichTextBlock control)
        {
            var para = new Paragraph();
            SetCommandStyle(para);
            para.Inlines.Add(new Run { Text = commandString });
            control.Blocks.Add(para);
        }

        public static void AddContent(GamePage page, IList<string> inputLines, RichTextBlock control)
        {
            bool inList = false;
            bool inCodeBlock = false;

            foreach (string input in inputLines)
            {
                var para = new Paragraph();
                int textPos = 0;

                if (input.StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;

                    if (inCodeBlock)
                    {
                        // Add vertical space before first code paragraph.
                        AddSpacingParagraph(control, para);
                    }
                    continue;
                }
                else if (inCodeBlock)
                {
                    SetCodeStyle(para);

                    // Ignore markup within a code paragraph.
                    para.Inlines.Add(new Run { Text = input });
                }
                else
                {
                    if (input.StartsWith("- "))
                    {
                        SetListStyle(para);
                        textPos = 2;

                        if (!inList)
                        {
                            // Add vertical space before first list item.
                            AddSpacingParagraph(control);
                            inList = true;
                        }
                    }
                    else
                    {
                        inList = false;

                        if (input.StartsWith("# "))
                        {
                            SetHeadingStyle(para);
                            textPos = 2;
                        }
                        else
                        {
                            SetBodyStyle(para);
                        }
                    }

                    ParseParaContent(page, para.Inlines, input, textPos, input.Length, '\0');
                }

                control.Blocks.Add(para);
            }
        }

        static void AddSpacingParagraph(RichTextBlock control)
        {
            AddSpacingParagraph(control, new Paragraph());
        }

        static void AddSpacingParagraph(RichTextBlock control, Paragraph para)
        {
            para.FontSize = 10;
            control.Blocks.Add(para);
        }

        static void SetCodeStyle(Paragraph para)
        {
            para.FontFamily = m_monoFont;
            para.FontSize = m_monoSize;
            para.Margin = new Thickness
            {
                Left = 20,
                Top = 0,
                Right = 0,
                Bottom = 0
            };
        }

        static void SetHeadingStyle(Paragraph para)
        {
            para.FontWeight = FontWeights.Bold;
            para.FontSize = 24;
            para.Margin = new Thickness
            {
                Left = 20,
                Top = 10,
                Right = 0,
                Bottom = 0
            };
        }

        static void SetBodyStyle(Paragraph para)
        {
            para.Margin = new Thickness
            {
                Left = 20,
                Top = 10,
                Right = 0,
                Bottom = 0
            };
        }

        static void SetListStyle(Paragraph para)
        {
            para.Inlines.Add(new Run { Text = " \x2022  " });

            para.Margin = new Thickness
            {
                Left = 30,
                Top = 0,
                Right = 0,
                Bottom = 0
            };
        }

        static void SetCommandStyle(Paragraph para)
        {
            para.FontWeight = FontWeights.Bold;
            para.Margin = new Thickness
            {
                Left = 0,
                Top = 20,
                Right = 0,
                Bottom = 0
            };
        }

        static void AddPlainText(InlineCollection inlines, string input, int textPos, int endPos)
        {
            if (textPos < endPos)
            {
                inlines.Add(new Run { Text = input.Substring(textPos, endPos - textPos) });
            }
        }

        static int ParseParaContent(
            GamePage page,
            InlineCollection inlines,
            string input,
            int textPos,
            int endPos,
            char endDelim
            )
        {
            for (int i = textPos; i < endPos; i++)
            {
                char ch = input[i];

                // Stop if we've reached the end delimiter.
                if (ch == endDelim)
                {
                    AddPlainText(inlines, input, textPos, i);
                    return i;
                }

                if (ch == '`')
                {
                    // Possibly inline code: `run`
                    int endIndex = input.IndexOf('`', i + 1);
                    if (endIndex >= 0)
                    {
                        // Add preceding text before the inline code.
                        AddPlainText(inlines, input, textPos, i);

                        // Add the inline code.
                        // Ignore any markup within the inline code.
                        inlines.Add(new Run
                        {
                            Text = input.Substring(i + 1, endIndex - i - 1),
                            FontFamily = m_monoFont,
                            FontSize = m_monoSize
                        });

                        // Advance past the inline code.
                        textPos = endIndex + 1;
                        i = textPos - 1;
                        continue;
                    }
                }
                else if (ch == '_')
                {
                    // Possibly italic text of the form: _ital_
                    if (i == 0 || !char.IsLetterOrDigit(input[i - 1]))
                    {
                        int endIndex = input.IndexOf('_', i + 1);
                        while (endIndex >= 0 && endIndex < input.Length && char.IsLetterOrDigit(input[endIndex]))
                        {
                            endIndex = input.IndexOf('_', endIndex + 1);
                        }
                        if (endIndex >= 0)
                        {
                            // Recursively parse the italicized content.
                            var ital = new Italic();
                            if (ParseParaContent(page, ital.Inlines, input, i + 1, endIndex, endDelim) == endIndex)
                            {
                                // Add the italicized text and preceding text.
                                AddPlainText(inlines, input, textPos, i);
                                inlines.Add(ital);

                                // Advance past the italicized text.
                                textPos = endIndex + 1;
                                i = textPos - 1;
                                continue;
                            }
                        }
                    }
                }
                else if (ch == '*')
                {
                    // Possibly bold text of the form: **bold**
                    if (i + 1 < input.Length && input[i + 1] == '*')
                    {
                        int endIndex = input.IndexOf("**", i + 2);
                        if (endIndex >= 0)
                        {
                            // Recursively parse the bold content.
                            var bold = new Bold();
                            if (ParseParaContent(page, bold.Inlines, input, i + 2, endIndex, endDelim) == endIndex)
                            {
                                // Add the bold text and preceding text.
                                AddPlainText(inlines, input, textPos, i);
                                inlines.Add(bold);

                                // Advance past the bold text.
                                textPos = endIndex + 2;
                                i = textPos - 1;
                                continue;
                            }
                        }
                    }
                }
                else if (ch == '[')
                {
                    // Possibly a link of the form: [content](url)
                    // Recursively parse the content part.
                    var link = new Hyperlink();
                    int linkEndPos = ParseParaContent(page, link.Inlines, input, i + 1, endPos, ']');

                    if (linkEndPos + 1 < input.Length &&
                        input[linkEndPos] == ']' &&
                        input[linkEndPos + 1] == '(')
                    {
                        int urlStartPos = linkEndPos + 2;
                        int urlEndPos = input.IndexOf(')', urlStartPos);
                        if (urlEndPos > urlStartPos)
                        {
                            string url = input.Substring(urlStartPos, urlEndPos - urlStartPos);
                            link.NavigateUri = new Uri(url);
                        }

                        // Add the link and any preceding text.
                        AddPlainText(inlines, input, textPos, i);
                        inlines.Add(link);

                        // Advance past the link.
                        textPos = urlEndPos + 1;
                        i = textPos - 1;
                        continue;
                    }
                }
                else if (ch == '!')
                {
                    // Possibly an image of the form: ![alt](href)
                    if (i + 1 < input.Length && input[i + 1] == '[')
                    {
                        int altStartPos = i + 2;
                        int altEndPos = input.IndexOf(']', altStartPos);
                        if (altEndPos > 0 && altEndPos + 1 < input.Length && input[altEndPos + 1] == '(')
                        {
                            int urlStartPos = altEndPos + 2;
                            int urlEndPos = input.IndexOf(')', urlStartPos);
                            if (urlEndPos > urlStartPos)
                            {
                                string href = input.Substring(urlStartPos, urlEndPos - urlStartPos);
                                var image = CreateImage(page, href);

                                // Add the image and any preceding text.
                                AddPlainText(inlines, input, textPos, i);
                                inlines.Add(new InlineUIContainer { Child = image });

                                // Advance past the image.
                                textPos = urlEndPos + 1;
                                i = textPos - 1;
                                continue;
                            }
                        }
                    }
                }
            }

            // We've reached the end position without finding any more markup.
            AddPlainText(inlines, input, textPos, endPos);
            return endPos;
        }

        static UIElement CreateImage(GamePage page, string href)
        {
            if (href.StartsWith('#'))
            {
                int id;
                if (int.TryParse(href.AsSpan(1), out id))
                {
                    var drawing = page.Game.GetDrawing(id);
                    if (drawing != null)
                    {
                        var sink = new DrawingSink(drawing.Width, drawing.Height);
                        drawing.Draw(sink);
                        return sink.Canvas;
                    }
                }
                return new Canvas();
            }
            else
            {
                var path = Path.Combine(page.FolderPath, href);

                var bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new System.Uri(path);
                var image = new Microsoft.UI.Xaml.Controls.Image
                {
                    Stretch = Stretch.None,
                    Source = bitmapImage
                };

                image.Loaded += (obj, args) => page.ScrollToEnd();
                return image;
            }
        }
    }
}
