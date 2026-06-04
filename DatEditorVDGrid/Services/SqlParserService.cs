using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DatEditorVDGrid.Services
{
    internal class SqlParserService
    {
        public static void SplitSelectFrom(string fullSql, out string selectPart, out string fromPart)
        {
            selectPart = fullSql;
            fromPart = "";

            if (string.IsNullOrEmpty(fullSql))
                return;

            int parenLevel = 0;
            bool inString = false;
            int length = fullSql.Length;

            for (int i = 0; i < length; i++)
            {
                char c = fullSql[i];

                if (c == '\'')
                {
                    if (inString)
                    {
                        if (i + 1 < length && fullSql[i + 1] == '\'')
                        {
                            i++; // skip escaped quote
                            continue;
                        }
                        else
                        {
                            inString = false;
                        }
                    }
                    else
                    {
                        inString = true;
                    }
                }

                if (!inString)
                {
                    if (c == '(') parenLevel++;
                    if (c == ')') parenLevel--;

                    // Look for "FROM" at parenLevel == 0
                    if (parenLevel == 0 && i + 4 <= length)
                    {
                        if (string.Compare(fullSql, i, "FROM", 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            bool prevOk = i == 0 || char.IsWhiteSpace(fullSql[i - 1]);
                            bool nextOk = i + 4 == length || char.IsWhiteSpace(fullSql[i + 4]);

                            if (prevOk && nextOk)
                            {
                                selectPart = fullSql.Substring(0, i);
                                fromPart = fullSql.Substring(i).Trim();
                                return;
                            }
                        }
                    }
                }
            }
        }

        public static List<string> SplitSqlColumns(string input)
        {
            var result = new List<string>();
            int parenLevel = 0;
            int bracketLevel = 0;
            bool inString = false;
            var current = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '\'')
                {
                    if (inString)
                    {
                        if (i + 1 < input.Length && input[i + 1] == '\'')
                        {
                            current.Append('\'');
                            current.Append('\'');
                            i++; // skip escaped quote
                            continue;
                        }
                        else
                        {
                            inString = false;
                        }
                    }
                    else
                    {
                        inString = true;
                    }
                }

                if (!inString)
                {
                    if (c == '(') parenLevel++;
                    if (c == ')') parenLevel--;
                    if (c == '[') bracketLevel++;
                    if (c == ']') bracketLevel--;
                }

                if (c == ',' && parenLevel == 0 && bracketLevel == 0 && !inString)
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

            string normalized = sql.Trim();
            int length = normalized.Length;
            int parenLevel = 0;
            bool inString = false;

            int selectStart = -1;
            int fromStart = -1;
            int whereStart = -1;
            int orderStart = -1;

            // First, find the indices of the keywords at the outer level (parenLevel == 0)
            for (int i = 0; i < length; i++)
            {
                char c = normalized[i];

                if (c == '\'')
                {
                    if (inString)
                    {
                        if (i + 1 < length && normalized[i + 1] == '\'')
                        {
                            i++; // skip escaped quote
                            continue;
                        }
                        else
                        {
                            inString = false;
                        }
                    }
                    else
                    {
                        inString = true;
                    }
                }

                if (!inString)
                {
                    if (c == '(') parenLevel++;
                    if (c == ')') parenLevel--;

                    if (parenLevel == 0)
                    {
                        // Check SELECT
                        if (selectStart == -1 && i + 6 <= length && string.Compare(normalized, i, "SELECT", 0, 6, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (i == 0 || char.IsWhiteSpace(normalized[i - 1]))
                            {
                                if (i + 6 == length || char.IsWhiteSpace(normalized[i + 6]))
                                {
                                    selectStart = i;
                                }
                            }
                        }
                        // Check FROM
                        else if (fromStart == -1 && i + 4 <= length && string.Compare(normalized, i, "FROM", 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (i == 0 || char.IsWhiteSpace(normalized[i - 1]))
                            {
                                if (i + 4 == length || char.IsWhiteSpace(normalized[i + 4]))
                                {
                                    fromStart = i;
                                }
                            }
                        }
                        // Check WHERE
                        else if (whereStart == -1 && i + 5 <= length && string.Compare(normalized, i, "WHERE", 0, 5, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (i == 0 || char.IsWhiteSpace(normalized[i - 1]))
                            {
                                if (i + 5 == length || char.IsWhiteSpace(normalized[i + 5]))
                                {
                                    whereStart = i;
                                }
                            }
                        }
                        // Check ORDER BY
                        else if (orderStart == -1 && i + 8 <= length && string.Compare(normalized, i, "ORDER BY", 0, 8, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (i == 0 || char.IsWhiteSpace(normalized[i - 1]))
                            {
                                if (i + 8 == length || char.IsWhiteSpace(normalized[i + 8]))
                                {
                                    orderStart = i;
                                }
                            }
                        }
                    }
                }
            }

            // Now slice the parts
            if (selectStart >= 0)
            {
                int start = selectStart + 6;
                int end = (fromStart >= 0) ? fromStart :
                          (whereStart >= 0) ? whereStart :
                          (orderStart >= 0) ? orderStart : length;
                string selectText = normalized.Substring(start, end - start).Trim();
                selectFields = SplitSqlColumns(selectText);
            }

            if (fromStart >= 0)
            {
                int start = fromStart;
                int end = (whereStart >= 0) ? whereStart :
                          (orderStart >= 0) ? orderStart : length;
                fromPart = normalized.Substring(start, end - start).Trim();
            }

            if (whereStart >= 0)
            {
                int start = whereStart + 5;
                int end = (orderStart >= 0) ? orderStart : length;
                wherePart = normalized.Substring(start, end - start).Trim();
            }

            if (orderStart >= 0)
            {
                int start = orderStart + 8;
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
