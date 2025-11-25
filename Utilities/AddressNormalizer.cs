using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRecordMatcher.Utilities
{
    public class AddressNormalizer
    {
        // ساده‌سازی: تبدیل به حروف کوچک و حذف فاصله اضافی
        public string Normalize(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return "";
            return System.Text.RegularExpressions.Regex.Replace(address.ToLower().Trim(), @"\s+", " ");
        }

        // تقسیم آدرس به توکن‌ها (کلمات)
        public List<string> Tokenize(string address)
        {
            return Normalize(address).Split(' ').ToList();
        }
    }

}
