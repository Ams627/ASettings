using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASettings
{
    /// <summary>
    /// global settings are specified in the global section and apply everywhere
    /// other sections can be swapped in our out. We have a dictionary for global
    /// setting and a dictionary for each section. So in effect we need 
    /// Dictionary<string, Dictionary<string, List<string>>
    /// </summary>
    public class Settings
    {
        public class ValidSetting
        {
            public string Name { get; set; }
            public bool IsSingle { get; set; }
        }

        private readonly string _settingsFile;
        Dictionary<string, Dictionary<string, List<string>>> _settings = new Dictionary<string, Dictionary<string, List<string>>>();
        IEnumerable<ValidSetting> _validSettings;

        List<(int linenumber, string line)> _errors = new List<(int linenumber, string line)>();
        public Settings(string filename, IEnumerable<ValidSetting> validSettings = null)
        {
            this._settingsFile = filename;
            this._validSettings = validSettings;

            var currentSection = "";
            var linenumber = 0;
            foreach (var (line, skip, equalsCount) in File.ReadLines(_settingsFile).Select(ProcessLine))
            {
                linenumber++;
                if (skip)
                {
                    continue;
                }
                if (line.First() == '[')
                {
                    if (line.Last() != ']')
                    {
                        _errors.Add((linenumber, line));
                        continue;
                    }
                    currentSection = line.Substring(1, line.Length - 2);
                }
                else if (equalsCount != 1)
                {
                    _errors.Add((linenumber, line));
                    continue;
                }
                else if (string.IsNullOrEmpty(currentSection))
                {
                    _errors.Add((linenumber, line));
                    continue;
                }
                else
                {
                    AddDictionaryEntry(currentSection, line);
                }
            }

        }

        public string[] Sections => _settings.Keys.ToArray();

        public (int linenumber, string line)[] Errors => _errors.ToArray();

        public string[] Setting(string section, string name)
        {
            if (_settings.TryGetValue(section, out var innerDict))
            {
                if (innerDict.TryGetValue(name, out var list))
                {
                    return list.ToArray();
                }
            }
            return new string[] { };
        }

        private void AddDictionaryEntry(string currentSection, string line)
        {
            if (!_settings.TryGetValue(currentSection, out var innerDict))
            {
                innerDict = new Dictionary<string, List<string>>();
                _settings.Add(currentSection, innerDict);
            }

            var segments = line.Split('=').Select(x => x.Trim()).ToArray();
            if (!innerDict.TryGetValue(segments[0], out var list))
            {
                list = new List<string>();
                innerDict.Add(segments[0], list);
            }
            list.Add(segments[1]);
        }

        private (string line, bool skip, int equalsCount) ProcessLine(string line)
        {
            var equalCount = 0;
            if (string.IsNullOrEmpty(line))
            {
                return (line, true, 0);
            }

            var trimmed = line.Trim();
            if (trimmed.StartsWith("#"))
            {
                return (trimmed, true, 0);
            }

            var quote = 0;
            int count = 0;
            foreach (var c in trimmed)
            {
                if (quote == 0)
                {
                    if (c == '#')
                    {
                        break;
                    }
                    else if (c == '\'')
                    {
                        quote = 1;
                    }
                    else if (c == '"')
                    {
                        quote = 2;
                    }
                    else if (c == '=')
                    {
                        equalCount++;
                    }
                }
                else if (quote == 1)
                {
                    if (c == '\'')
                    {
                        quote = 0;
                    }
                }
                else if (quote == 2)
                {
                    if (c == '"')
                    {
                        quote = 0;
                    }
                }

                count++;
            }

            return (trimmed.Substring(0, count), false, equalCount);
        }
    }
}
