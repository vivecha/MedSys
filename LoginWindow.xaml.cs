using System;
using System.Windows;
using MedicalSystem.Helpers;
using MySql.Data.MySqlClient;

namespace MedicalSystem
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                txtError.Text = "Логин и пароль не могут быть пустыми!";
                return;
            }

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT COUNT(*) FROM users WHERE username = @username AND password = @password";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        int count = Convert.ToInt32(command.ExecuteScalar());

                        if (count > 0)
                        {
                            MainWindow mainWindow = new MainWindow(username);
                            mainWindow.Show();
                            this.Close();
                        }
                        else
                        {
                            txtError.Text = "Неверный логин или пароль!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtError.Text = $"Ошибка: {ex.Message}";
            }
        }
    }
}