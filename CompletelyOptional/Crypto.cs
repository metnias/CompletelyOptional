using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CompletelyOptional
{
    /// <summary>
    /// This Encrypt/Decrypt data/config.
    /// https://stackoverflow.com/questions/5251759/easy-way-to-encrypt-obfuscate-a-byte-array-using-a-secret-in-net
    /// </summary>
    public static class Crypto
    {
        /*
        public static void DumpString(string value)
        {
            Debug.Log(value);
            foreach (char c in value)
            {
                Debug.Log(string.Concat( "{0:x4} ", (int)c ));
            }
        }*/

        /// <summary>
        /// Create and initialize a crypto algorithm.
        /// </summary>
        /// <param name="password">The password.</param>
        private static SymmetricAlgorithm GetAlgorithm(string password)
        {
            var algorithm = Rijndael.Create(); // salty goodness
            var rdb = new Rfc2898DeriveBytes(password, new byte[] {
        0x64,0xad,0xca,0xe1,0x0d,0x5c,0x28,             // salty goodness
        0x26,0x90,0x63,0xd5,0x7f,0x61,0xf2,0x31
    });
            algorithm.Padding = PaddingMode.ISO10126;
            algorithm.Key = rdb.GetBytes(32);
            algorithm.IV = rdb.GetBytes(16);
            return algorithm;
        }

        /// <summary>
        /// Encrypts a string with a given password.
        /// </summary>
        /// <param name="clearText">The clear text.</param>
        /// <param name="password">The password.</param>
        public static string EncryptString(string clearText, string password)
        {
            SymmetricAlgorithm algorithm = GetAlgorithm(password);
            ICryptoTransform encryptor = algorithm.CreateEncryptor();
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            //return Convert.ToBase64String(clearBytes);

            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(clearBytes, 0, clearBytes.Length);
                cs.Close();

                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// Decrypts a string using a given password.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="password">The password.</param>
        public static string DecryptString(string cipherText, string password)
        {
            SymmetricAlgorithm algorithm = GetAlgorithm(password);
            ICryptoTransform decryptor = algorithm.CreateDecryptor();
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            //return Encoding.Unicode.GetString(cipherBytes);

            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                cs.Write(cipherBytes, 0, cipherBytes.Length);
                cs.Close();

                return Encoding.Unicode.GetString(ms.ToArray());
            }
        }
    }
}