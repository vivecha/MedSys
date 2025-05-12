using MedicalSystem.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MedicalSystem.Views
{
    public partial class AddPatientWindow : Window
    {
        public AddPatientWindow()
        {
            InitializeComponent();
            BirthDatePicker.SelectedDate = DateTime.Now.AddYears(-18);
            GenderComboBox.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                BirthDatePicker.SelectedDate == null ||
                GenderComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля (помеченные *)",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            // Валидация email (если указан)
            if (!string.IsNullOrWhiteSpace(EmailTextBox.Text) &&
                !IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите корректный email адрес",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"INSERT INTO patients 
                                    (last_name, first_name, middle_name, date_of_birth, 
                                     gender, phone_number, email, address)
                                    VALUES (@lastName, @firstName, @middleName, @birthDate, 
                                            @gender, @phone, @email, @address)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@lastName", LastNameTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@firstName", FirstNameTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@middleName",
                            string.IsNullOrWhiteSpace(MiddleNameTextBox.Text) ?
                            DBNull.Value : (object)MiddleNameTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@birthDate", BirthDatePicker.SelectedDate);
                        command.Parameters.AddWithValue("@gender",
                            ((ComboBoxItem)GenderComboBox.SelectedItem).Tag.ToString());
                        command.Parameters.AddWithValue("@phone", PhoneTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@email",
                            string.IsNullOrWhiteSpace(EmailTextBox.Text) ?
                            DBNull.Value : (object)EmailTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@address", AddressTextBox.Text.Trim());

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Пациент успешно добавлен", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пациента: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}