//using System.Text;
//using System.Text.RegularExpressions;
//using SmartRecordMatcher.Models;
//using SmartRecordMatcher.Utilities;

//namespace SmartRecordMatcher.Services
//{
//    public class AddressParser
//    {
//        private readonly AddressNormalizer normalizer = new();

//        public AddressFields Parse(string raw)
//        {
//            var f = new AddressFields();
//            if (string.IsNullOrWhiteSpace(raw)) return f;

//            string s = normalizer.Normalize(raw);

//            // city (heuristic: look for known cities as tokens) - can be improved by a list
//            var cityMatch = Regex.Match(s, @"\b(تهران|مشهد|اصفهان|شیراز|کرج|تبریز)\b", RegexOptions.IgnoreCase);
//            if (cityMatch.Success) f.City = cityMatch.Groups[1].Value;

//            // plaque
//            var plaque = Regex.Match(raw, @"پلاک[:\s\-]*([0-9\u06F0-\u06F9]+)", RegexOptions.IgnoreCase);
//            if (plaque.Success) f.Plaque = NormalizeDigits(plaque.Groups[1].Value);

//            // unit
//            var unit = Regex.Match(raw, @"واحد[:\s\-]*([0-9\u06F0-\u06F9]+)", RegexOptions.IgnoreCase);
//            if (unit.Success) f.Unit = NormalizeDigits(unit.Groups[1].Value);

//            // alley (کوچه ... یا بن‌بست)
//            var alley = Regex.Match(s, @"\b(کوچه|بن بست|بن‌بست|کوی)\s*([\p{L}\d\-]+)", RegexOptions.IgnoreCase);
//            if (alley.Success) f.Alley = alley.Groups[2].Value;

//            // street detection: try phrases with خیابان/بلوار/میدان
//            var street = Regex.Match(s, @"\b(خیابان|بلوار|میدان)\s+([\p{L}\d\s\-]+)", RegexOptions.IgnoreCase);
//            if (street.Success) f.Street = street.Groups[2].Value.Trim();

//            // fallback: take first long token as street
//            if (string.IsNullOrWhiteSpace(f.Street))
//            {
//                var tokens = normalizer.Tokenize(s);
//                var candidate = tokens.FirstOrDefault(t => t.Length > 3 && !Regex.IsMatch(t, @"^\d+$"));
//                if (!string.IsNullOrWhiteSpace(candidate)) f.Street = candidate;
//            }

//            // region heuristic: محله
//            var region = Regex.Match(s, @"\bمحله\s+([\p{L}\d\s\-]+)", RegexOptions.IgnoreCase);
//            if (region.Success) f.Region = region.Groups[1].Value.Trim();

//            return f;
//        }

//        private string NormalizeDigits(string s)
//        {
//            if (string.IsNullOrWhiteSpace(s)) return s;
//            var sb = new StringBuilder();
//            foreach (var c in s)
//            {
//                if (c >= 0x06F0 && c <= 0x06F9) sb.Append((char)('0' + (c - 0x06F0)));
//                else if (c >= '0' && c <= '9') sb.Append(c);
//            }
//            return sb.ToString();
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SmartRecordMatcher.Models;
using SmartRecordMatcher.Utilities;

namespace SmartRecordMatcher.Services
{
    public class AddressParser
    {
        private readonly AddressNormalizer normalizer = new();

    // لیست شهرها قابل توسعه است
    private readonly HashSet<string> KnownCities = new(StringComparer.OrdinalIgnoreCase)
    {
        "تهران", "مشهد", "اصفهان", "شیراز", "کرج", "تبریز", "قم", "اهواز"
    };

        // نگاشت مخفف‌ها به متن کامل
        private readonly Dictionary<string, string> Abbreviations = new(StringComparer.OrdinalIgnoreCase)
        {
            { "خ", "خیابان" }, { "بل", "بلوار" }, { "م", "میدان" },{ "بولوار", "بلوار" },
            { "پ", "پلاک" }, { "و", "واحد" }, { "ک", "کوچه" },
            { "بن‌ب", "بن‌بست" }, { "بن ب", "بن‌بست" }, { "کوی", "کوچه" }
        };

        public AddressFields Parse(string raw)
        {
            var f = new AddressFields();
            if (string.IsNullOrWhiteSpace(raw)) return f;

            string s = normalizer.Normalize(raw);

            // جایگزینی مخفف‌ها
            foreach (var kv in Abbreviations)
            {
                s = Regex.Replace(s, $@"\b{kv.Key}\b", kv.Value, RegexOptions.IgnoreCase);
            }

            // تشخیص شهر
            foreach (var city in KnownCities)
            {
                if (Regex.IsMatch(s, $@"\b{Regex.Escape(city)}\b", RegexOptions.IgnoreCase))
                {
                    f.City = city;
                    break;
                }
            }

            // پلاک
            var plaque = Regex.Match(s, @"\bپلاک[:\s\-]*([0-9\u06F0-\u06F9]+)", RegexOptions.IgnoreCase);
            if (plaque.Success) f.Plaque = NormalizeDigits(plaque.Groups[1].Value);

            // واحد
            var unit = Regex.Match(s, @"\bواحد[:\s\-]*([0-9\u06F0-\u06F9]+)", RegexOptions.IgnoreCase);
            if (unit.Success) f.Unit = NormalizeDigits(unit.Groups[1].Value);

            // کوچه / بن‌بست / کوی
            var alley = Regex.Match(s, @"\b(کوچه|بن‌بست|بن بست|کوی)\s*([\p{L}\d\-]+)", RegexOptions.IgnoreCase);
            if (alley.Success) f.Alley = alley.Groups[2].Value;

            // خیابان / بلوار / میدان
            var street = Regex.Match(s, @"\b(خیابان|بلوار|میدان)\s+([\p{L}\d\s\-]+)", RegexOptions.IgnoreCase);
            if (street.Success) f.Street = street.Groups[2].Value.Trim();

            // fallback برای خیابان: اولین توکن طولانی غیر عددی
            if (string.IsNullOrWhiteSpace(f.Street))
            {
                var tokens = normalizer.Tokenize(s);
                var candidate = tokens.Find(t => t.Length > 3 && !Regex.IsMatch(t, @"^\d+$"));
                if (!string.IsNullOrWhiteSpace(candidate)) f.Street = candidate;
            }

            // محله
            var region = Regex.Match(s, @"\bمحله\s+([\p{L}\d\s\-]+)", RegexOptions.IgnoreCase);
            if (region.Success) f.Region = region.Groups[1].Value.Trim();

            return f;
        }

        private string NormalizeDigits(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;

            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (c >= 0x06F0 && c <= 0x06F9) sb.Append((char)('0' + (c - 0x06F0)));
                else if (c >= '0' && c <= '9') sb.Append(c);
            }
            return sb.ToString();
        }
    }

}
