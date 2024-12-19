using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Project.Helphers
{
    public class Guard
    {
        public static string Key { get; set; }
        public static string Salt { get; set; }

        public static void Start_Session()
        {
            try
            {
                Key = Algorithms.SaltString(Convert.ToBase64String(Encoding.Default.GetBytes(Setting.Instance.key)));
                Salt = Algorithms.SaltString(Convert.ToBase64String(Encoding.Default.GetBytes(Setting.Instance.salt)));
            }
            catch
            {
                Key = Algorithms.SaltString(Convert.ToBase64String(Encoding.Default.GetBytes(Setting.Instance.key)));
                Salt = Algorithms.SaltString(Convert.ToBase64String(Encoding.Default.GetBytes(Setting.Instance.salt)));
            }
        }


        internal class Algorithms
        {
            public static string SaltString(string value)
            {
                value = value.Replace("a", "!");
                value = value.Replace("z", "?");
                value = value.Replace("b", "}");
                value = value.Replace("c", "{");
                value = value.Replace("d", "]");
                value = value.Replace("e", "[");
                return value;
            }
            public static string DesaltString(string value)
            {
                value = value.Replace("?", "z");
                value = value.Replace("!", "a");
                value = value.Replace("}", "b");
                value = value.Replace("{", "c");
                value = value.Replace("]", "d");
                value = value.Replace("[", "e");
                return value;
            }
            public static string DecryptData(string value)
            {
                string message = value;
                string password = Encoding.Default.GetString(Convert.FromBase64String(DesaltString(Guard.Key)));
                SHA256 mySHA256 = SHA256Managed.Create();
                byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(password));
                byte[] iv = Encoding.ASCII.GetBytes(Encoding.Default.GetString(Convert.FromBase64String(DesaltString(Guard.Salt))));
                string decrypted = DecryptString(message, key, iv);
                return decrypted;
            }
            public static string EncryptData(string value)
            {
                string message = value;
                string password = Encoding.Default.GetString(Convert.FromBase64String(DesaltString(Guard.Key)));
                SHA256 mySHA256 = SHA256Managed.Create();
                byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(password));
                byte[] iv = Encoding.ASCII.GetBytes(Encoding.Default.GetString(Convert.FromBase64String(DesaltString(Guard.Salt))));
                string decrypted = EncryptString(message, key, iv);
                return decrypted;
            }
            public static string EncryptString(string plainText, byte[] key, byte[] iv)
            {
                Aes encryptor = Aes.Create();
                encryptor.Mode = CipherMode.CBC;
                encryptor.Key = key;
                encryptor.IV = iv;
                MemoryStream memoryStream = new MemoryStream();
                ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);
                byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);
                cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] cipherBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);
                return cipherText;
            }
            public static string DecryptString(string cipherText, byte[] key, byte[] iv)
            {
                Aes encryptor = Aes.Create();
                encryptor.Mode = CipherMode.CBC;
                encryptor.Key = key;
                encryptor.IV = iv;
                MemoryStream memoryStream = new MemoryStream();
                ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);
                string plainText = String.Empty;
                try
                {
                    byte[] cipherBytes = Convert.FromBase64String(cipherText);
                    cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    byte[] plainBytes = memoryStream.ToArray();
                    plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
                }
                finally
                {
                    memoryStream.Close();
                    cryptoStream.Close();
                }
                return plainText;
            }
            public static string Encrypt(string clearText)
            {

                byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes("datexd", new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    encryptor.Padding = PaddingMode.PKCS7;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        clearText = Convert.ToBase64String(ms.ToArray());
                    }
                }
                return clearText;
            }
            public static string Decrypt(string cipherText)
            {
                cipherText = cipherText.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes("datexd", new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    encryptor.Padding = PaddingMode.PKCS7;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return cipherText;
            }
            public static string CalculateMD5(string filename)
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
        }
    }

}
