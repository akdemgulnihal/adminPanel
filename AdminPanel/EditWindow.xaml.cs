using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Firebase.Database.Query;
using AdminPanel;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Page
    {
        // Define the UserEdit event for the page
        public event EventHandler UserEdit;

        private string firebaseUrl = FirebaseConfig.FirebaseUrl;
        private string _username;
        private HttpClient client = new HttpClient();

        // Constructor that accepts username to load data
        public EditWindow(string username)
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
            // Collect data from the form
            string name = NameTextBox.Text;
            string surname = SurnameTextBox.Text;
            string age = Age.Text;
            string email = EmailTextBox.Text;
            string birthday = Birthday.Text;
            string university = University.Text;
            string experience = Experience.Text;
            string password = PasswordBox.Password;

            // Create updated user object
            var updatedUser = new UsersData
            {
                Name = name,
                Surname = surname,
                Age = age,
                Email = email,
                Birthday = birthday,
                University = university,
                Experience = experience,
                Username = _username,
                Password = password
            };

            try
            {
                var firebase = new FirebaseClient(firebaseUrl);
                var userRef = firebase.Child("StandartUserTable");

                // Fetch data from Firebase
                string url = firebaseUrl + "StandartUserTable.json";
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
                                { "Password", updatedUser.Password }
                            };

                            // Update data on Firebase
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

            // Navigate back to the Admin Home Page
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();  // This will navigate to the previous page (Admin Home Page)
            }
            else
            {
                // If no page to go back to, navigate directly to AdminHomePage
                NavigationService.Navigate(new AdminHomePage(_username));
            }
        }
    }
}
