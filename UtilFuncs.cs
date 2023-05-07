using System.Security.Cryptography;
using System.Text;

namespace Animal_Hotel
{
    public static class UtilFuncs
    {
        public static string DateToHtmlFormatter(DateTime date)
        {
            date = date == default(DateTime) ? DateTime.Now : date;
            string year = date.Year.ToString();
            _ = year.PadLeft(4, '0');
            string month = date.Month.ToString().PadLeft(2, '0');
            string day = date.Day.ToString().PadLeft(2, '0');
            string htmlDate = $"{year}-{month}-{day}";
            return htmlDate;
        }

        public static string Sha256_Hash(string value)
        {
            var sb = new StringBuilder();
            using SHA256 hash = SHA256.Create();
            Byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
            return sb.GetString(result);
        }
    }
}
