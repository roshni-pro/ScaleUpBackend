using System.Security.Cryptography;
using System.Text;

namespace ScaleUP.Services.LeadAPI.Helper
{
    public class eNachHdfcAesHelper
    {
        private byte[] Key = { 89, 83, 45, 236, 140, 228, 180, 79, 209, 164, 231, 131, 28, 7, 110, 73, 140, 235, 118, 52, 225, 46, 202, 118 };
        private byte[] IV = { 161, 200, 187, 207, 22, 92, 119, 227 };

        //public static string key_Val = AppConstants.eMandatekey;
        //public static string key_Val = "k2hLr4X0ozNyZByj5DT66edtCEee1x+6";
        //    public  static int count =  0;
        public static string DecryptAES(string encryptedValue,string key_Val)
        {
            if (string.IsNullOrEmpty(encryptedValue))
            {
                return encryptedValue;
            }

            //count = count + 1;
            //HDFC_Emandate.Common cmn = new HDFC_Emandate.Common();
            //cmn.ErrorLog_Text("DecryptAES" ,  count.ToString() +  ":" + encryptedValue  + ":");

            if (encryptedValue.Substring(0, 2) == "\\x")
            {
                encryptedValue = encryptedValue.Remove(0, 2);
            }

            byte[] key = Encoding.UTF8.GetBytes(key_Val);
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = key;
                aesAlg.Mode = CipherMode.ECB;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                MemoryStream msDecrypt = new MemoryStream(HexToByteArray(encryptedValue));
                CryptoStream csDecrypt = new System.Security.Cryptography.CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

                StreamReader srDecrypt = new System.IO.StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();

            }
        }
        public static string Encrypt(string plainText, string key_Val)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            byte[] Key = Encoding.UTF8.GetBytes(key_Val);
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.Mode = CipherMode.ECB;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return "\\x" + ByteArrayToString(outputStream.ToArray());

                }
            }
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        private static byte[] HexToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            int iPos = 0;
            for (int i = 0; i <= NumberChars - 1; i += 2)
            {
                bytes[iPos] = Convert.ToByte(hex.Substring(i, 2), 16);
                iPos += 1;
            }
            return bytes;
        }
        public static string DecryptStringAES(string cipherText)
        {

            var keybytes = Encoding.UTF8.GetBytes("8080808080808080");
            var iv = Encoding.UTF8.GetBytes("8080808080808080");

            var encrypted = Convert.FromBase64String(cipherText);
            var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
            return string.Format(decriptedFromJavascript);
        }
        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
                try
                {
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
                catch
                {
                    plaintext = "keyError";
                }
            }

            return plaintext;
        }
        public static string ComputeSha256Hash(string rawData)
        {
            //ComputeSha256Hash
            // Create a SHA256   
            using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

    }
}
