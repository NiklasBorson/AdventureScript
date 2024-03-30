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
        public static void AddCommandParagraph(string commandString, RichTextBlock control)
        {
            var para = new Paragraph();
            SetCommandStyle(para);
            para.Inlines.Add(new Run { Text = commandString });
            control.Blocks.Add(para);
        }

        public static void AddContent(string folderPath, IList<string> inputLines, RichTextBlock control)
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
                        AddSpacingParagraph(control, para);
                    }
                    continue;
                }
                else if (inCodeBlock)
                {
                    SetCodeStyle(para);
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

                    while (textPos < input.Length)
                    {
                        textPos = ParseParaContent(folderPath, para.Inlines, input, textPos, '\0');
                    }
                }

                control.Blocks.Add(para);
            }
        }

        static FontFamily m_monoFont = new FontFamily("Consolas");

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

        static int ParseParaContent(string folderPath, InlineCollection inlines, string input, int textPos, char endDelim)
        {
            char ch = input[textPos];

            // Is it a link?
            if (ch == '[')
            {
                // We expect something of the form [link](url)
                // Recursively parse the link part.
                var link = new Hyperlink();
                int linkEndPos = ParseParaContent(folderPath, link.Inlines, input, textPos + 1, ']');

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

                    inlines.Add(link);
                    return urlEndPos + 1;
                }
            }

            // Not a link. Is it an image?
            if (ch == '!' && textPos + 1 < input.Length && input[textPos + 1] == '[')
            {
                // We expect something of the form: ![alt](url)
                int altStartPos = textPos + 2;
                int altEndPos = input.IndexOf(']', altStartPos);
                if (altEndPos > 0 && altEndPos + 1 < input.Length && input[altEndPos + 1] == '(')
                {
                    int urlStartPos = altEndPos + 2;
                    int urlEndPos = input.IndexOf(')', urlStartPos);
                    if (urlEndPos > urlStartPos)
                    {
                        string url = input.Substring(urlStartPos, urlEndPos - urlStartPos);

                        var path = Path.Combine(folderPath, url);

                        var bitmapImage = new BitmapImage();
                        bitmapImage.UriSource = new System.Uri(path);
                        var image = new Microsoft.UI.Xaml.Controls.Image
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Stretch = Stretch.None,
                            Source = bitmapImage
                        };

                        inlines.Add(new InlineUIContainer { Child = image });

                        return urlEndPos + 1;
                    }
                }
            }

            // Scan ahead for end of plain text.
            int endPos = input.IndexOfAny(
                new char[] { '[', '!', endDelim },
                textPos + 1
                );

            if (endPos < 0)
            {
                endPos = input.Length;
            }

            // Add a run for the plain text.
            if (textPos < endPos)
            {
                inlines.Add(new Run { Text = input.Substring(textPos, endPos - textPos) });
            }

            return endPos;
        }
    }
}
