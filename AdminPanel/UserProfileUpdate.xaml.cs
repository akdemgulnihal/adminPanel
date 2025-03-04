using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;

namespace AdminPanel
{
    /// <summary>
    /// Interaction logic for UserProfileUpdate.xaml
    /// </summary>
    public partial class UserProfileUpdate : Window
    {
        public event EventHandler UserEdit;

        private string _username;
        private HttpClient client = new HttpClient();

        // Constructor that accepts username to load data
        public UserProfileUpdate(string username)
        {
            InitializeComponent();
            _username = username;
        }

        // Call this method after successfully editing a user
        private void OnUserEdit()
        {
            // Trigger the event
            UserEdit?.Invoke(this, EventArgs.Empty);
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Create updated user object
            var updatedUser = new UsersData
            {
                Name = NameTextBox.Text,
                Surname = SurnameTextBox.Text,
                Age = Age.Text,
                Email = EmailTextBox.Text,
                Birthday = Birthday.SelectedDate?.ToString(),  // Ensure date format
                University = University.Text,
                Experience = Experience.Text,
                Username = _username,
                Password = PasswordBox.Password
            };

            try
            {
                var firebase = new FirebaseClient(FirebaseConfig.FirebaseUrl);
                var userRef = firebase.Child("StandartUserTable");

                // Fetch data from Firebase
                string url = FirebaseConfig.FirebaseUrl + "StandartUserTable.json";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    // Deserialize JSON data
                    var usersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                    bool userFound = false;

                    // Find and update the user
                    foreach (var item in usersData)
                    {
                        if (item.Value.Username == _username)
                        {
                            var userRefToUpdate = firebase.Child("StandartUserTable").Child(item.Key);  // Reference to user by key
                            var dataToSend = new Dictionary<string, object>
                            {
                                { "Name", updatedUser.Name },
                                { "Surname", updatedUser.Surname },
                                { "Age", updatedUser.Age },
                                { "Email", updatedUser.Email },
                                { "Birthday", updatedUser.Birthday },
                                { "University", updatedUser.University },
                                { "Experience", updatedUser.Experience },
                                { "Username", updatedUser.Username },
                                { "Password", updatedUser.Password },
                                { "LoginStatus", "Active" }
                            };

                            // Update user data
                            await userRefToUpdate.PutAsync(dataToSend);
                            MessageBox.Show("Kullanıcı başarıyla güncellendi.");
                            userFound = true;
                            break;
                        }
                    }

                    if (!userFound)
                    {
                        MessageBox.Show("Bu kullanıcı adıyla eşleşen bir kullanıcı bulunamadı.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                MessageBox.Show("Bir hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OnUserEdit(); // Trigger the UserEdit event

            // Close the current window
            this.Close();

           
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            // Close the current window and optionally show UserHomePage again
            this.Close();

            
        }
    }
}
