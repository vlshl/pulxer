using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public class DataProtect
    {
        private const byte ENTROPY_SIZE = 32;
        private static byte[] _salt = {
            0xae, 0x9a, 0xe7, 0x88, 0x61, 0xbb, 0x94, 0x55,
            0xf0, 0x82, 0xd2, 0xca, 0x44, 0x10, 0xe3, 0x88,
            0x7d, 0x95, 0x78, 0xe9, 0xbe, 0x8b, 0xb8, 0xda,
            0x6c, 0x60, 0x20, 0x25, 0x69, 0x10, 0x8e, 0xbc };

        public DataProtect()
        {
        }

        /// <summary>
        /// Зашифровка данных
        /// </summary>
        /// <param name="str">Строка данные</param>
        /// <param name="isLocalMachineProtection">Уровень защиты (на уровне компьютера или на уровне пользователя)</param>
        /// <returns>Результат в base64</returns>
        public static string Protect(string str, bool isLocalMachineProtection = false)
        {
            if (str == null)
                throw new ArgumentNullException();

            byte[] entropy = new byte[ENTROPY_SIZE];
            new Random().NextBytes(entropy);

            byte[] fullEntropy = new byte[_salt.Length + ENTROPY_SIZE];
            Array.Copy(entropy, 0, fullEntropy, 0, ENTROPY_SIZE);
            Array.Copy(_salt, 0, fullEntropy, ENTROPY_SIZE, _salt.Length);

            byte[] data = Encoding.UTF8.GetBytes(str);
            byte[] encryptedData = ProtectedData.Protect(data, fullEntropy,
                isLocalMachineProtection ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser);

            List<byte> resList = new List<byte>();
            resList.Add((byte)(isLocalMachineProtection ? 0x00 : 0xff));
            resList.AddRange(entropy);
            resList.AddRange(encryptedData);

            return Convert.ToBase64String(resList.ToArray());
        }

        /// <summary>
        /// Расшифровка данных подключения к серверу
        /// </summary>
        /// <param name="protectedStr">Зашифрованные данные в base64</param>
        /// <param name="str">Расшифрованные данные</param>
        /// <returns>true - успешно, false - ошибка</returns>
        public static bool UnProtect(string protectedStr, out string str)
        {
            if (protectedStr == null)
                throw new ArgumentNullException();

            str = "";

            try
            {
                byte[] bytes = Convert.FromBase64String(protectedStr);
                if (bytes == null || bytes.Length < ENTROPY_SIZE + 2) return false; // первый байт + энтропия + еще хотя бы один байт

                bool isLocalMachineProtection = bytes[0] == 0x00;
                byte[] fullEntropy = new byte[_salt.Length + ENTROPY_SIZE];
                Array.Copy(bytes, 1, fullEntropy, 0, ENTROPY_SIZE);
                Array.Copy(_salt, 0, fullEntropy, ENTROPY_SIZE, _salt.Length);

                byte[] encData = new byte[bytes.Length - 1 - ENTROPY_SIZE]; // первый байт и энтропию убираем, остается хотя бы еще один байт
                Array.Copy(bytes, ENTROPY_SIZE + 1, encData, 0, encData.Length);

                byte[] decData = ProtectedData.Unprotect(encData, fullEntropy,
                    isLocalMachineProtection ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser);
                if (decData == null || decData.Length == 0)
                    return false;

                string dataStr = Encoding.UTF8.GetString(decData);
                if (dataStr == null || dataStr.Length == 0)
                    return false;

                str = dataStr;
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Входная строка может быть как зашифрованной, так и нет.
        /// Если строка зашифрована, она расшифровывается.
        /// Иначе возвращяется исходная строка.
        /// </summary>
        /// <param name="inStr">Входная строка</param>
        /// <returns>Результат расшифровки</returns>
        public static string TryUnProtect(string inStr)
        {
            string outStr = "";
            bool isSuccess = DataProtect.UnProtect(inStr, out outStr);

            return isSuccess ? outStr : inStr;
        }

        /// <summary>
        /// По исходному текстовому файлу формируется новый файл, который содержит все строки старого файла в защищенном виде.
        /// </summary>
        /// <param name="path">Путь к исходному текстовому файлу</param>
        /// <param name="newFileExt">Расширение у файла с результатом. Добавляется к имени и расширению исходного файла.
        /// (т.е. у исходного файла расшинение не отбразывается)</param>
        /// <param name="isLocalMachineProtection">Уровень защиты (на уровне компьютера или на уровне пользователя)</param>
        /// <returns>Путь к файлу результата</returns>
        public static string FileProtect(string path, string newFileExt, bool isLocalMachineProtection = false)
        {
            string newPath = "";
            try
            {
                List<string> protectLines = new List<string>();
                var lines = File.ReadAllLines(path);

                foreach (string line in lines)
                {
                    protectLines.Add(DataProtect.Protect(line, isLocalMachineProtection));
                }

                newPath = path + (newFileExt.StartsWith('.') ? newFileExt : "." + newFileExt);
                File.WriteAllLines(newPath, protectLines);
            }
            catch(Exception ex)
            {
                throw new Exception("Ошибка при обработке файла.", ex);
            }

            return newPath;
        }
    }
}
