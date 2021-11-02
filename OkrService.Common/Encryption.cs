using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OKRService.Common
{
    public static class Encryption
    {
        public static string DecryptStringAes(string cipherText, string secretKey, string secretIvKey)
        {
            string decryptedFromJavascript = null;
            try
            {
                var bytestokey = Encoding.UTF8.GetBytes(secretKey);
                var iv = Encoding.UTF8.GetBytes(secretIvKey);
                if (!string.IsNullOrEmpty(cipherText))
                {
                    var encrypted = Convert.FromBase64String(cipherText);
                    decryptedFromJavascript = DecryptStringFromBytes(encrypted, bytestokey, iv);
                }
            }
            catch
            {
                decryptedFromJavascript = null;
            }
            return decryptedFromJavascript;
        }
        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException(nameof(cipherText));
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException(nameof(key));
            }
            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using var rijAlg = new RijndaelManaged();
            //Settings
            rijAlg.Mode = CipherMode.CBC;
            rijAlg.Padding = PaddingMode.PKCS7;
            rijAlg.FeedbackSize = 128;
            rijAlg.Key = key;
            rijAlg.IV = iv;
            // Create a decrytor to perform the stream transform.
            var decryption = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
            CryptoStream csDecrypt = null;
            try
            {
                // Create the streams used for decryption.
                using var msDecrypt = new MemoryStream(cipherText);
                csDecrypt = new CryptoStream(msDecrypt, decryption, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                csDecrypt = null;
                // Read the decrypted bytes from the decrypting stream
                // and place them in a string.
                plaintext = srDecrypt.ReadToEnd();
            }
            catch
            {
                plaintext = null;
            }
            finally
            {
                if (csDecrypt != null)
                    csDecrypt.Dispose();
            }

            return plaintext;
        }
        private static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            // Check arguments.  
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException(nameof(plainText));
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException(nameof(key));
            }
            byte[] encrypted;
            // Create a RijndaelManaged object  
            // with the specified key and IV.  
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;
                rijAlg.Key = key;
                rijAlg.IV = iv;
                // Create a decrytor to perform the stream transform.  
                var encrypt = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
                // Create the streams used for encryption.  
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encrypt, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.  
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.  
            return encrypted;
        }
        public static string EncryptStringAes(string plainText, string secretKey, string secretIvKey)
        {
            string decryptedFromJavascript = null;
            try
            {
                var bytestokey = Encoding.UTF8.GetBytes(secretKey);
                var iv = Encoding.UTF8.GetBytes(secretIvKey);
                if (!string.IsNullOrEmpty(plainText))
                {
                    var byteArray = EncryptStringToBytes(plainText, bytestokey, iv);
                    decryptedFromJavascript = Convert.ToBase64String(byteArray);
                }
            }
            catch
            {
                decryptedFromJavascript = null;
            }
            return decryptedFromJavascript;
        }

        #region Rijndael Encryption
        public static string DecryptRijndael(string cipherinput, string salt)
        {
            if (string.IsNullOrEmpty(cipherinput))
                throw new ArgumentNullException("cipherinput");
            if (!IsBase64String(cipherinput))
                throw new FormatException("The cipherText input parameter is not base64 encoded");
            var aesAlg = NewRijndaelManaged(salt);
            var decryption = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            var cipher = Convert.FromBase64String(cipherinput);
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryption, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            var text = srDecrypt.ReadToEnd();

            return text;
        }
        private static RijndaelManaged NewRijndaelManaged(string salt)
        {
            string InputKey = "99334E81-342C-4900-86D9-07B7B9FE5EBB";
            if (salt == null) throw new ArgumentNullException("salt");
            var saltBytes = Encoding.ASCII.GetBytes(salt);
            var key = new Rfc2898DeriveBytes(InputKey, saltBytes);

            var aesAlg = new RijndaelManaged();
            aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
            aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);
            return aesAlg;
        }
        private static bool IsBase64String(string base64String)
        {
            base64String = base64String.Trim();
            return (base64String.Length % 4 == 0) &&
                   Regex.IsMatch(base64String, Constants.Base64Regex, RegexOptions.None);

        }
        #endregion
    }
}

