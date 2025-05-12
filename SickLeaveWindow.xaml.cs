using MedicalSystem.Classes;
using MedicalSystem.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace MedicalSystem.Views
{
    public partial class SickLeaveWindow : Window
    {
        private readonly int? _sickLeaveId;

        public SickLeaveWindow(int? sickLeaveId = null)
        {
            _sickLeaveId = sickLeaveId;
            InitializeComponent();
            LoadSickLeaveDetails();
        }

        private void LoadSickLeaveDetails()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"SELECT sl.patient_id, sl.doctor_id, sl.issue_date, sl.start_date, sl.end_date, sl.diagnosis, sl.notes
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
                                IssueDatePicker.SelectedDate = reader.GetDateTime("issue_date");
                                StartDatePicker.SelectedDate = reader.GetDateTime("start_date");
                                EndDatePicker.SelectedDate = reader.GetDateTime("end_date");
                                DiagnosisTextBox.Text = reader["diagnosis"].ToString();
                                NotesTextBox.Text = reader["notes"].ToString();
                            }
                        }
                    }

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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки деталей больничного: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"UPDATE sickleaves
                                   SET issue_date = @issueDate, start_date = @startDate, end_date = @endDate,
                                       diagnosis = @diagnosis, notes = @notes
                                   WHERE sick_leave_id = @sickLeaveId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@issueDate", IssueDatePicker.SelectedDate);
                        command.Parameters.AddWithValue("@startDate", StartDatePicker.SelectedDate);
                        command.Parameters.AddWithValue("@endDate", EndDatePicker.SelectedDate);
                        command.Parameters.AddWithValue("@diagnosis", DiagnosisTextBox.Text);
                        command.Parameters.AddWithValue("@notes", NotesTextBox.Text);
                        command.Parameters.AddWithValue("@sickLeaveId", _sickLeaveId);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Больничный лист успешно обновлен.", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения больничного: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
