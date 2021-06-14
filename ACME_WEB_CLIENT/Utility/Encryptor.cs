using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ACME_WEB_CLIENT.Utility
{
    public class Encryptor
    {
        //encrypts a string and returns the encrypted string
        public static string hashString(string input)
        {
            byte[] hash;

            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                hash = sha1.ComputeHash(Encoding.Unicode.GetBytes(input));
            }

            var sb = new StringBuilder();

            foreach (byte b in hash)
            {
                sb.AppendFormat("{0:x2}", b);
            }

            return sb.ToString();
        }
    }
}
