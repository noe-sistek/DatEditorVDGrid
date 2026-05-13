using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public static void ParseFullQuery(string sql, out List<string> selectFields, out string fromPart, out string wherePart, out string orderPart)
        {
            selectFields = new List<string>();
            fromPart = "";
            wherePart = "";
            orderPart = "";

            if (string.IsNullOrWhiteSpace(sql)) return;

            // Normalize spaces and remove newlines for easier regex/index matching
            string normalized = Regex.Replace(sql, @"\s+", " ").Trim();

            int selectIdx = normalized.IndexOf("SELECT ", StringComparison.OrdinalIgnoreCase);
            int fromIdx = normalized.IndexOf(" FROM ", StringComparison.OrdinalIgnoreCase);
            int whereIdx = normalized.IndexOf(" WHERE ", StringComparison.OrdinalIgnoreCase);
            int orderIdx = normalized.IndexOf(" ORDER BY ", StringComparison.OrdinalIgnoreCase);

            // 1. SELECT
            if (selectIdx >= 0)
            {
                int start = selectIdx + 7;
                int end = (fromIdx >= 0) ? fromIdx : 
                          (whereIdx >= 0) ? whereIdx : 
                          (orderIdx >= 0) ? orderIdx : normalized.Length;
                
                string selectText = normalized.Substring(start, end - start).Trim();
                selectFields = SplitSqlColumns(selectText);
            }

            // 2. FROM
            if (fromIdx >= 0)
            {
                int start = fromIdx + 6;
                int end = (whereIdx >= 0) ? whereIdx : 
                          (orderIdx >= 0) ? orderIdx : normalized.Length;
                
                fromPart = "FROM " + normalized.Substring(start, end - start).Trim();
            }

            // 3. WHERE
            if (whereIdx >= 0)
            {
                int start = whereIdx + 7;
                int end = (orderIdx >= 0) ? orderIdx : normalized.Length;
                
                wherePart = normalized.Substring(start, end - start).Trim();
            }

            // 4. ORDER BY
            if (orderIdx >= 0)
            {
                int start = orderIdx + 10;
                orderPart = normalized.Substring(start).Trim();
            }
        }

        public static void ParseFieldNameAndAlias(string fieldRaw, out string name, out string alias)
        {
            fieldRaw = fieldRaw.Trim();
            name = fieldRaw;
            alias = "";

            // Try to find " AS " (case insensitive)
            var match = Regex.Match(fieldRaw, @"^(?<name>.+)\s+AS\s+(?<alias>[^\s]+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                name = match.Groups["name"].Value.Trim();
                alias = match.Groups["alias"].Value.Trim();
                return;
            }

            // Try to find a space that separates name and alias (if no AS)
            // Be careful not to split inside function calls func(...) alias
            // We search from the end for the last space
            int lastSpace = fieldRaw.LastIndexOf(' ');
            if (lastSpace > 0)
            {
                // Check if the last space is outside parentheses
                int parenLevel = 0;
                bool insideParen = false;
                for (int i = 0; i < lastSpace; i++)
                {
                    if (fieldRaw[i] == '(') parenLevel++;
                    if (fieldRaw[i] == ')') parenLevel--;
                }

                if (parenLevel == 0)
                {
                    string candidateName = fieldRaw.Substring(0, lastSpace).Trim();
                    string candidateAlias = fieldRaw.Substring(lastSpace + 1).Trim();

                    // Basic check: alias shouldn't contain dots or parentheses
                    if (!candidateAlias.Contains(".") && !candidateAlias.Contains("("))
                    {
                        name = candidateName;
                        alias = candidateAlias;
                    }
                }
            }
        }
    }
}
