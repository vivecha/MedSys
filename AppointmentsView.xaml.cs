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
    public partial class AppointmentsView : UserControl
    {
        private Appointment _selectedAppointment;
        private List<Appointment> _allPastAppointments = new List<Appointment>();
        private List<Appointment> _allUpcomingAppointments = new List<Appointment>();
        private List<Appointment> _allTodayAppointments = new List<Appointment>();

        public AppointmentsView()
        {
            InitializeComponent();
            LoadAppointments();
        }

        private void LoadAppointments()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    // Загрузка прошедших приемов (без изменений)
                    string pastQuery = @"SELECT a.appointment_id, a.appointment_date, 
                               CONCAT(p.last_name, ' ', LEFT(p.first_name, 1), '. ', LEFT(p.middle_name, 1), '.') as patient_name,
                               CONCAT(e.last_name, ' ', LEFT(e.first_name, 1), '. ', LEFT(e.middle_name, 1), '.') as doctor_name,
                               sl.sick_leave_id IS NOT NULL as has_sick_leave, sl.sick_leave_id
                               FROM appointments a
                               JOIN patients p ON p.patient_id = a.patient_id
                               JOIN doctors d ON a.doctor_id = d.doctor_id
                               JOIN users u on d.user_id = u.user_id
                               JOIN employees e ON u.employee_id = e.employee_id
                               LEFT JOIN sickleaves sl ON a.appointment_id = sl.appointment_id
                               WHERE a.appointment_date < CURDATE()
                               ORDER BY a.appointment_date DESC";

                    _allPastAppointments.Clear();
                    using (var command = new MySqlCommand(pastQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _allPastAppointments.Add(new Appointment
                                {
                                    AppointmentId = reader.GetInt32("appointment_id"),
                                    AppointmentDate = reader.GetDateTime("appointment_date"),
                                    PatientName = reader.GetString("patient_name"),
                                    DoctorName = reader.GetString("doctor_name"),
                                    HasSickLeave = reader.GetBoolean("has_sick_leave"),
                                    SickLeaveId = reader.IsDBNull("sick_leave_id") ? (int?)null : reader.GetInt32("sick_leave_id")
                                });
                            }
                        }
                    }
                    PastAppointmentsGrid.ItemsSource = _allPastAppointments;

                    // Загрузка сегодняшних приемов (без изменений)
                    string todayQuery = @"SELECT a.appointment_id, a.appointment_date, 
                                          CONCAT(p.last_name, ' ', LEFT(p.first_name, 1), '. ', LEFT(p.middle_name, 1), '.') as patient_name,
                                          CONCAT(e.last_name, ' ', LEFT(e.first_name, 1), '. ', LEFT(e.middle_name, 1), '.') as doctor_name,
                                          sl.sick_leave_id IS NOT NULL as has_sick_leave, sl.sick_leave_id
                                          FROM appointments a
                                          JOIN patients p ON p.patient_id = a.patient_id
                                          JOIN doctors d ON a.doctor_id = d.doctor_id
                                          JOIN users u on d.user_id = u.user_id
                                          JOIN employees e ON u.employee_id = e.employee_id
                                          LEFT JOIN sickleaves sl ON a.appointment_id = sl.appointment_id
                                          WHERE DATE(a.appointment_date) = CURDATE()
                                          ORDER BY a.appointment_date";

                    _allTodayAppointments.Clear();
                    using (var command = new MySqlCommand(todayQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _allTodayAppointments.Add(new Appointment
                                {
                                    AppointmentId = reader.GetInt32("appointment_id"),
                                    AppointmentDate = reader.GetDateTime("appointment_date"),
                                    PatientName = reader.GetString("patient_name"),
                                    DoctorName = reader.GetString("doctor_name"),
                                    HasSickLeave = reader.GetBoolean("has_sick_leave"),
                                    SickLeaveId = reader.IsDBNull("sick_leave_id") ? (int?)null : reader.GetInt32("sick_leave_id")
                                });
                            }
                        }
                    }
                    TodayAppointmentsGrid.ItemsSource = _allTodayAppointments;

                    // Загрузка предстоящих приемов (без изменений)
                    string upcomingQuery = @"SELECT a.appointment_id, a.appointment_date,
                                          CONCAT(p.last_name, ' ', LEFT(p.first_name, 1), '. ', LEFT(p.middle_name, 1), '.') as patient_name,
                                          CONCAT(e.last_name, ' ', LEFT(e.first_name, 1), '. ', LEFT(e.middle_name, 1), '.') as doctor_name
                                          FROM appointments a
                                          JOIN patients p ON p.patient_id = a.patient_id
                                          JOIN doctors d ON a.doctor_id = d.doctor_id
                                          JOIN users u on d.user_id = u.user_id
                                          JOIN employees e ON u.employee_id = e.employee_id
                                          WHERE a.appointment_date > CURDATE()
                                          ORDER BY a.appointment_date";

                    _allUpcomingAppointments.Clear();
                    using (var command = new MySqlCommand(upcomingQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _allUpcomingAppointments.Add(new Appointment
                                {
                                    AppointmentId = reader.GetInt32("appointment_id"),
                                    AppointmentDate = reader.GetDateTime("appointment_date"),
                                    PatientName = reader.GetString("patient_name"),
                                    DoctorName = reader.GetString("doctor_name")
                                });
                            }
                        }
                    }
                    UpcomingAppointmentsGrid.ItemsSource = _allUpcomingAppointments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки приемов: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new EditAppointmentWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadAppointments();
            }
        }

        private void AppointmentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid != null && grid.SelectedItem is Appointment selectedAppointment)
            {
                _selectedAppointment = selectedAppointment;
                EditButton.IsEnabled = true;
            }
        }

        private void EditAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAppointment != null)
            {
                var editWindow = new EditAppointmentWindow(_selectedAppointment.AppointmentId)
                {
                    Owner = Window.GetWindow(this) // Устанавливаем владельца для модального окна
                };

                if (editWindow.ShowDialog() == true)
                {
                    LoadAppointments();
                    _selectedAppointment = null; // Сбрасываем выбранный элемент
                    EditButton.IsEnabled = false;
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите прием для редактирования.", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ResetSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            DateFilterPicker.SelectedDate = null;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string searchText = SearchTextBox.Text.ToLower();
            DateTime? selectedDate = DateFilterPicker.SelectedDate;

            // Filter past appointments
            var filteredPast = _allPastAppointments.FindAll(a =>
                (string.IsNullOrEmpty(searchText) || a.PatientName.ToLower().Contains(searchText)) &&
                (!selectedDate.HasValue || a.AppointmentDate.Date == selectedDate.Value.Date));

            PastAppointmentsGrid.ItemsSource = filteredPast;

            // Filter today's appointments (only by name, since they're all for today)
            var filteredToday = _allTodayAppointments.FindAll(a =>
                string.IsNullOrEmpty(searchText) || a.PatientName.ToLower().Contains(searchText));

            TodayAppointmentsGrid.ItemsSource = filteredToday;

            // Filter upcoming appointments
            var filteredUpcoming = _allUpcomingAppointments.FindAll(a =>
                (string.IsNullOrEmpty(searchText) || a.PatientName.ToLower().Contains(searchText)) &&
                (!selectedDate.HasValue || a.AppointmentDate.Date == selectedDate.Value.Date));

            UpcomingAppointmentsGrid.ItemsSource = filteredUpcoming;
        }
    }
}