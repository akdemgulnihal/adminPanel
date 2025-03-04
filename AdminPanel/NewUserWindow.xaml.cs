using AdminPanel;
using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AdminPanel
{
    public partial class NewUserWindow : Window
    {
        // Define the UserAdded event
        public event EventHandler UserAdded;
        private string _username;

        private bool flag = false;

        // Constructor for NewUserWindow
        public NewUserWindow(string _username)
        {
            InitializeComponent();
        }

        // Method to trigger UserAdded event after a user is successfully added
        private void OnUserAdded()
        {
            UserAdded?.Invoke(this, EventArgs.Empty);
            // Close the NewUserWindow
            this.Close();
        }

        // Save button click event handler
        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Create a new user object with the entered data
            var newUser = new UsersData
            {
                Name = NameTextBox.Text,
                Surname = SurnameTextBox.Text,
                Age = Age.Text,
                Email = EmailTextBox.Text,
                Birthday = Birthday.Text,
                University = University.Text,
                Experience = Experience.Text,
                Username = userid.Text,
                Password = PasswordBox.Password,
                LoginStatus = "Passive"
            };

            try
            {
                // Initialize Firebase client
                var firebase = new FirebaseClient(FirebaseService.FirebaseUrl);

                // Reference to the "StandartUserTable" in the Firebase database
                var userRef = firebase.Child("StandartUserTable");

                // Firebase URL to fetch data
                string url = FirebaseService.FirebaseUrl + "StandartUserTable.json";

                // Fetch data from Firebase
                var response = await FirebaseService.Client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    var UsersData = JsonConvert.DeserializeObject<Dictionary<string, UsersData>>(responseData);

                    // Check if username already exists in the database
                    foreach (var item in UsersData)
                    {
                        if (item.Value.Username == userid.Text)
                        {
                            MessageBox.Show("Sistemde bu username'e sahip birisi bulunmaktadır.");
                            flag = true;
                        }
                    }
                }

                // If the username doesn't exist, add the new user
                if (!flag)
                {
                    var result = await userRef.PostAsync(newUser);
                    MessageBox.Show("Yeni kullanıcı başarılı şekilde veritabanına yüklendi.");
                }
            }
            catch (Exception ex)
            {
                // Display error message
                MessageBox.Show("Hata: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OnUserAdded(); // Trigger the UserAdded event

       
       
            this.Close(); // Close NewUserWindow
        }

        // Cancel button click event handler
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
          this.Close(); // Close NewUserWindow
        }
    }
}
