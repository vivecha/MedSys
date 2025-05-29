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
                    string patientsQuery = @"SELECT p.patient_id, 
                                      CONCAT(p.last_name, ' ', LEFT(p.first_name, 1), '. ', LEFT(p.middle_name, 1), '.') as full_name
                                      FROM patients p";

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
                                      CONCAT(e.last_name, ' ', LEFT(e.first_name, 1), '. ', LEFT(e.middle_name, 1), '.') as full_name
                                      FROM doctors doc
                                      JOIN users u ON doc.user_id = u.user_id
                                      JOIN employees e ON u.employee_id = e.employee_id";

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

                                    DateTime appointmentDate = reader.GetDateTime("appointment_date");
                                    AppointmentDatePicker.SelectedDate = appointmentDate.Date;
                                    TimeComboBox.Text = appointmentDate.ToString("HH:mm");

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

        private bool IsTimeSlotAvailable(int doctorId, DateTime appointmentDateTime, int? excludeAppointmentId = null)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"SELECT COUNT(*) FROM appointments 
                           WHERE doctor_id = @doctorId 
                           AND appointment_date = @appointmentDate
                           AND (@excludeAppointmentId IS NULL OR appointment_id != @excludeAppointmentId)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@doctorId", doctorId);
                        command.Parameters.AddWithValue("@appointmentDate", appointmentDateTime);
                        command.Parameters.AddWithValue("@excludeAppointmentId", excludeAppointmentId);

                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки времени: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private int GetDuplicateAppointmentsCount(int patientId, int doctorId, DateTime appointmentDateTime, int? excludeAppointmentId = null)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"SELECT COUNT(*) FROM appointments 
                           WHERE patient_id = @patientId 
                           AND doctor_id = @doctorId 
                           AND appointment_date = @appointmentDate
                           AND (@excludeAppointmentId IS NULL OR appointment_id != @excludeAppointmentId)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@patientId", patientId);
                        command.Parameters.AddWithValue("@doctorId", doctorId);
                        command.Parameters.AddWithValue("@appointmentDate", appointmentDateTime);
                        command.Parameters.AddWithValue("@excludeAppointmentId", excludeAppointmentId);

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки дубликата: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения полей
            if (PatientComboBox.SelectedValue == null || DoctorComboBox.SelectedValue == null ||
                AppointmentDatePicker.SelectedDate == null || TimeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем выбранное время
            string timeString = TimeComboBox.Text;
            if (!TimeSpan.TryParse(timeString, out TimeSpan selectedTime))
            {
                MessageBox.Show("Введите корректное время в формате HH:mm!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаем полную дату и время
            DateTime appointmentDate = AppointmentDatePicker.SelectedDate.Value;
            DateTime appointmentDateTime = appointmentDate.Add(selectedTime);

            // Получаем ID текущей записи (если это редактирование)
            int? currentAppointmentId = _appointmentId;

            // Проверка на полное совпадение (пациент + врач + дата + время)
            int duplicateCount = GetDuplicateAppointmentsCount(
                (int)PatientComboBox.SelectedValue,
                (int)DoctorComboBox.SelectedValue,
                appointmentDateTime,
                currentAppointmentId);

            if (duplicateCount > 0)
            {
                // Если это полное совпадение, спрашиваем подтверждение
                var result = MessageBox.Show(
                    "Такая запись уже существует. Вы уверены, что хотите изменить её?",
                    "Подтверждение изменений",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    return; // Пользователь отказался от изменений
                }
            }
            else
            {
                // Если это не полное совпадение, проверяем занятость времени у врача
                if (!IsTimeSlotAvailable(
                    (int)DoctorComboBox.SelectedValue,
                    appointmentDateTime,
                    currentAppointmentId))
                {
                    MessageBox.Show("Это время уже занято у выбранного врача!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

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
                        command.Parameters.AddWithValue("@appointmentDate", appointmentDateTime);
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
                MessageBox.Show($"Ошибка сохранения приема: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
