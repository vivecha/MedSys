using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using MedicalSystem.Helpers;
using System.Data;

namespace MedicalSystem.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            LoadStatistics();
            LoadRecentSickLeaves();
        }

        private void LoadStatistics()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string patientsQuery = "SELECT COUNT(*) FROM patients";
                    PatientsCount.Text = ExecuteScalarQuery(connection, patientsQuery).ToString();

                    string doctorsQuery = "SELECT COUNT(*) FROM doctors";
                    DoctorsCount.Text = ExecuteScalarQuery(connection, doctorsQuery).ToString();

                    string appointmentsQuery = @"SELECT COUNT(*) FROM appointments 
                                              WHERE DATE(appointment_date) = CURDATE()";
                    AppointmentsCount.Text = ExecuteScalarQuery(connection, appointmentsQuery).ToString();

                    string sickLeavesQuery = "SELECT COUNT(*) FROM sickleaves WHERE closed_date IS NULL";
                    SickLeavesCount.Text = ExecuteScalarQuery(connection, sickLeavesQuery).ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRecentSickLeaves()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"SELECT sl.sick_leave_id, CONCAT(p.last_name, ' ', p.first_name, ' ', IFNULL(p.middle_name, '')) as patient_name,
                                    CONCAT(e.last_name, ' ', e.first_name, ' ', IFNULL(e.middle_name, '')) as doctor_name, 
                                    sl.start_date, p.phone_number
                                    FROM sickleaves sl
                                    JOIN patients p ON sl.patient_id = p.patient_id
                                    JOIN doctors d ON sl.doctor_id = d.doctor_id
                                    JOIN users u ON d.user_id = u.user_id
                                    JOIN employees e ON u.employee_id = e.employee_id
                                    WHERE sl.closed_date IS NULL
                                    ORDER BY sl.start_date DESC
                                    LIMIT 10";

                    var sickLeaves = new System.Collections.ObjectModel.ObservableCollection<dynamic>();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sickLeaves.Add(new
                                {
                                    SickLeaveId = reader.GetInt32("sick_leave_id"),
                                    PatientName = reader.GetString("patient_name"),
                                    DoctorName = reader.GetString("doctor_name"),
                                    start_date = reader.GetDateTime("start_date"),
                                    phone_number = reader.GetString("phone_number")
                                });
                            }
                        }
                    }
                    RecentSickLeavesGrid.ItemsSource = sickLeaves;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки последних больничных листов: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private object ExecuteScalarQuery(MySqlConnection connection, string query)
        {
            using (var command = new MySqlCommand(query, connection))
            {
                return command.ExecuteScalar();
            }
        }


        private void ViewPatients_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToView("PatientsView");
        }

        private void ViewDoctors_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToView("DoctorsView");
        }

        private void ViewAppointments_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToView("AppointmentsView");
        }

        private void ViewSickLeaves_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToView("SickLeavesView");
        }

        private void CreateSickLeave_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToView("SickLeavesFormView");
        }

        private void RegisterPatient_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToView("AddPatientWindow");
        }

        private void ScheduleAppointment_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToView("EditAppointmentWindow");
        }
    }
}
