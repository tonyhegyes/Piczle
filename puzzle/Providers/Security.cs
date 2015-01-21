using System;
using System.IO;
using System.Text;
using CryptSharp.Utility;
using System.Security.Cryptography;


namespace puzzle.Providers.Security
{
    public class RijndaelEncryption : IDisposable
    {
        Rfc2898DeriveBytes pwdGen;
        public RijndaelEncryption(string passPhrase, string salt)
        {
            byte[] Salt = Encoding.ASCII.GetBytes(salt);
            pwdGen = new Rfc2898DeriveBytes(passPhrase, Salt, 10000);
        }

        public byte[] EncryptStringToBytes(string plainText)
        {
            byte[] encrypted;

            using (RijndaelManaged rijAlg = new RijndaelManaged() { BlockSize = 256, Key = pwdGen.GetBytes(32), IV = pwdGen.GetBytes(32), Padding = PaddingMode.ISO10126 })
            {
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, rijAlg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText); //Write all data to the stream.
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            pwdGen.Reset();
            return encrypted;
        }
        public string DecryptStringFromBytes(byte[] cipherText)
        {
            string plaintext = null;

            using (RijndaelManaged rijAlg = new RijndaelManaged() { BlockSize = 256, Key = pwdGen.GetBytes(32), IV = pwdGen.GetBytes(32), Padding = PaddingMode.ISO10126 })
            {
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, rijAlg.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd(); // Read the decrypted bytes from the decrypting stream and place them in a string.
                        }
                    }
                }
            }
            pwdGen.Reset();
            return plaintext;
        }
        public static string GetBase64sCryptString(string SaltSource, string StringToEncrypt, int memoryCost)
        {
            byte[] derivedBytes = new byte[128];
            byte[] Salt = Encoding.ASCII.GetBytes(SaltSource);

            SCrypt.ComputeKey(Encoding.ASCII.GetBytes(StringToEncrypt), (new Rfc2898DeriveBytes(SaltSource, Salt, 10000)).GetBytes(32), (memoryCost != 0 ? memoryCost : 8192), 8, 1, null, derivedBytes);
            return Convert.ToBase64String(derivedBytes);
        }

        #region IDisposable
        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RijndaelEncryption() { Dispose(false); }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                pwdGen.Dispose();

            _disposed = true;
        }
        #endregion
    }
}
