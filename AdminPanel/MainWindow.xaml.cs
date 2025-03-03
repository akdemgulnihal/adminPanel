using System.Net.Http;
using System.Windows;


namespace WpfApp1
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




    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            // AdminLoginPage Frame yükleme 
            MainFrame.Navigate(new AdminLoginPage());
        }
    }
}
