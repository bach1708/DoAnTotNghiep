using System.Text;
using System.Text.RegularExpressions;

namespace MangaShop.Helpers
{
    public static class TextHelper
    {
        // chuyển về chữ thường, bỏ dấu, bỏ ký tự đặc biệt, bỏ khoảng trắng
        public static string NormalizeForSearch(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            input = input.Trim().ToLower();

            // bỏ dấu tiếng Việt
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            var noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

            // bỏ ký tự không phải chữ/số (bao gồm khoảng trắng)
            noDiacritics = Regex.Replace(noDiacritics, @"[^a-z0-9]+", "");

            return noDiacritics;
        }
    }
}
