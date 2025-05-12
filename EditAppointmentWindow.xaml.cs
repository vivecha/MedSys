using MedicalSystem.Classes;
using MedicalSystem.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MedicalSystem.Views
{
    public partial class EditAppointmentWindow : Window
    {
        private readonly int? _appointmentId;

        public EditAppointmentWindow(int? appointmentId = null)
        {
            _appointmentId = appointmentId;
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    // Загрузка пациентов
                    string patientsQuery = @"SELECT patient_id, 
                                  CONCAT(last_name, ' ', LEFT(first_name, 1), '. ', LEFT(middle_name, 1), '.') as full_name
                                  FROM patients";

                    var patients = new List<KeyValuePair<int, string>>();
                    using (var command = new MySqlCommand(patientsQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                patients.Add(new KeyValuePair<int, string>(reader.GetInt32("patient_id"), reader.GetString("full_name")));
                            }
                        }
                    }
                    PatientComboBox.ItemsSource = patients;
                    PatientComboBox.DisplayMemberPath = "Value";
                    PatientComboBox.SelectedValuePath = "Key";

                    // Загрузка врачей
                    string doctorsQuery = @"SELECT doc.doctor_id, 
                                  CONCAT(u.last_name, ' ', LEFT(u.first_name, 1), '. ', LEFT(u.middle_name, 1), '.') as full_name
                                  FROM doctors doc
                                  JOIN users u ON doc.user_id = u.user_id";

                    var doctors = new List<KeyValuePair<int, string>>();
                    using (var command = new MySqlCommand(doctorsQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                doctors.Add(new KeyValuePair<int, string>(reader.GetInt32("doctor_id"), reader.GetString("full_name")));
                            }
                        }
                    }
                    DoctorComboBox.ItemsSource = doctors;
                    DoctorComboBox.DisplayMemberPath = "Value";
                    DoctorComboBox.SelectedValuePath = "Key";

                    if (_appointmentId.HasValue)
                    {
                        string appointmentQuery = @"SELECT patient_id, doctor_id, appointment_date, complaints, diagnosis, treatment
                                         FROM appointments
                                         WHERE appointment_id = @appointmentId";

                        using (var command = new MySqlCommand(appointmentQuery, connection))
                        {
                            command.Parameters.AddWithValue("@appointmentId", _appointmentId);
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    PatientComboBox.SelectedValue = reader.GetInt32("patient_id");
                                    DoctorComboBox.SelectedValue = reader.GetInt32("doctor_id");
                                    AppointmentDatePicker.SelectedDate = reader.GetDateTime("appointment_date");
                                    ComplaintsTextBox.Text = reader["complaints"].ToString();
                                    DiagnosisTextBox.Text = reader["diagnosis"].ToString();
                                    TreatmentTextBox.Text = reader["treatment"].ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query;
                    if (_appointmentId.HasValue)
                    {
                        query = @"UPDATE appointments
                                SET patient_id = @patientId, doctor_id = @doctorId, appointment_date = @appointmentDate,
                                    complaints = @complaints, diagnosis = @diagnosis, treatment = @treatment
                                WHERE appointment_id = @appointmentId";
                    }
                    else
                    {
                        query = @"INSERT INTO appointments (patient_id, doctor_id, appointment_date, complaints, diagnosis, treatment)
                                VALUES (@patientId, @doctorId, @appointmentDate, @complaints, @diagnosis, @treatment)";
                    }

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@patientId", PatientComboBox.SelectedValue);
                        command.Parameters.AddWithValue("@doctorId", DoctorComboBox.SelectedValue);
                        command.Parameters.AddWithValue("@appointmentDate", AppointmentDatePicker.SelectedDate);
                        command.Parameters.AddWithValue("@complaints", ComplaintsTextBox.Text);
                        command.Parameters.AddWithValue("@diagnosis", DiagnosisTextBox.Text);
                        command.Parameters.AddWithValue("@treatment", TreatmentTextBox.Text);

                        if (_appointmentId.HasValue)
                        {
                            command.Parameters.AddWithValue("@appointmentId", _appointmentId);
                        }

                        command.ExecuteNonQuery();
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения приема: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
