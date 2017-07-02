using System;
using System.IO;
using System.Security.Cryptography;

namespace Quest.Lib.Utils
{
    public static class Crypto
    {
        private static byte[] MakeIV()
        {
            byte[] IV = {65, 1, 2, 23, 4, 5, 6, 7, 32, 21, 10, 11, 12, 13, 84, 45};
            return IV;
        }

        private static byte[] MakeKey()
        {
            var key = new byte[]
            {
                13, 7, 7, 29, 42, 97, 99, 74, 24, 98, 5, 46, 27, 79, 65, 20, 92, 93, 77, 72, 56, 44, 21, 64, 30, 73, 46, 76,
                1, 74, 98, 66
            };
            return key;
        }

        public static string Encrypt(string plainText)
        {
            if (plainText == null || plainText.Length == 0)
                plainText = " ";

            using (var myAes = Aes.Create())
            {
                myAes.Key = MakeKey();
                myAes.IV = MakeIV();

                // Encrypt the string to an array of bytes.
                var encrypted = EncryptStringToBytes_Aes(plainText, myAes.Key, myAes.IV);

                return Convert.ToBase64String(encrypted);
            }
        }

        public static string Dencrypt(string encrypted)
        {
            using (var myAes = Aes.Create())
            {
                myAes.Key = MakeKey();
                myAes.IV = MakeIV();

                // convert hex to byte array
                var data = Convert.FromBase64String(encrypted);

                // Decrypt the bytes to a string.
                var decoded = DecryptStringFromBytes_Aes(data, myAes.Key, myAes.IV);

                return decoded;
            }
        }


        private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an AesManaged object 
            // with the specified key and IV. 
            using (var aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Padding = PaddingMode.PKCS7;

                // Create a decrytor to perform the stream transform.
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

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
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream. 
            return encrypted;
        }

        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold 
            // the decrypted text. 
            string plaintext = null;

            // Create an AesManaged object 
            // with the specified key and IV. 
            using (var aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

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