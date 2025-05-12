using MedicalSystem.Classes;
using MedicalSystem.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace MedicalSystem.Views
{
    public partial class AddDoctorWindow : Window, INotifyPropertyChanged
    {
        private bool _isEditMode;
        private List<DoctorSpecialty> _specialties;

        public string WindowTitle => _isEditMode ? "Редактирование врача" : "Добавление врача";
        public bool IsEditMode => _isEditMode;

        private Doctor _currentDoctor;
        public Doctor CurrentDoctor
        {
            get => _currentDoctor;
            set
            {
                _currentDoctor = value;
                OnPropertyChanged(nameof(CurrentDoctor));
            }
        }

        public List<DoctorSpecialty> Specialties
        {
            get => _specialties;
            set
            {
                _specialties = value;
                OnPropertyChanged(nameof(Specialties));
            }
        }

        public AddDoctorWindow() // Добавление
        {
            InitializeComponent();
            _isEditMode = false;
            CurrentDoctor = new Doctor();
            LoadSpecialties();
            DataContext = this;
        }

        public AddDoctorWindow(int doctorId) // Редактирование
        {
            InitializeComponent();
            _isEditMode = true;
            CurrentDoctor = new Doctor();
            LoadSpecialties();
            LoadDoctorData(doctorId);
            DataContext = this;
        }

        private void LoadSpecialties()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT specialty_id, specialty_name FROM doctorspecialties";
                    var specialties = new List<DoctorSpecialty>();

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                specialties.Add(new DoctorSpecialty
                                {
                                    SpecialtyId = reader.GetInt32("specialty_id"),
                                    SpecialtyName = reader.GetString("specialty_name")
                                });
                            }
                        }
                    }

                    Specialties = specialties;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки специальностей: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDoctorData(int doctorId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"SELECT d.doctor_id, d.specialty_id, 
                                   u.user_id, u.username, u.first_name, u.last_name, 
                                   u.middle_name, u.email
                                   FROM doctors d
                                   JOIN users u ON d.user_id = u.user_id
                                   WHERE d.doctor_id = @doctorId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@doctorId", doctorId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                CurrentDoctor = new Doctor
                                {
                                    DoctorId = reader.GetInt32("doctor_id"),
                                    UserId = reader.GetInt32("user_id"),
                                    SpecialtyId = reader.GetInt32("specialty_id"),
                                    LastName = reader.GetString("last_name"),
                                    FirstName = reader.GetString("first_name"),
                                    MiddleName = reader.IsDBNull("middle_name") ? null : reader.GetString("middle_name"),
                                    Email = reader.IsDBNull("email") ? null : reader.GetString("email"),
                                    Username = reader.GetString("username")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных врача: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(CurrentDoctor.LastName) ||
                string.IsNullOrWhiteSpace(CurrentDoctor.FirstName) ||
                string.IsNullOrWhiteSpace(CurrentDoctor.Email) ||
                string.IsNullOrWhiteSpace(CurrentDoctor.Username) ||
                CurrentDoctor.SpecialtyId == 0)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля (помеченные *)",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            if (_isEditMode)
                            {
                                // Обновление пользователя
                                string updateUserQuery = @"UPDATE users SET 
                                                        first_name = @firstName,
                                                        last_name = @lastName,
                                                        middle_name = @middleName,
                                                        email = @email,
                                                        username = @username
                                                        WHERE user_id = @userId";

                                using (var command = new MySqlCommand(updateUserQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@firstName", CurrentDoctor.FirstName);
                                    command.Parameters.AddWithValue("@lastName", CurrentDoctor.LastName);
                                    command.Parameters.AddWithValue("@middleName",
                                        string.IsNullOrWhiteSpace(CurrentDoctor.MiddleName) ? DBNull.Value : (object)CurrentDoctor.MiddleName);
                                    command.Parameters.AddWithValue("@email", CurrentDoctor.Email);
                                    command.Parameters.AddWithValue("@username", CurrentDoctor.Username);
                                    command.Parameters.AddWithValue("@userId", CurrentDoctor.UserId);

                                    command.ExecuteNonQuery();
                                }

                                // Обновление врача
                                string updateDoctorQuery = @"UPDATE doctors SET 
                                                          specialty_id = @specialtyId
                                                          WHERE doctor_id = @doctorId";

                                using (var command = new MySqlCommand(updateDoctorQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@specialtyId", CurrentDoctor.SpecialtyId);
                                    command.Parameters.AddWithValue("@doctorId", CurrentDoctor.DoctorId);

                                    command.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // Добавление пользователя
                                string insertUserQuery = @"INSERT INTO users 
                                                        (first_name, last_name, middle_name, email, username, password, role_id)
                                                        VALUES (@firstName, @lastName, @middleName, @email, @username, @password, 2)";

                                int newUserId;

                                using (var command = new MySqlCommand(insertUserQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@firstName", CurrentDoctor.FirstName);
                                    command.Parameters.AddWithValue("@lastName", CurrentDoctor.LastName);
                                    command.Parameters.AddWithValue("@middleName",
                                        string.IsNullOrWhiteSpace(CurrentDoctor.MiddleName) ? DBNull.Value : (object)CurrentDoctor.MiddleName);
                                    command.Parameters.AddWithValue("@email", CurrentDoctor.Email);
                                    command.Parameters.AddWithValue("@username", CurrentDoctor.Username);

                                    command.ExecuteNonQuery();
                                    newUserId = (int)command.LastInsertedId;
                                }

                                // Добавление врача
                                string insertDoctorQuery = @"INSERT INTO doctors 
                                                          (user_id, specialty_id)
                                                          VALUES (@userId, @specialtyId)";

                                using (var command = new MySqlCommand(insertDoctorQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@userId", newUserId);
                                    command.Parameters.AddWithValue("@specialtyId", CurrentDoctor.SpecialtyId);

                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            MessageBox.Show("Данные врача успешно сохранены", "Успех",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                            DialogResult = true;
                            Close();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Ошибка сохранения данных: {ex.Message}", "Ошибка",
                                          MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DoctorSpecialty
    {
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; }
    }
}