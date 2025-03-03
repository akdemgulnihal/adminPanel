using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{

    public static class FirebaseConfig
    {
        // Firebase URL'sini burada tanımlıyoruz
        public static readonly string FirebaseUrl = "https://adminpanel-a66ab-default-rtdb.firebaseio.com/";
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
            // Load the AdminLoginPage into the Frame
            MainFrame.Navigate(new AdminLoginPage());
        }
    }
}
