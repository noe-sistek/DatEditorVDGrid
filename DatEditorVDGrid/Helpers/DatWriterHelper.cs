using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatEditorVDGrid.Helpers
{
    internal class DatWriterHelper
    {
        public static void TrimTrailingEmpty(List<string> list)
        {
            for (int k = list.Count - 1; k >= 0; k--)
            {
                if (string.IsNullOrWhiteSpace(list[k]))
                    list.RemoveAt(k);
                else
                    break;
            }
        }

        public static List<string> SplitSqlIntoLines(string sql, int maxChunkLength = 255)
        {
            var result = new List<string>();

            sql = (sql ?? "").Trim();

            while (sql.Length > maxChunkLength)
            {
                int searchIndex = Math.Min(maxChunkLength - 1, sql.Length - 1);

                int commaPos = sql.LastIndexOf(',', searchIndex);
                if (commaPos > 0)
                {
                    int len = commaPos + 1;
                    var chunk = sql.Substring(0, len).Trim();
                    result.Add(chunk);
                    sql = sql.Substring(len).Trim();
                    continue;
                }

                int spacePos = sql.LastIndexOf(' ', searchIndex);
                if (spacePos > 0)
                {
                    int len = spacePos;
                    var chunk = sql.Substring(0, len).Trim();
                    result.Add(chunk);
                    sql = sql.Substring(len).Trim();
                    continue;
                }

                int take = Math.Min(maxChunkLength, sql.Length);
                var forced = sql.Substring(0, take).Trim();
                result.Add(forced);
                sql = sql.Substring(take).Trim();
            }

            if (!string.IsNullOrWhiteSpace(sql))
                result.Add(sql);

            return result;
        }

        public static List<string> GetWrappedChunks(string key, string value)
        {
            value = value ?? "";
            int maxLine = 250;
            int prefixLength = key.Length + 1;
            int chunkMax = Math.Max(1, maxLine - prefixLength);
            return SplitSqlIntoLines(value, chunkMax);
        }

        public static void WriteNumberedKeys(StringBuilder sb, string baseKey, string value, int totalSlots)
        {
            var chunks = GetWrappedChunks(baseKey, value);
            int count = Math.Max(totalSlots, chunks.Count);

            for (int i = 0; i < count; i++)
            {
                string key = (i == 0) ? baseKey : $"{baseKey}{i + 1}";
                string val = (i < chunks.Count) ? chunks[i] : "";
                sb.AppendLine($"{key}={val}");
            }
        }

        public static void AppendWrappedProperty(StringBuilder sb, string key, string value)
        {
            value = value ?? "";

            int maxLine = 255;
            int prefixLength = key.Length + 1;
            int chunkMax = Math.Max(1, maxLine - prefixLength);

            var chunks = SplitSqlIntoLines(value, chunkMax);

            if (chunks.Count == 0)
            {
                sb.AppendLine($"{key}=");
                return;
            }

            for (int i = 0; i < chunks.Count; i++)
            {
                if (i == 0)
                    sb.AppendLine($"{key}={chunks[i]}");
                else
                    sb.AppendLine($"{key}{i + 1}={chunks[i]}");
            }
        }
    }
}
