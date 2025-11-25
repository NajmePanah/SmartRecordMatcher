using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartRecordMatcher.Utilities
{
    public class AddressNormalizer
    {
        private static readonly Dictionary<char, char> CharMap = new()
        {
            ['ي'] = 'ی',
            ['ك'] = 'ک',
            ['ة'] = 'ه',
            ['ـ'] = ' '
        };

        private static readonly Dictionary<char, char> PersianDigits = new()
        {
            ['\u06F0'] = '0',
            ['\u06F1'] = '1',
            ['\u06F2'] = '2',
            ['\u06F3'] = '3',
            ['\u06F4'] = '4',
            ['\u06F5'] = '5',
            ['\u06F6'] = '6',
            ['\u06F7'] = '7',
            ['\u06F8'] = '8',
            ['\u06F9'] = '9'
        };

        private static readonly Dictionary<string, string> Abbreviations = new()
        {
            {"خ", "خیابان"}, {"خیابانها","خیابان"}, {"بل","بلوار"}, {"بلوارها","بلوار"},
            {"مید","میدان"}, {"م.", "میدان"}
        };

        private static readonly string[] StopWords = new[]
        {
            "طبقه","واحد","روبرو","روبروی","جنب","نبش","پلاک","پلاک:" // پلاک رو حذف نکنیم کامل، فقط به عنوان stop
        };

        public string Normalize(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";

            string s = raw.Trim();

            // map chars
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (CharMap.ContainsKey(c)) sb.Append(CharMap[c]);
                else if (PersianDigits.ContainsKey(c)) sb.Append(PersianDigits[c]);
                else sb.Append(c);
            }
            s = sb.ToString();

            // lowercase (latin part)
            s = s.ToLowerInvariant();

            // replace punctuation with space
            s = Regex.Replace(s, @"[.,؛;:/\\()""\[\]\{\}\-‏-–—]", " ");

            // normalize persian comma
            s = s.Replace("،", " ");

            // collapse whitespace
            s = Regex.Replace(s, @"\s+", " ").Trim();

            // expand simple abbreviations
            foreach (var kv in Abbreviations)
            {
                s = Regex.Replace(s, $@"\b{Regex.Escape(kv.Key)}\b", kv.Value);
            }

            // trim again
            s = Regex.Replace(s, @"\s+", " ").Trim();

            return s;
        }

        // پایه‌ای: شکستن به توکن‌های معنی‌دار (حفظ اعداد جدا)
        public List<string> Tokenize(string normalized)
        {
            if (string.IsNullOrWhiteSpace(normalized)) return new List<string>();
            // جداکننده‌ها: space و comma
            var parts = Regex.Split(normalized, @"[,\s]+")
                             .Where(p => !string.IsNullOrWhiteSpace(p))
                             .Select(p => p.Trim())
                             .ToList();
            return parts;
        }
    }
}
