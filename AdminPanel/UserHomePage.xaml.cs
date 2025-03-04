using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Linq;
using System.Windows.Threading;

namespace AdminPanel
{
    /// <summary>
    /// Interaction logic for UserHomePage.xaml
    /// </summary>
    public partial class UserHomePage : Window  // Changed from Page to Window
    {
        private DispatcherTimer _refreshTimer;

        //private int currentPage = 1;
        //private int pageSize = 15;
        //private int totalPages = 1;
        //private List<UsersData> UserList;
        private string _username;

        // Constructor
        public UserHomePage(string username)
        {
            InitializeComponent();
            _username = username;
            LoadUserData();

            StartRefreshTimer();  // Start the page refresh
        }

        private void StartRefreshTimer()
        {
            // Start a DispatcherTimer that triggers every 5 seconds
            _refreshTimer = new DispatcherTimer();
            HomePageHelper.StartRefreshTimer(_refreshTimer, RefreshTimer_Tick, TimeSpan.FromSeconds(5));
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            // Refresh the data when the timer ticks
            LoadUserData();
        }

        private async void LoadUserData()
        {
            var userList = await HomePageHelper.LoadUserDataAsycn("StandartUserTable");

            if (userList != null)
            {
                userDataGrid.ItemsSource = userList;
            }
            else
            {
                MessageBox.Show("Veri alınırken bir hata oluştu.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HomePageHelper.HandleWindowDrag(sender, e);
        }

        private bool isMaximized = false;

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HomePageHelper.HandleWindowDragAndMaximize(sender, e, ref isMaximized);
        }

        // Logout button click event handler
        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = FirebaseService.FirebaseUrl + "/StandartUserTable.json";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        var usersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                        foreach (var item in usersData)
                        {
                            if (item.Value.Username == _username && item.Value.LoginStatus == "Active")
                            {
                                var firebase = new FirebaseClient(FirebaseService.FirebaseUrl);
                                var userRef = firebase.Child("StandartUserTable").Child(item.Key);

                                var dataToSend = new Dictionary<string, object>
                                {
                                    { "Username", item.Value.Username },
                                    { "Password", item.Value.Password },
                                    { "Name", item.Value.Name },
                                    { "Surname", item.Value.Surname },
                                    { "Age", item.Value.Age },
                                    { "Email", item.Value.Email },
                                    { "Birthday", item.Value.Birthday },
                                    { "University", item.Value.University },
                                    { "Experience", item.Value.Experience },
                                    { "LoginStatus", "Passive" }
                                };

                                await userRef.PutAsync(dataToSend);
                                break;
                            }
                        }

                        // Navigate to the UserLoginPage (you need to instantiate and show the LoginPage as a new window)
                        var loginPage = new UserLoginPage();
                        loginPage.Show();  // Show the LoginPage as a new window
                        this.Close();  // Close the current window (UserHomePage)
                    }
                    else
                    {
                        MessageBox.Show("Veri alınırken hata oluştu: " + response.StatusCode, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Edit user profile click event handler
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Edit user: {_username}");

            try
            {
                var editUserWindow = new UserProfileUpdate(_username);
                editUserWindow.Show();  // Open the UserProfileUpdate window

                editUserWindow.UserEdit += (s, args) =>
                {
                    LoadUserData();  // Refresh the data after user profile update
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
            }
        }
    }
}
