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
                    // Загрузка прошедших приемов
                    string pastQuery = @"SELECT a.appointment_id, a.appointment_date, 
                                       CONCAT(p.last_name, ' ', LEFT(p.first_name, 1), '. ', LEFT(p.middle_name, 1), '.') as patient_name,
                                       CONCAT(d.last_name, ' ', LEFT(d.first_name, 1), '. ', LEFT(d.middle_name, 1), '.') as doctor_name,
                                       sl.sick_leave_id IS NOT NULL as has_sick_leave, sl.sick_leave_id
                                       FROM appointments a
                                       JOIN users p ON a.patient_id = p.user_id
                                       JOIN doctors doc ON a.doctor_id = doc.doctor_id
                                       JOIN users d ON doc.user_id = d.user_id
                                       LEFT JOIN sickleaves sl ON a.appointment_id = sl.appointment_id
                                       WHERE a.appointment_date < NOW()
                                       ORDER BY a.appointment_date DESC";

                    var pastAppointments = new List<Appointment>();
                    using (var command = new MySqlCommand(pastQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pastAppointments.Add(new Appointment
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
                    PastAppointmentsGrid.ItemsSource = pastAppointments;

                    // Загрузка предстоящих приемов
                    string upcomingQuery = @"SELECT a.appointment_id, a.appointment_date, 
                                           CONCAT(p.last_name, ' ', LEFT(p.first_name, 1), '. ', LEFT(p.middle_name, 1), '.') as patient_name
                                           FROM appointments a
                                           JOIN users p ON a.patient_id = p.user_id
                                           WHERE a.appointment_date >= NOW()
                                           ORDER BY a.appointment_date";

                    var upcomingAppointments = new List<Appointment>();
                    using (var command = new MySqlCommand(upcomingQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                upcomingAppointments.Add(new Appointment
                                {
                                    AppointmentId = reader.GetInt32("appointment_id"),
                                    AppointmentDate = reader.GetDateTime("appointment_date"),
                                    PatientName = reader.GetString("patient_name")
                                });
                            }
                        }
                    }
                    UpcomingAppointmentsGrid.ItemsSource = upcomingAppointments;
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

        private void EditAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAppointment != null)
            {
                var editWindow = new EditAppointmentWindow(_selectedAppointment.AppointmentId);
                if (editWindow.ShowDialog() == true)
                {
                    LoadAppointments();
                }
            }
        }

        private void ViewSickLeave_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is int sickLeaveId)
            {
                var viewWindow = new SickLeaveWindow(sickLeaveId);
                viewWindow.ShowDialog();
            }
        }
    }
}