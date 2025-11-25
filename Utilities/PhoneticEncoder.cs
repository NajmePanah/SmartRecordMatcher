using System;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartRecordMatcher.Utilities
{
    public static class PhoneticEncoder
    {
        // یک تابع سادهٔ phonetic: حذف حروف غیرضروری، نگاشت برخی حروف نزدیک آوا
        public static string Encode(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";

            s = s.Trim().ToLowerInvariant();

            s = s.Replace("آ", "ا").Replace("أ", "ا").Replace("إ", "ا");
            s = s.Replace("و", "و").Replace("ؤ", "و");
            s = s.Replace("ۀ", "ه");

            // map similar sounding groups
            s = Regex.Replace(s, "[قغ]", "ق");
            s = Regex.Replace(s, "[صثس]", "س");
            s = Regex.Replace(s, "[طظزذ]", "ز");
            s = Regex.Replace(s, "[چج]", "ج");
            s = Regex.Replace(s, "[ایىي]", "ی");

            // remove vowels (option): keep only consonant skeleton
            s = Regex.Replace(s, "[aeiouآاوییو]", "");

            // remove non letters/numbers
            s = Regex.Replace(s, @"[^آ-ی0-9]", "");

            return s;
        }
    }
}
