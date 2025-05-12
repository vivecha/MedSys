using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MedicalSystem.Helpers
{
    public static class DatabaseHelper
    {
        private static string GetConnectionString()
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["MedicalSystemDB"]?.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("Строка подключения не найдена в конфигурации");
                }
                return connectionString;
            }
            catch (ConfigurationErrorsException ex)
            {
                MessageBox.Show($"Ошибка конфигурации: {ex.Message}");
                throw;
            }
        }

        public static MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(GetConnectionString());
            try
            {
                connection.Open();
                return connection;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка MySQL: {ex.Message}\nКод ошибки: {ex.Number}");
                throw;
            }
        }

        public static bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    return connection.State == System.Data.ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка БД",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToInverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool && (bool)value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}