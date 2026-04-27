using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatEditorVDGrid.UIHelpers
{
    internal class RichTextHelper
    {
        public static void ApplySqlKeywordHighlight(
            RichTextBox rtb,
            string[] sqlKeywords,
            Color joinColor,
            Color keywordColor,
            ref bool suppressEvent,
            bool uppercaseKeywords = true)
        {
            if (rtb == null) return;

            int origSelStart = rtb.SelectionStart;
            int origSelLen = rtb.SelectionLength;

            string originalText = rtb.Text ?? "";
            string processedText = originalText;

            if (uppercaseKeywords)
            {
                foreach (var kw in sqlKeywords)
                {
                    processedText = Regex.Replace(
                        processedText,
                        $@"\b{Regex.Escape(kw)}\b",
                        kw,
                        RegexOptions.IgnoreCase);
                }
            }

            bool textChanged = !string.Equals(originalText, processedText, StringComparison.Ordinal);

            if (textChanged)
            {
                suppressEvent = true;
                try
                {
                    rtb.Text = processedText;
                }
                finally
                {
                    suppressEvent = false;
                }
            }

            Color normalColor = rtb.ForeColor;
            string textToScan = rtb.Text ?? "";

            foreach (var kw in sqlKeywords)
            {
                string pattern = $@"\b{Regex.Escape(kw)}\b";

                foreach (Match m in Regex.Matches(textToScan, pattern, RegexOptions.IgnoreCase))
                {
                    rtb.Select(m.Index, m.Length);

                    if (kw.Equals("ON", StringComparison.OrdinalIgnoreCase) ||
                        kw.Equals("AS", StringComparison.OrdinalIgnoreCase))
                        rtb.SelectionColor = keywordColor;
                    else
                        rtb.SelectionColor = joinColor;
                }
            }

            if (origSelStart >= 0 && origSelStart <= rtb.Text.Length)
            {
                rtb.SelectionStart = origSelStart;
                rtb.SelectionLength = origSelLen;
            }
            else
            {
                rtb.SelectionStart = rtb.Text.Length;
                rtb.SelectionLength = 0;
            }

            rtb.SelectionColor = normalColor;
            rtb.DeselectAll();
        }

        public static void ApplyDatSyntaxHighlighting(RichTextBox rtb)
        {
            if (rtb == null) return;

            int origSelStart = rtb.SelectionStart;
            int origSelLen = rtb.SelectionLength;

            Color[] colors = new Color[] {
                ColorTranslator.FromHtml("#FF5555"),
                ColorTranslator.FromHtml("#39FF14"),
                ColorTranslator.FromHtml("#00A2FF"),
                ColorTranslator.FromHtml("#FFE600"),
                ColorTranslator.FromHtml("#FF00FF"),
                ColorTranslator.FromHtml("#00FFFF"),
                ColorTranslator.FromHtml("#FF8C00"),
                ColorTranslator.FromHtml("#BA68FF"),
                ColorTranslator.FromHtml("#A3FF00"),
                ColorTranslator.FromHtml("#FF69B4")
            };

            // Remove previous formatting
            rtb.SelectAll();
            rtb.SelectionColor = rtb.ForeColor;
            rtb.SelectionFont = new Font(rtb.Font, FontStyle.Regular);

            string text = rtb.Text;
            int currentIndex = 0;

            string[] lines = text.Split('\n');
            
            var propertyCounters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            
            foreach (string line in lines)
            {
                string cleanLine = line.TrimEnd('\r');
                if (cleanLine.StartsWith("[") && cleanLine.EndsWith("]"))
                {
                    rtb.Select(currentIndex, cleanLine.Length);
                    rtb.SelectionFont = new Font(rtb.Font, FontStyle.Bold);
                }
                else
                {
                    int equalsIndex = cleanLine.IndexOf('=');
                    if (equalsIndex >= 0)
                    {
                        string fullKey = cleanLine.Substring(0, equalsIndex).Trim();
                        string baseKey = Regex.Replace(fullKey, @"\d+$", "");
                        
                        if (!propertyCounters.ContainsKey(baseKey))
                            propertyCounters[baseKey] = 0;
                            
                        int colorIndex = propertyCounters[baseKey];
                        
                        int startValueIndex = currentIndex + equalsIndex + 1;
                        string valuesPart = cleanLine.Substring(equalsIndex + 1);
                        
                        int currentElementStart = startValueIndex;
                        
                        for (int i = 0; i < valuesPart.Length; i++)
                        {
                            char c = valuesPart[i];
                            if (c == ',' || c == '~')
                            {
                                int length = (startValueIndex + i) - currentElementStart;
                                if (length > 0)
                                {
                                    rtb.Select(currentElementStart, length);
                                    rtb.SelectionColor = colors[colorIndex % 10];
                                }
                                
                                colorIndex++;
                                currentElementStart = startValueIndex + i + 1;
                            }
                        }
                        
                        int lastLength = (startValueIndex + valuesPart.Length) - currentElementStart;
                        if (lastLength > 0)
                        {
                            rtb.Select(currentElementStart, lastLength);
                            rtb.SelectionColor = colors[colorIndex % 10];
                        }
                        colorIndex++;
                        
                        propertyCounters[baseKey] = colorIndex;
                    }
                }
                
                currentIndex += line.Length + 1;
            }

            if (origSelStart >= 0 && origSelStart <= rtb.Text.Length)
            {
                rtb.SelectionStart = origSelStart;
                rtb.SelectionLength = origSelLen;
            }
            else
            {
                rtb.SelectionStart = rtb.Text.Length;
                rtb.SelectionLength = 0;
            }

            rtb.SelectionColor = rtb.ForeColor;
            rtb.SelectionFont = rtb.Font;
            rtb.DeselectAll();
        }

        public static void HighlightSeparators(
            RichTextBox rtb,
            char[] separators,
            Color sepColor)
        {
            if (rtb == null) return;

            int origSelStart = rtb.SelectionStart;
            int origSelLen = rtb.SelectionLength;

            string text = rtb.Text ?? "";

            for (int i = 0; i < text.Length; i++)
            {
                if (separators.Contains(text[i]))
                {
                    rtb.Select(i, 1);
                    rtb.SelectionColor = sepColor;
                }
            }

            if (origSelStart >= 0 && origSelStart <= rtb.Text.Length)
            {
                rtb.SelectionStart = origSelStart;
                rtb.SelectionLength = origSelLen;
            }
            else
            {
                rtb.SelectionStart = rtb.Text.Length;
                rtb.SelectionLength = 0;
            }

            rtb.SelectionColor = rtb.ForeColor;
            rtb.DeselectAll();
        }
    }
}
