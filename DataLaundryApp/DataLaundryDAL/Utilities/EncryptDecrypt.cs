using System.IO;
using System.Security.Cryptography;

namespace DataLaundryDAL.Utilities
{
    public class EncryptDecrypt
    {
        private RijndaelManaged CreateAes256Algorithm()
        {
            return new RijndaelManaged { KeySize = 256, BlockSize = 128 };
        }

        public byte[] GenerateKey()
        {
            using (var aes = CreateAes256Algorithm())
            {
                aes.GenerateKey();
                return aes.Key;
            }
        }

        public byte[] EncryptStringToBytes(byte[] key, string plainText)
        {
            byte[] encrypted;
            byte[] iv;

            // Create an RijndaelManaged object with the specified key. 
            using (var aes = CreateAes256Algorithm())
            {
                aes.Key = key;

                iv = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76, 0x12, 0x71, 0x20 };

                // Create a encrytor to perform the stream transform.
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, iv);

                // Create the streams used for encryption. 
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        // convert stream to bytes
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        public string DecryptBytesToString(byte[] key, byte[] data)
        {
            byte[] iv;
            iv = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76, 0x12, 0x71, 0x20 };
            byte[] cipherText = data;

            // Declare the string used to hold the decrypted text. 
            string plaintext;

            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (var aes = CreateAes256Algorithm())
            {
                aes.Key = key;
                aes.IV = iv;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                // Create the streams used for decryption. 
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream 
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}