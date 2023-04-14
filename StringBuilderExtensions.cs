using System.Text;

namespace Animal_Hotel
{
    public static class StringBuilderExtensions
    {
        public static string GetString(this StringBuilder sb,byte[] bytes)
        {
            foreach(var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
