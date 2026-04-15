using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatEditorVDGrid.Services
{
    internal class DatService
    {
        public static Dictionary<string, string> ParseDatFile(string path)
        {
            var dict = new Dictionary<string, string>();
            var lines = File.ReadAllLines(path);

            string currentSection = "";

            foreach (var line in lines)
            {
                if (line.StartsWith("["))
                {
                    currentSection = line.Trim();
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line) || !line.Contains("="))
                    continue;

                var parts = line.Split(new[] { '=' }, 2);
                string key = parts[0].Trim();
                string value = parts[1].Trim();

                dict[key] = value;
            }

            return dict;
        }

        public static string GetFullProperty(Dictionary<string, string> data, string baseKey)
        {
            var parts = new List<string>();

            int i = 1;

            while (true)
            {
                string key = (i == 1) ? baseKey : $"{baseKey}{i}";

                if (!data.ContainsKey(key))
                    break;

                parts.Add(data[key]);
                i++;
            }

            return string.Join(" ", parts);
        }
    }
}
