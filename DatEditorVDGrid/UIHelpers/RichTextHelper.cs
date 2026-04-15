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
