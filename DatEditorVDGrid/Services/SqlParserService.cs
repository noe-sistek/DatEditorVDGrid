using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatEditorVDGrid.Services
{
    internal class SqlParserService
    {
        public static void SplitSelectFrom(string fullSql, out string selectPart, out string fromPart)
        {
            selectPart = fullSql;
            fromPart = "";

            var lower = (fullSql ?? "").ToLower();
            int idx = lower.IndexOf(" from ");
            if (idx >= 0)
            {
                selectPart = fullSql.Substring(0, idx);
                fromPart = fullSql.Substring(idx + 1).Trim();
            }
        }

        public static List<string> SplitSqlColumns(string input)
        {
            var result = new List<string>();
            int parenLevel = 0;
            var current = new StringBuilder();

            foreach (char c in input)
            {
                if (c == '(') parenLevel++;
                if (c == ')') parenLevel--;

                if (c == ',' && parenLevel == 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
                result.Add(current.ToString());

            return result;
        }

        public static string ExtractMainTableFromFromPart(string fromPart)
        {
            var lower = (fromPart ?? "").ToLower();
            int idxFrom = lower.IndexOf("from");
            string tail = (idxFrom >= 0) ? fromPart.Substring(idxFrom + 4).Trim() : (fromPart ?? "").Trim();

            string[] terminators = new[] { " where ", " order ", " group ", " having ", " limit " };
            int endIdx = -1;

            foreach (var t in terminators)
            {
                int p = tail.ToLower().IndexOf(t);
                if (p >= 0)
                {
                    if (endIdx < 0 || p < endIdx)
                        endIdx = p;
                }
            }

            if (endIdx >= 0)
                tail = tail.Substring(0, endIdx).Trim();

            var tokens = tail.Split(new[] { ' ', '\t', '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
                return "";

            return tokens[0];
        }

        public static string ExtractMainTableAliasFromFromPart(string fromPart)
        {
            if (string.IsNullOrWhiteSpace(fromPart))
                return "";

            var lower = fromPart.ToLower();
            int idxFrom = lower.IndexOf("from");
            string tail = (idxFrom >= 0) ? fromPart.Substring(idxFrom + 4).Trim() : fromPart.Trim();

            string[] terminators = new[] { " where ", " order ", " group ", " having ", " limit " };
            int endIdx = -1;

            foreach (var t in terminators)
            {
                int p = tail.ToLower().IndexOf(t);
                if (p >= 0)
                {
                    if (endIdx < 0 || p < endIdx) endIdx = p;
                }
            }

            if (endIdx >= 0)
                tail = tail.Substring(0, endIdx).Trim();

            var tokens = tail.Split(new[] { ' ', '\t', '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 2)
                return "";

            var candidate = tokens[1];

            string[] reserved = new[] { "inner", "left", "right", "full", "outer", "join", "on", "as" };

            if (reserved.Contains(candidate.ToLower()))
                return "";

            return candidate;
        }
    }
}
