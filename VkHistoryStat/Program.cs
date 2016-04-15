using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kasthack.vksharp;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace VkHistoryStat
{
    public class Message
    {
        public string body
        {
            get; set;
        }
        public int from { get; set; }

        public static implicit operator Message(JObject o)
        {
            return new Message { from = (int)o["from_id"], body = o["body"].ToString().ToUpper() };
        }
    }
    class Program
    {
        private static string[] test =
        {
            "ТВОЙ",
            "ЛАБЫ",
            "ЕМУ",
            "СКАЗАЛА",
            "ОДНА",
            "Т",
            "НАПИСАНО",
            "40",
            "THE",
            "ЧАСА",
            "ЕХАТЬ",
            "ТЕЛЕФОН",
            "НОРМАЛЬНО",
            "СЕРЬЕЗНО",
            "МОЙ",
            "ORG",
            "14",
            "ОНО",
            "ЭТА",
            "ВСЁ",
            "ПРИЕЗЖАЙ",
            "МОЛОДЕЦ",
            "СМОТРЮ",
            "ПРИЕДЕШЬ",
            "ТИПА",
            "КАКОГО",
            "ПОЗЖЕ",
            "КОТОРЫЙ",
            "КАКОЕ",
            "LOL",
            "18",
            "00",
            "19",
            "СРАЗУ",
            "КУПИЛ",
            "ЗНАЕШЬ",
            "ЧУДНО"
        };
        static Dictionary<string, int> myDict = new Dictionary<string, int>();
        static Dictionary<string, int> userDict = new Dictionary<string, int>();
        static Regex reg = new Regex(@"[\wёЁ]+", RegexOptions.Compiled);
        static int user = 5843234;

        static void Main(string[] args)
        {
            var y1y = MyStemWorker.MyStemConvert(test);

            var redirect_uri = Token.GetOAuthURL(
                    5390280,    
                    Permission.Messages 
                    );
            var token = new Token("8501d914fb2dab6f4a440b63b4f1f4880f7d779449cade3bb21151e1a8683ef72a13da212069902618dfb");
            var api = new RawApi();
            api.AddToken(token);

            var items = GetMessages(api, 0);
            ExtractInfo(items);
            for(int i = 200; i < 21000; i += 200)
            {
                Thread.Sleep(350);
                ExtractInfo(GetMessages(api, i));
            }
            var yy = myDict.OrderByDescending(x => x.Value);
            var yy1 = userDict.OrderByDescending(x => x.Value);

            int a = 0;
        }
        static IEnumerable<Message> GetMessages(RawApi api, int offset)
        {
            var yy = api.Messages.GetHistoryUser(user, offset: offset).Result;
            dynamic oo = Newtonsoft.Json.JsonConvert.DeserializeObject(yy);
            return (oo.response.items as JArray).Cast<JObject>().Select(x => (Message)x);
        }

        static void ExtractInfo(IEnumerable<Message> items)
        {
            foreach (var m in items)
            {
                foreach (Match e in reg.Matches(m.body))
                    if (m.from == user)
                        userDict.AddOrInc(e.Value);
                    else
                        myDict.AddOrInc(e.Value);

            }
        }
    }

}

public static class Ext
{
    public static void AddOrInc(this Dictionary<string,int> d, string word)
    {
        int tmp;
        if (d.TryGetValue(word, out tmp))
            d[word]++;
        else
            d.Add(word, 1);
    }
}
