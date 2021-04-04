using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Dynamic;
using System.Net;
using Flight_Center_Project_FinalExam_BL;
using System.Security.Claims;
using Flight_Center_Project_FinalExam_DAL;

namespace Web_Api_interface
{
    //enumerartion for setting up the method
    //"GetUniqueKeyOriginal_BIASED"
    public enum Charset
    {
        OnlyNumber,
        OnlyUpperCaseLetters,
        OnlyLowerCaseLetters,
        AllAviliableChars
    }

    static class Statics
    {

        public static string Base64ImageAbsenceImage = "data:image/jpeg;base64,/9j/4QAYRXhpZgAASUkqAAgAAAAAAAAAAAAAAP/sABFEdWNreQABAAQAAAA8AAD/4QMpaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjAtYzA2MCA2MS4xMzQ3NzcsIDIwMTAvMDIvMTItMTc6MzI6MDAgICAgICAgICI+IDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+IDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOnhtcD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDUzUgV2luZG93cyIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDpCRTFENTcxOTNBNTkxMUVCQjE3M0M2MzI5ODI2ODE2RSIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDpCRTFENTcxQTNBNTkxMUVCQjE3M0M2MzI5ODI2ODE2RSI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOkJFMUQ1NzE3M0E1OTExRUJCMTczQzYzMjk4MjY4MTZFIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOkJFMUQ1NzE4M0E1OTExRUJCMTczQzYzMjk4MjY4MTZFIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+/+4ADkFkb2JlAGTAAAAAAf/bAIQABgQEBAUEBgUFBgkGBQYJCwgGBggL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oq76r8ZVLaU4gIJDWbvV2Y1B5ZOqzsnDdDl3hrXHKIIBw7m+g7/iSrdKOQ3iQPiSrdKOQ3iQPiSrdKOQ3iQPiSrdKOQ3iQPiSrdKOQ3iQPiSrdKOQ3iQPiSrdKOQ3iQPiSrdKOQ3iQPiSrdKOQ3iQeWdqE1Oua6YcHFgsbYAMHkQauqXaJjMGNBToCAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICDrmezxcx2JcJN3XY65dzXZq+S/f/qP4KieLucry7i73KtP257iHo4lv+D6Ku+q/GVS2lOICDxTdGkJuNz0dhdEIAtDiMA8SDp+G6T0R5buNA+G6T0R5buNA+G6T0R5buNA+G6T0R5buNA+G6T0R5buNA+G6T0R5buNA+G6T0R5buNA+G6T0R5buNA+G6T0R5buNA+G6T0R5buNA+G6T0R5buNB6JKlyUk5zpdhaXix1pJweVB60BAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBB1zPZ4uY7EuEm7rsdcu5rs1fJfv/1H8FRPF3OV5dxd7lbOqWttJpVJZAjvBiEAkAjBgX7/ABvkooYtPb7tfy1vCebgpoNPb7tfy29o1Cv0gtC+QtjKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpjaNQr9IJfITKqY2jUK/SCXyEyqmNo1Cv0gl8hMqpnGJ+4dCfDczKsygRbaN8WL57vNw66ejj7/tNNrprp6/tDdfk+et55tnXudtt+pzVmVwqd/29nr++P1/j0Rn/AEx/23tOr6/x/V//2Q==";

        private static Random _rnd = new Random();

        public static List<DataType> ShuffleList<DataType>(List<DataType> inputList)
        {
            List<DataType> randomList = new List<DataType>();

            Random r = new Random();
            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = r.Next(0, inputList.Count - 1); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return randomList; //return the new random list
        }

        public static string ReadFromUrl(string url)
        {
            string read_str = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            // read data
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                read_str = reader.ReadToEnd();
            }
            return read_str;
        }







        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
        static public string GetUniqueKeyOriginal_BIASED(int size)
        {
            return GetUniqueKeyOriginal_BIASED_Internal(size, Charset.AllAviliableChars);
        }
        static public string GetUniqueKeyOriginal_BIASED(int size, Charset charset)
        {
            return GetUniqueKeyOriginal_BIASED_Internal(size, charset);
        }
        static private string GetUniqueKeyOriginal_BIASED_Internal(int size, Charset charset)
        {
            char[] chars = null;

            if (charset == Charset.AllAviliableChars) chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            if (charset == Charset.OnlyLowerCaseLetters) chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            if (charset == Charset.OnlyUpperCaseLetters) chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            if (charset == Charset.OnlyNumber) chars = "1234567890".ToCharArray();

            byte[] data = new byte[size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        static public string DashingString(string input, int num)
        {
            int globalCount = 0;
            int dashingCount = 0;
            string output = string.Empty;
            int count = 0;
            foreach(var s in input)
            {
                count++;
                globalCount++;
                output += s;
                if(count == num)
                {
                    dashingCount++;
                    if(globalCount != input.Length) output += "-";
                    count = 0;
                    
                }
            }
            return output;
        }

        public static Stream GetStreamFromUrl(string url)
        {
            byte[] imageData = null;

            using (var wc = new System.Net.WebClient())
                imageData = wc.DownloadData(url);

            return new MemoryStream(imageData);
        }

        public static DateTime GetRandomDate(DateTime from, DateTime to)
        {
            TimeSpan range = new TimeSpan(to.Ticks - from.Ticks);
            return from + new TimeSpan((long)(range.Ticks * _rnd.NextDouble()));
        }








        /// <summary>
        /// This method compares a two Poco objects of the same type by all the properties except ID
        /// </summary>
        /// <typeparam name="T">type of the objects</typeparam>
        /// <param name="o1">The first compared object</param>
        /// <param name="o2">The second compared object</param>
        /// <returns></returns>
        public static bool BulletprofComparsion<T>(T o1, T o2) where T : IPoco
        {
            bool isEqual = true;
            for (int i = 0; i < typeof(T).GetProperties().Length; i++)
            {
                if (typeof(T).GetProperties()[i].Name.Equals("ID".ToUpper())) continue;
                var value1 = typeof(T).GetProperties()[i].GetValue(o1);
                var value2 = typeof(T).GetProperties()[i].GetValue(o2);
                

                if (!value1.Equals(value2))
                {
                    isEqual = false;
                    break;
                }
            }
            return isEqual;
        }

    }





}
