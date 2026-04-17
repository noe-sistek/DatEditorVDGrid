using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatEditorVDGrid.Helpers
{
    internal class DatOptions
    {
        public string GlobalEllipsisColsToShow { get; set; } = "";
        public string FullEllipsisColsToAsign { get; set; } = "";
        public string FullEllipsisColsToAlias { get; set; } = "";

        public readonly Dictionary<string, string> BehaviorProps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, string> FormattingProps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, string> MaskedProps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, string> EllipsisExtraProps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, string> SubTotalsProps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public void Reset()
        {
            GlobalEllipsisColsToShow = "";
            FullEllipsisColsToAsign = "";
            FullEllipsisColsToAlias = "";

            BehaviorProps.Clear();
            FormattingProps.Clear();
            MaskedProps.Clear();
            EllipsisExtraProps.Clear();
            SubTotalsProps.Clear();
        }

        // Helper: strip table alias prefix if present (e.g. "d.Cantidad" => "Cantidad")
        public static string StripTablePrefix(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;
            var idx = name.LastIndexOf('.');
            if (idx >= 0 && idx < name.Length - 1)
                return name.Substring(idx + 1);
            return name;
        }

        // Helper: return preserved value if present otherwise create a repeated "0" token list of length count using sep
        public static string PreserveOrRepeat(Dictionary<string, string> dict, string key, char sep, int count)
        {
            if (dict != null && dict.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val))
                return val;
            if (count <= 0)
                return "";
            var token = "0";
            return string.Join(sep.ToString(), Enumerable.Repeat(token, count));
        }
    }
}
