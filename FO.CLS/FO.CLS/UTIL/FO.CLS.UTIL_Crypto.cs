using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace FO.CLS.UTIL
{
    public class FOCrypto
    {
        #region 변수 및 상수

        private const int DES_KEY_LENGTH = 8;

        public static string DES_KEY_VALUE = "FOURONE.";

        #endregion


        #region 메서드

        /// <summary>
        /// 평문과 키를 받아서 DES 방식으로 암호화
        /// </summary>
        /// <param name="endcryText">평문</param>
        /// <param name="key">암호화 키</param>
        /// <returns>성공 : DES 방식의 암호화 문자 실패 : 공백 </returns>
        private static string EncryptoDES(string endcryText, byte[] key)
        {
            if(string.IsNullOrEmpty(endcryText))
            {
                string errorMessage = "EncryptoDes endcryText is Null or Empty.";

                throw new Exception(errorMessage);
            }

            if(key.Length != DES_KEY_LENGTH)
            {
                string errorMessage = "EncryptoDes key Length not 8.";

                throw new Exception(errorMessage);
            }

            try
            {
                DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateEncryptor(key, key), CryptoStreamMode.Write);
                StreamWriter streamWriter = new StreamWriter(cryptoStream);

                streamWriter.Write(endcryText);
                streamWriter.Flush();

                cryptoStream.FlushFinalBlock();
                streamWriter.Flush();

                string data = Convert.ToBase64String(memoryStream.GetBuffer(), 0, Convert.ToInt32(memoryStream.Length));

                streamWriter.Dispose();
                cryptoStream.Dispose();
                memoryStream.Dispose();
                dESCryptoServiceProvider.Dispose();

                return data;
            }
            catch(Exception ex)
            {
                string errorMessage = "Fourone.UtilEncry to Des Error -> " + ex.Message;

                MessageBox.Show(errorMessage, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return string.Empty;
            }
        }
        /// <summary>
        /// 평문과 키를 받아서 DES 알고리즘으로 암호화
        /// </summary>
        /// <param name="encryText">평문</param>
        /// <param name="key">암호화 키</param>
        /// <returns>성공 : DES 암호화된 문자열, 실패 : 공백</returns>
        public static string EncryptoDES(string encryText, string key)
        {
            return EncryptoDES(encryText, Encoding.ASCII.GetBytes(key));
        }

        /// <summary>
        /// 암호문과 키를 받아서 DES 방식으로 복호화
        /// </summary>
        /// <param name="decryText">암호문</param>
        /// <param name="key">키</param>
        /// <returns>성공 : DES 복호화된 문자열, 실패 : 공백</returns>
        private static string DecrytoDES(string decryText, byte[] key)
        {
            if(string.IsNullOrEmpty(decryText))
            {
                string errorMessge = "DecrytoDes decryText is Null or Empty.";

                throw new Exception(errorMessge);
            }

            if(key.Length != DES_KEY_LENGTH)
            {
                string errorMessage = "DecrytoDesc decryText Key Length not 8.";

                throw new Exception(errorMessage);
            }

            try
            {
                DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
                MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(decryText));
                CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateDecryptor(key, key), CryptoStreamMode.Read);
                StreamReader streamReader = new StreamReader(cryptoStream);

                string data = streamReader.ReadToEnd();

                streamReader.Dispose();
                dESCryptoServiceProvider.Dispose();
                memoryStream.Dispose();
                cryptoStream.Dispose();

                return data;
            }
            catch(Exception ex)
            {
                string errorMessage = "Fourone.UtilDecry to Des Error -> " + ex.Message;

                MessageBox.Show(errorMessage, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return string.Empty;
            }
        }

        /// <summary>
        /// 평문과 키를 받아서 DES 방식으로 복호화
        /// </summary>
        /// <param name="decryText">암호문</param>
        /// <param name="key">키</param>
        /// <returns>성공 : DES 복호화된 문자열, 실패 : 공백</returns>
        public static string DecrytoDES(string decryText, string key)
        {
            return DecrytoDES(decryText, Encoding.ASCII.GetBytes(key));
        }

        #endregion
    }
}
