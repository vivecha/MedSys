using MedicalSystem.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace MedicalSystem.Views
{
    public partial class SickLeaveFormWindow : Window
    {
        private readonly int? _sickLeaveId;
        private List<PatientItem> _patients;
        private List<DoctorItem> _doctors;
        private List<AppointmentItem> _appointments;

        public SickLeaveFormWindow(int? sickLeaveId = null)
        {
            InitializeComponent();
            _sickLeaveId = sickLeaveId;
            LoadComboBoxData();
            InitializeForm();
            if (_sickLeaveId.HasValue)
                LoadSickLeaveData();
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    _patients = new List<PatientItem>();
                    string patientsQuery = @"SELECT patient_id, CONCAT(last_name, ' ', first_name, ' ', 
                                       IFNULL(middle_name, '')) as full_name 
                                       FROM patients";
                    using (var command = new MySqlCommand(patientsQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _patients.Add(new PatientItem
                                {
                                    UserId = reader.GetInt32("patient_id"),
                                    FullName = reader.GetString("full_name")
                                });
                            }
                        }
                    }
                    PatientComboBox.ItemsSource = _patients;

                    _doctors = new List<DoctorItem>();
                    string doctorsQuery = @"SELECT d.doctor_id, CONCAT(e.last_name, ' ', e.first_name, ' ', 
                                       IFNULL(e.middle_name, '')) as full_name, ds.specialty_name
                                       FROM doctors d
                                       JOIN users u ON d.doctor_id = u.user_id
                                       JOIN employees e ON u.employee_id = e.employee_id
                                       JOIN doctorspecialties ds ON d.specialty_id = ds.specialty_id";
                    using (var command = new MySqlCommand(doctorsQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _doctors.Add(new DoctorItem
                                {
                                    DoctorId = reader.GetInt32("doctor_id"),
                                    FullName = reader.GetString("full_name"),
                                    Specialty = reader.GetString("specialty_name")
                                });
                            }
                        }
                    }
                    DoctorComboBox.ItemsSource = _doctors;

                    _appointments = new List<AppointmentItem>();
                    string appointmentsQuery = @"SELECT a.appointment_id, a.appointment_date, 
                                           CONCAT(p.last_name, ' ', p.first_name) as patient_name
                                           FROM appointments a
                                           JOIN patients p ON a.patient_id = p.patient_id
                                           ORDER BY a.appointment_date DESC";
                    using (var command = new MySqlCommand(appointmentsQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _appointments.Add(new AppointmentItem
                                {
                                    AppointmentId = reader.GetInt32("appointment_id"),
                                    AppointmentDate = reader.GetDateTime("appointment_date"),
                                    PatientName = reader.GetString("patient_name")
                                });
                            }
                        }
                    }
                    AppointmentComboBox.ItemsSource = _appointments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeForm()
        {
            FormTitle.Text = _sickLeaveId.HasValue ? "Редактирование больничного листа" : "Создание больничного листа";
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(14);
        }

        private void LoadSickLeaveData()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"SELECT sl.patient_id, sl.doctor_id, sl.appointment_id, 
                                   sl.start_date, sl.end_date, sl.diagnosis, sl.notes
                                   FROM sickleaves sl
                                   WHERE sl.sick_leave_id = @sickLeaveId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sickLeaveId", _sickLeaveId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                PatientComboBox.SelectedValue = reader.GetInt32("patient_id");
                                DoctorComboBox.SelectedValue = reader.GetInt32("doctor_id");
                                AppointmentComboBox.SelectedValue = reader.IsDBNull("appointment_id") ? 0 : reader.GetInt32("appointment_id");

                                StartDatePicker.SelectedDate = reader.GetDateTime("start_date");
                                EndDatePicker.SelectedDate = reader.GetDateTime("end_date");

                                DiagnosisTextBox.Text = reader["diagnosis"].ToString();
                                NotesTextBox.Text = reader["notes"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных больничного листа: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query;
                    if (_sickLeaveId.HasValue)
                    {
                        query = @"UPDATE sickleaves SET
                                patient_id = @patientId,
                                doctor_id = @doctorId,
                                appointment_id = @appointmentId,
                                start_date = @startDate,
                                end_date = @endDate,
                                diagnosis = @diagnosis,
                                notes = @notes
                                WHERE sick_leave_id = @sickLeaveId";
                    }
                    else
                    {
                        query = @"INSERT INTO sickleaves 
                                (patient_id, doctor_id, appointment_id, 
                                 start_date, end_date, diagnosis, notes)
                                VALUES (@patientId, @doctorId, @appointmentId, 
                                        @startDate, @endDate, @diagnosis, @notes)";
                    }

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@patientId", (PatientComboBox.SelectedItem as PatientItem)?.UserId);
                        command.Parameters.AddWithValue("@doctorId", (DoctorComboBox.SelectedItem as DoctorItem)?.DoctorId);
                        command.Parameters.AddWithValue("@appointmentId", (AppointmentComboBox.SelectedItem as AppointmentItem)?.AppointmentId ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@startDate", StartDatePicker.SelectedDate);
                        command.Parameters.AddWithValue("@endDate", EndDatePicker.SelectedDate);
                        command.Parameters.AddWithValue("@diagnosis", DiagnosisTextBox.Text);
                        command.Parameters.AddWithValue("@notes", NotesTextBox.Text);

                        if (_sickLeaveId.HasValue)
                            command.Parameters.AddWithValue("@sickLeaveId", _sickLeaveId);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Больничный лист успешно сохранен", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения больничного листа: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            if (PatientComboBox.SelectedItem == null ||
                DoctorComboBox.SelectedItem == null ||
                StartDatePicker.SelectedDate == null ||
                EndDatePicker.SelectedDate == null ||
                string.IsNullOrWhiteSpace(DiagnosisTextBox.Text))
            {
                StatusMessage.Text = "Пожалуйста, заполните все обязательные поля (помеченные *)";
                return false;
            }

            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
            {
                StatusMessage.Text = "Дата окончания не может быть раньше даты начала";
                return false;
            }

            StatusMessage.Text = "";
            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class PatientItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
    }

    public class DoctorItem
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; }
        public string Specialty { get; set; }
    }

    public class AppointmentItem
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string PatientName { get; set; }
    }
}