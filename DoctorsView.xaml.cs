using MedicalSystem.Classes;
using MedicalSystem.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace MedicalSystem.Views
{
    public partial class DoctorsView : UserControl
    {
        private List<Doctor> _allDoctors;

        public DoctorsView()
        {
            InitializeComponent();
            LoadDoctors();
        }

        private void LoadDoctors()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"SELECT d.doctor_id, d.user_id, d.specialty_id, 
                           e.last_name, e.first_name, e.middle_name, e.email,
                           ds.specialty_name
                           FROM doctors d
                           JOIN users u ON d.user_id = u.user_id
                           JOIN employees e ON u.employee_id = e.employee_id
                           JOIN doctorspecialties ds ON d.specialty_id = ds.specialty_id";

                    _allDoctors = new List<Doctor>();

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _allDoctors.Add(new Doctor
                                {
                                    DoctorId = reader.GetInt32("doctor_id"),
                                    UserId = reader.GetInt32("user_id"),
                                    SpecialtyId = reader.GetInt32("specialty_id"),
                                    LastName = reader.GetString("last_name"),
                                    FirstName = reader.GetString("first_name"),
                                    MiddleName = reader.IsDBNull("middle_name") ? null : reader.GetString("middle_name"),
                                    Email = reader.IsDBNull("email") ? null : reader.GetString("email"),
                                    SpecialtyName = reader.GetString("specialty_name")
                                });
                            }
                        }
                    }

                    DoctorsGrid.ItemsSource = _allDoctors;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки врачей: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SearchDoctors_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                DoctorsGrid.ItemsSource = _allDoctors;
                return;
            }

            var filtered = _allDoctors.FindAll(d =>
                (d.LastName?.ToLower().Contains(searchText) ?? false) ||
                (d.FirstName?.ToLower().Contains(searchText) ?? false) ||
                (d.SpecialtyName?.ToLower().Contains(searchText) ?? false) ||
                (d.Email?.ToLower().Contains(searchText) ?? false));

            DoctorsGrid.ItemsSource = filtered;
        }

        private void AddDoctor_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddDoctorWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadDoctors();
            }
        }

        private void EditDoctor_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is int doctorId)
            {
                var editWindow = new AddDoctorWindow(doctorId);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDoctors();
                }
            }
        }

        private void DeleteDoctor_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is int userId)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить этого врача?", "Подтверждение",
                                   MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var connection = DatabaseHelper.GetConnection())
                        {
                            // Удаляем сначала из doctors, затем из users
                            string deleteDoctorQuery = "DELETE FROM doctors WHERE user_id = @userId";
                            string deleteUserQuery = "DELETE FROM users WHERE user_id = @userId";

                            using (var transaction = connection.BeginTransaction())
                            {
                                try
                                {
                                    using (var command = new MySqlCommand(deleteDoctorQuery, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@userId", userId);
                                        command.ExecuteNonQuery();
                                    }

                                    using (var command = new MySqlCommand(deleteUserQuery, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@userId", userId);
                                        command.ExecuteNonQuery();
                                    }

                                    transaction.Commit();
                                    LoadDoctors();
                                }
                                catch
                                {
                                    transaction.Rollback();
                                    throw;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления врача: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RefreshDoctors_Click(object sender, RoutedEventArgs e)
        {
            LoadDoctors();
        }
    }
}