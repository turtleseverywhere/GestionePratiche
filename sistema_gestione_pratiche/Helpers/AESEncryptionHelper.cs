using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace sistema_gestione_pratiche.Helpers
{
    public static class AESEncryptionHelper
    {
        public static string? DefaultKey { get; set; } = "ars856bvre81s51vr5e6y7ry8e91f133";

        public static string? EncryptString(string? toEncrypt, string? key = null)
        {
            if (toEncrypt != null)
            {
                if (key == null)
                    key = DefaultKey;
                byte[] iv = new byte[16];
                byte[] array;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = iv;
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                            {
                                streamWriter.Write(toEncrypt);
                            }
                            array = memoryStream.ToArray();
                        }
                    }
                }
                toEncrypt = Convert.ToBase64String(array);
            }
            return toEncrypt;
        }
        public static string? DecryptString(string? toDecrypt, string? key = null)
        {
            if (toDecrypt != null)
            {
                try
                {
                    if (key == null)
                        key = DefaultKey;
                    byte[] iv = new byte[16];
                    byte[] buffer = Convert.FromBase64String(toDecrypt);

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = Encoding.UTF8.GetBytes(key);
                        aes.IV = iv;
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                        using (MemoryStream memoryStream = new MemoryStream(buffer))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                                {
                                    toDecrypt = streamReader.ReadToEnd();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    //in questo caso restituisco la stringa originale
                }
            }
            return toDecrypt;
        }
        public static byte[]? EncryptFile(byte[]? toEncrypt, string key = null)
        {
            if (toEncrypt != null)
            {
                if (key == null)
                    key = DefaultKey;
                byte[] iv = new byte[16];

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = iv;
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using (var ms = new MemoryStream())
                    using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(toEncrypt, 0, toEncrypt.Length);
                        cryptoStream.FlushFinalBlock();
                        toEncrypt = ms.ToArray();
                    }
                }
            }
            return toEncrypt;
        }
        /*CryptoStream dalla versione .net 6.0 in poi è cambiato e a quanto pare
         è più pistino sulla lettura.
        fare semplicemente cryptoStream.Read non funziona più in tutti i casi, e quindi
        bisogna leggere a manella.
        ref: https://github.com/dotnet/runtime/issues/61535
        */
        public static byte[]? DecryptFile(byte[]? toDecrypt, string key = null)
        {
            if (toDecrypt != null)
            {
                try
                {
                    if (key == null)
                        key = DefaultKey;
                    byte[] iv = new byte[16];

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = Encoding.UTF8.GetBytes(key);
                        aes.IV = iv;
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                        using (var ms = new MemoryStream(toDecrypt))
                        using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            var dycrypted = new byte[toDecrypt.Length];
                            //var bytesRead = cryptoStream.Read(dycrypted, 0, toDecrypt.Length);

                            int bytesRead = 0;
                            while (bytesRead < dycrypted.Length)
                            {
                                int n = cryptoStream.Read(dycrypted, bytesRead, dycrypted.Length - bytesRead);
                                if (n == 0)
                                    break;
                                bytesRead += n;
                            }
                            toDecrypt = dycrypted.Take(bytesRead).ToArray();
                        }
                    }
                }
                catch
                {
                    //in questo caso restituisco il byte[] originale
                }
            }
            return toDecrypt;
        }
    }
}
