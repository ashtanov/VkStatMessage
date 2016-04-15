using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;

namespace VkHistoryStat
{
    public class NormalizedWord
    {
        public string word { get; set; }
        public string normalizedWord { get; set; }
        public override string ToString()
        {
            return $"{word} {normalizedWord}";
        }
    }
    public class WordNFreq
    {
        public int freq { get; set; }
        public string word { get; set; }
    }
    public class MyStemWorker
    {
        private static Dictionary<string, string> normalizedWords;
        private static XmlSerializer s = new XmlSerializer(typeof(List<KeyValuePair<string, string>>));
        private static object locker;
        static Regex matchResult = new Regex("(.*?){(.*?)[|}?]", RegexOptions.Compiled);
        private static ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = @"C:\mystem.exe",
            Arguments = "-n",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        };

        static MyStemWorker()
        {
            if (File.Exists("cache.nfd"))
                using (var stream = new StreamReader("cache.nfd"))
                {
                    var yy = s.Deserialize(stream) as List<KeyValuePair<string, string>>;
                    normalizedWords = yy.ToDictionary(x => x.Key, x => x.Value);
                }
            normalizedWords = new Dictionary<string, string>();

            Timer timer = new Timer(
                AutoSave,
                null,
                TimeSpan.FromSeconds(50),
                TimeSpan.FromMinutes(1)
                );
        }

        static void AutoSave(object o)
        {
            lock (locker)
            {
                using (var stream = new StreamWriter("cache.nfd"))
                    s.Serialize(stream, normalizedWords.ToList());
            }
        }

        public static ICollection<NormalizedWord> GetNormalForm(IEnumerable<WordNFreq> words)
        {
            List<WordNFreq> toResolve = new List<WordNFreq>();
            List<NormalizedWord> result = new List<NormalizedWord>();
            foreach (var word in words)
            {
                string nForm;
                if (normalizedWords.TryGetValue(word.word, out nForm))
                    result.Add(new NormalizedWord { normalizedWord = nForm, word = word.word });
                else
                    toResolve.Add(word);
            }

            return result;
        }

        public static ICollection<NormalizedWord> MyStemConvert(ICollection<string> coll)
        {
            var input = string.Join("\n", coll);
            string output;
            lock (locker)
            {
                Process mystem = new Process { StartInfo = startInfo };
                mystem.Start();
                StreamWriter mystemStreamWriter = new StreamWriter(mystem.StandardInput.BaseStream, Encoding.UTF8);
                StreamReader mystemStreamReader = new StreamReader(mystem.StandardOutput.BaseStream, Encoding.UTF8);
                mystemStreamWriter.Write(input);
                mystemStreamWriter.Flush();
                mystemStreamWriter.Close();
                output = mystemStreamReader.ReadToEnd();
                mystem.WaitForExit();
                mystem.Close();
            }
            var list = output.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(x =>
            {
                var res = matchResult.Match(x);
                return new NormalizedWord { normalizedWord = res.Groups[2].Value, word = res.Groups[1].Value };
            }).ToList();
            int i = 0;
            foreach (var elem in coll)
            {
                if (!list[i].Equals(elem))
                {
                    //Добавить слова, необработанные майстемом!
                }
            }
            return list;
        }


    }
}