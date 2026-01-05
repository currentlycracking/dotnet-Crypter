using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace dotNetCrypt.Protections.StringEncrypter {
    internal static class DecryptionHelper {
        public static string Decrypt_Base64(string dataEnc) {
            try {
                return Encoding.UTF8.GetString(Convert.FromBase64String(dataEnc));
            } catch(Exception) {
                return string.Empty;
            }
        }
    }
}
