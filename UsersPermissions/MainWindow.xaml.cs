using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;


namespace UserPermissions
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string UserNameSettings = "./UserSettings.xml"; //файл с настройками разрешений пользователям использовать приложение CloseOpenFilesApp
        string KeyFile = "./confiq.ini";

        public MainWindow()
        {

            InitializeComponent();

        }


        private void PermissionsWindow_Initialized(object sender, EventArgs e)
        {
            XDocument userXml = XDocument.Load(UserNameSettings);
            XElement users = userXml.Element("Users");
            string domainName = users.Element("DomainName").Value;

            if (isDomainUser(domainName, Environment.UserName) && isAdmin())
            {
                FormListBox(users);
            }
            else
            {
                CustomMessageBox.Show("Откройте программу от имени доменного администратора (с повышенными правами).", "Ошибка доступа");
                PermissionsWindow.Close();
                return;
            }
        }


        private void AddUserTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (AddUserTextBox.Text == "Введите имя пользователя")
            {
                AddUserTextBox.Text = "";
                AddUserTextBox.Foreground = Brushes.Black;
            }

        }

        private void AddButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            XDocument userXml = XDocument.Load(UserNameSettings);
            XElement users = userXml.Element("Users");
            string domainName = users.Element("DomainName").Value;
            string domain = domainName.Split('.')[0];
   

            string inputUserName = AddUserTextBox.Text.Replace(';', ',');
            List<string> userNamesList = inputUserName.Split(',').ToList();

            foreach (string userName in userNamesList)
            {

                string trimUserName = userName.Trim(' ').ToLower();

                if (!isDomainUser(domainName, trimUserName))
                {
                    CustomMessageBox.Show($"Пользователь \"{trimUserName}\" не найден!", "Неверный ввод");
                    continue;
                }

                if (users != null)
                {

                    XElement newUser = userXml.Descendants("User").FirstOrDefault(x => x.Attribute("name")?.Value.ToLower() == trimUserName);

                    if (newUser != null)
                    {
                        CustomMessageBox.Show($"Пользователь \"{trimUserName}\" уже имеет разрешение!", "Неверный ввод");
                        continue;
                    }

                    newUser = new XElement("User", new XAttribute("name", trimUserName));

                    users.Add(newUser);

                    NTAccount accountName = new NTAccount(domain + @"\" + trimUserName);

                    AddReadPermission(accountName);

                    userXml.Save(UserNameSettings);
                }

            }

            FormListBox(users);

            AddUserTextBox.Text = "Введите имя пользователя";
            AddUserTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBDBDBD"));


        }

        private void FormListBox(XElement xmlList)
        {

            ListUser.Items.Clear();

            if (xmlList != null)
            {
                foreach (XElement user in xmlList.Elements("User"))
                {
                    ListUser.Items.Add(user.Attribute("name").Value);
                }
            }


        }

        private void AddReadPermission(NTAccount account)
        {
            FileSecurity fileSecurity = File.GetAccessControl(KeyFile);
            fileSecurity.AddAccessRule(new FileSystemAccessRule(account, FileSystemRights.Read, AccessControlType.Allow));
            File.SetAccessControl(KeyFile, fileSecurity);
        }

        private void RemoveReadPermission(NTAccount account)
        {
            FileSecurity fileSecurity = File.GetAccessControl(KeyFile);
            fileSecurity.RemoveAccessRule(new FileSystemAccessRule(account, FileSystemRights.Read, AccessControlType.Allow));
            File.SetAccessControl(KeyFile, fileSecurity);
        }

        private bool isDomainUser(string domainName, string userName)
        {

            List<string> domainParts = domainName.Split('.').ToList();
            string dirrEntryString = @"LDAP://" + domainName + "/";

            foreach (string part in domainParts)
            {
                dirrEntryString += $"DC={part},";
            }

            dirrEntryString = dirrEntryString.TrimEnd(',');

            DirectoryEntry deDomain = new DirectoryEntry(dirrEntryString);
            DirectorySearcher search = new DirectorySearcher(deDomain);
            search.Filter = String.Format("(SAMAccountName={0})", userName);
            search.PropertiesToLoad.Add("cn");
            SearchResult result = search.FindOne();

            if (result != null) { return true; } else { return false; }
        }

        public static bool isAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void PermissionsWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            string textString = AddUserTextBox.Text;
            textString = textString.Replace(" ", "");

            if (textString == "")
            {
                AddUserTextBox.Text = "Введите имя пользователя";
                AddUserTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBDBDBD"));
            }
        }




        private void DelButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            XDocument userXml = XDocument.Load(UserNameSettings);
            XElement users = userXml.Element("Users");
            string domainName = users.Element("DomainName").Value;
            string domain = domainName.Split('.')[0];

            List<string> selectedItems = new List<string>();
            foreach (string item in ListUser.SelectedItems)
            {
                selectedItems.Add(item);
            }

            foreach (string item in selectedItems)
            {
                if (users != null)
                {
                    var delUser = users.Elements("User")
                        .FirstOrDefault(p => p.Attribute("name")?.Value == item);

                    if (delUser != null)
                    {
                        NTAccount accountName = new NTAccount(domain + @"\" + item);

                        RemoveReadPermission(accountName);

                        delUser.Remove();

                    }

                }
            }

            userXml.Save(UserNameSettings);

            FormListBox(users);

        }

        private void CloseButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PermissionsWindow.Close();
        }

        private void AddUserTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MouseButtonEventArgs mouseEvent = null;

                AddButton_PreviewMouseLeftButtonUp(sender, mouseEvent);
                Keyboard.ClearFocus();
            }

        }

    }
}
