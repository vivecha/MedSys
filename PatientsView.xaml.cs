using MedicalSystem.Helpers;
using MedicalSystem.Classes;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace MedicalSystem.Views
{
    public partial class PatientsView : UserControl
    {
        public PatientsView()
        {
            InitializeComponent();
            LoadPatients();
        }

        private void SearchPatients_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"SELECT * FROM patients 
                                   WHERE LOWER(last_name) LIKE @search OR
                                         LOWER(first_name) LIKE @search OR
                                         LOWER(phone_number) LIKE @search";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@search", $"%{searchText}%");
                        PatientsGrid.ItemsSource = ExecutePatientQuery(command);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddPatient_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddPatientWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadPatients(); // Обновляем список после добавления
            }
        }

        private void RefreshPatients_Click(object sender, RoutedEventArgs e)
        {
            LoadPatients();
        }

        private void LoadPatients()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT * FROM patients";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        PatientsGrid.ItemsSource = ExecutePatientQuery(command);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пациентов: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        private List<Patient> ExecutePatientQuery(MySqlCommand command)
        {
            var patients = new List<Patient>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    patients.Add(new Patient
                    {
                        PatientId = reader.GetInt32("patient_id"),
                        LastName = reader.GetString("last_name"),
                        FirstName = reader.GetString("first_name"),
                        MiddleName = reader.IsDBNull("middle_name") ? null : reader.GetString("middle_name"),
                        DateOfBirth = reader.GetDateTime("date_of_birth"),
                        Gender = reader.GetString("gender"),
                        PhoneNumber = reader.GetString("phone_number"),
                        Email = reader.IsDBNull("email") ? null : reader.GetString("email"),
                        Address = reader.GetString("address")
                    });
                }
            }

            return patients;
        }
    }

    
}