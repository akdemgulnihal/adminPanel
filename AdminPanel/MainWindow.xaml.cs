using Firebase.Database;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Firebase.Database.Query;
using System.Linq;
using System.Windows.Threading;
using System.Windows.Input;

namespace AdminPanel
{

  
    public static class FirebaseConfig
    {
        // Firebase URL'sini burada tanımlıyoruz
        public static readonly string FirebaseUrl = "https://adminpanel-a66ab-default-rtdb.firebaseio.com/";

    }

    public static class FirebaseService
    {
        // Firebase URL'si
        public static readonly string FirebaseUrl = FirebaseConfig.FirebaseUrl;

        // HttpClient'i global olarak başlatıyoruz
        public static readonly HttpClient Client = new HttpClient();
    }

    public class UsersData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string LoginStatus { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Age { get; set; }
        public string Email { get; set; }
        public string Birthday { get; set; }
        public string University { get; set; }
        public string Experience { get; set; }
    }


    //Admin ve User Login için ortak olan fonksiyonlar
    public static class LoginService
    {
        public static async Task<bool> LoginAsync(string username, string password, string tableName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Firebase'den kullanıcı verisini al
                    string url = FirebaseConfig.FirebaseUrl + $"{tableName}.json";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        var usersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                        foreach (var item in usersData)
                        {
                            if (item.Value.Username == username && item.Value.Password == password)
                            {
                                // Eğer giriş başarılıysa ve LoginStatus 'Passive' ise, 'Active' olarak güncelle
                                if (item.Value.LoginStatus == "Passive")
                                {
                                    var firebase = new FirebaseClient(FirebaseConfig.FirebaseUrl);
                                    var userRef = firebase.Child(tableName).Child(item.Key);

                                    var dataToSend = new Dictionary<string, object>
                                {
                                    {"Username", item.Value.Username},
                                    {"Password", item.Value.Password},
                                    {"LoginStatus", "Active"},
                                    {"Name", item.Value.Name},
                                    {"Surname", item.Value.Surname},
                                    {"Age", item.Value.Age},
                                    {"Email", item.Value.Email},
                                    {"Birthday", item.Value.Birthday},
                                    {"University", item.Value.University},
                                    {"Experience", item.Value.Experience}
                                };

                                    await userRef.PutAsync(dataToSend);
                                    return true; // Giriş başarılı
                                }
                                else
                                {
                                    // Eğer LoginStatus 'Active' ise, kullanıcıyı uyar
                                    MessageBox.Show("Birden fazla cihazda aynı anda oturum açmak mümkün değildir. Lütfen önce mevcut oturumu kapatın ve tekrar deneyin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return false;
                                }
                            }
                        }

                        MessageBox.Show("Geçersiz kullanıcı adı veya şifre", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    else
                    {
                        MessageBox.Show("Veri alınırken hata oluştu: " + response.StatusCode, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Common GotFocus event handler
        public static void ClearTextOnFocus(object sender, RoutedEventArgs e, string defaultText)
        {
            if (sender is TextBox textBox && textBox.Text == defaultText)
            {
                textBox.Clear(); // Clear default text in TextBox
            }
            else if (sender is PasswordBox passwordBox && passwordBox.Password == defaultText)
            {
                passwordBox.Clear(); // Clear default password in PasswordBox
            }
        }

        // Common LostFocus event handler
        public static void RestoreTextOnLostFocus(object sender, RoutedEventArgs e, string defaultText)
        {
            if (sender is TextBox textBox && string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = defaultText; // Restore default text in TextBox
            }
            else if (sender is PasswordBox passwordBox && string.IsNullOrEmpty(passwordBox.Password))
            {
                passwordBox.Password = defaultText; // Restore default password in PasswordBox
            }
        }

    }

    public static class HomePageHelper
    {
        public static void StartRefreshTimer(DispatcherTimer timer, EventHandler tickEventHandler, TimeSpan interval)
        {
            timer.Interval = interval;
            timer.Tick += tickEventHandler;
            timer.Start();
        }

        public static async Task<List<UsersData>> LoadUserDataAsycn(string tableName)
        {
            try
            {
                var firebase = new FirebaseClient(FirebaseService.FirebaseUrl);
                var userRef = firebase.Child(tableName);

                //  StandartUserTable'daki userların hepsini alıyot
                var users = await userRef.OnceAsync<UsersData>();

                // DataGrid'de gösterilmek icin Liste olusturuldu
                var userList = users.Select(u => new UsersData
                {
                    Username = u.Object.Username,
                    Name = u.Object.Name,
                    Surname = u.Object.Surname,
                    //Age = u.Object.Age,
                    LoginStatus = u.Object.LoginStatus,
                    Password = u.Object.Password
                }).ToList();

                // Bind the user list to the DataGrid
                return userList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri alınırken bir hata oluştu: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        // Window Drag and Maximize Logic
        public static void HandleWindowDragAndMaximize(object sender, MouseButtonEventArgs e, ref bool isMaximized)
        {
            if (e.ClickCount == 2)
            {
                Window parentWindow = Window.GetWindow(sender as Page);
                if (parentWindow != null)
                {
                    if (isMaximized)
                    {
                        parentWindow.WindowState = WindowState.Normal;
                        parentWindow.Width = 1080;
                        parentWindow.Height = 720;
                        isMaximized = false;
                    }
                    else
                    {
                        parentWindow.WindowState = WindowState.Maximized;
                        isMaximized = true;
                    }
                }
            }
        }

        public static void HandleWindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Window parentWindow = Window.GetWindow(sender as Window);
                if (parentWindow != null)
                {
                    parentWindow.DragMove();
                }
            }
        }


    }
    




    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AdminLoginPage adminLoginPage = new AdminLoginPage(); // Create an instance of AdminLoginPage
            adminLoginPage.Show();


            // AdminLoginPage Frame yükleme 
            //MainFrame.Navigate(new AdminLoginPage());

            // Close the MainWindow
            this.Close();
        }
     

    }
}
