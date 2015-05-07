using System;
using System.Net;
using System.Text;

class HttpUtility
{
    internal static string UrlEncode(string query, Encoding encoding)
    {
        if (encoding == Encoding.GetEncoding("windows-1251"))
        {
            byte[] win1251bytes = Encoding.GetEncoding("windows-1251").GetBytes(query);
            string hex = BitConverter.ToString(win1251bytes);
            string result = "%" + hex.Replace("-", "%");
            return result;
        }
        return WebUtility.UrlEncode(query);
    }
}
