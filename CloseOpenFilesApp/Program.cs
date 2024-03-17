using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using Path = System.IO.Path;

namespace CloseOpenFilesApp
{
    internal class Program
    {
        static string progLocation = Assembly.GetExecutingAssembly().Location;
        static string confiqPath = Path.GetDirectoryName(progLocation) + @"\confiq.ini"; //файл с ключем для пароля, хранится в месте установки программы, доступ для чтение данного файла настраивается путем добавления имени пользователя в настройки UserPermission
        static string systemAddress = ConfigurationManager.AppSettings.Get("SystemAddress");


        [STAThread]
        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                CloseOpenFile(args.First());
            }

        }

        static void CloseOpenFile(string path)
        {

            if (!IsFileRead(confiqPath))
            {
                return;
            }

            List<string> textLines = File.ReadLines(confiqPath).ToList();
            string key = textLines.First();
            string pass = "mEOre/40GZBCPjpNzO+wrA=="; //зашифрованный пароль системного сетевого пользователя (доменного администратора)

            string netPath;

            FileInfo fileInfo = new FileInfo(path);

            DriveInfo drive = null;

            try
            {
                drive = new DriveInfo(path);
            }
            catch { }


            if (drive == null || drive.DriveType != DriveType.Network)
            {
                Console.WriteLine("Разблокировка доступна только для сетевых файлов.");
                CustomMessageBox.Show($"Разблокировка доступна только для сетевых файлов.", "Ошибка");
                return;
            }

            netPath = string.Concat(@"C:\volume1\", drive.VolumeLabel, @"\", path.Replace(Path.GetPathRoot(path), ""));

            string domain = Environment.UserDomainName;

            if (IsFileAllowed(path) && IsFileLocked(fileInfo))
            {
                Process process = new Process();
                ProcessStartInfo psi = new ProcessStartInfo();

                Directory.SetCurrentDirectory(Environment.GetEnvironmentVariable("public"));

                psi.FileName = @"cmd.exe";
                psi.UseShellExecute = false;
                psi.Arguments = string.Format("/c openfiles /disconnect /s {2} /Op {0}{1}{0} /a *", @"""", netPath, systemAddress);
                psi.Domain = domain;
                psi.UserName = "synadm"; //сетевой пользователь (доменный администратор)
                psi.CreateNoWindow = true;

                SecureString secPassword = new SecureString();
                foreach (char c in DecryptStringFromBytes(pass, key))
                {
                    secPassword.AppendChar(c);
                }
                psi.Password = secPassword;

                process.StartInfo = psi;

                process.Start();
                process.WaitForExit();
                Console.WriteLine($"Файл '{Path.GetFileName(path)}' успешно разблокирован");
                CustomMessageBox.Show($"\"{Path.GetFileName(path)}\" успешно разблокирован!", "Готово");
            }

        }


        static bool IsFileAllowed(string filePath)
        {

            FileSecurity security = File.GetAccessControl(filePath);
            AuthorizationRuleCollection rules = security.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.AccessControlType == AccessControlType.Allow)
                {
                    if ((FileSystemRights.Write & rule.FileSystemRights) == FileSystemRights.Write)
                    {
                        Console.WriteLine("Текущий пользователь имеет права на запись в файл '{0}'.", filePath);

                        return true;
                    }
                }
            }

            Console.WriteLine("Текущий пользователь не имеет прав на запись в файл '{0}'.", filePath);
            CustomMessageBox.Show($"Вы не имеете разрешения на редактирование файла \"{Path.GetFileName(filePath)}\".", "Ошибка доступа");
            return false;
        }

        static bool IsFileRead(string filePath)
        {
            FileSecurity security = null;

            try { security = File.GetAccessControl(filePath); }
            catch { goto canceled; }
           
            AuthorizationRuleCollection rules = security.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.FileSystemRights.HasFlag(FileSystemRights.Read) &&
                    rule.IdentityReference.Value.Equals(System.Security.Principal.WindowsIdentity.GetCurrent().Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("Текущий пользователь имеет права на чтение файла '{0}'.", filePath);
                    return true;
                }
            }

            canceled:

            Console.WriteLine("Текущий пользователь не имеет права использовать программу.");
            CustomMessageBox.Show($"Вам не разрешено использование данной функции.", "Ошибка доступа");
            return false;
        }

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                Console.WriteLine("Файл '{0}' заблокирован другим пользователем.", file);
                return true;
            }
            finally
            {
                stream?.Close();
            }
            Console.WriteLine("Файл '{0}' не используется.", file);
            CustomMessageBox.Show($"\"{file.Name}\" не используется.", "Готово");
            return false;
        }

        static string DecryptStringFromBytes(string cryptPass, string key)
        {
            // Представляем ключ и вектор инициализации в виде байтовых массивов
            byte[] keyBytes = Convert.FromBase64String(key);
            byte[] cipherText = Convert.FromBase64String(cryptPass);

            // Создаем экземпляр класса Aes
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = new byte[16];
                // Создаем объект расшифратора
                ICryptoTransform decryptor = aes.CreateDecryptor();

                // Расшифровываем входные данные
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }


    }
}
