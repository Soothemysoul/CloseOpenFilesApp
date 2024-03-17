using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace CloseOpenFilesApp
{
    public class ProgGen
    {
        /// <summary>
        /// Генерация секретного ключа для шифровки/дешифровки пароля
        /// </summary>

        public static void GenerateKey()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();

                byte[] key = aes.Key;
                Console.WriteLine($"Secret key: {Convert.ToBase64String(key)}");

                //string filePath = @"путь_для_сохранения_ключа\confiq.ini"; // указываем путь и имя файла
                //File.WriteAllText(filePath, Convert.ToBase64String(key));
            }
        }

        /// <summary>
        /// Получение зашифрованного пароля
        /// </summary>
        /// <param name="pass">Строка зашифрованного пароля</param>
        public static void CryptPass(string pass)
        {
            string key = @"mTGCFIA5Uru9bWnnNXXby4HBpQu/FisqJ5mNExEhSt4=";
            byte[] encryptedPassword = EncryptStringToBytes(pass, key);
            Console.WriteLine(Convert.ToBase64String(encryptedPassword));
        }


        /// <summary>
        /// Методы для шифрования пароля используя секретный ключ
        /// </summary>
        /// <param name="plainText">Незашифрованный пароль</param>
        /// <param name="key">Секретный ключ шифрования</param>
        /// <returns>Зашифрованный пароль в виде byte[]</returns>
        static byte[] EncryptStringToBytes(string plainText, string key)
        {
            // Представляем ключ и вектор инициализации в виде байтовых массивов
            byte[] keyBytes = Convert.FromBase64String(key);

            // Создаем экземпляр класса Aes
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV=new byte[16];

                // Создаем объект шифратора
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Шифруем входные данные
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        
    }
}
