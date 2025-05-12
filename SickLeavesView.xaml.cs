using MedicalSystem.Classes;
using MedicalSystem.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MedicalSystem.Views
{
    public partial class SickLeavesView : UserControl
    {
        public SickLeavesView()
        {
            InitializeComponent();
            LoadSickLeaves();
            LoadFilters();
        }

        private void LoadSickLeaves(string filterQuery = "")
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string baseQuery = @"SELECT sl.sick_leave_id, 
                                       CONCAT(p.last_name, ' ', p.first_name, ' ', IFNULL(p.middle_name, '')) as patient_name,
                                       CONCAT(d.last_name, ' ', d.first_name, ' ', IFNULL(d.middle_name, '')) as doctor_name,
                                       sl.issue_date, sl.start_date, sl.end_date, sl.closed_date,
                                       CASE WHEN sl.closed_date IS NULL THEN 'Активный' ELSE 'Закрытый' END as status
                                       FROM sickleaves sl
                                       JOIN users p ON sl.patient_id = p.user_id
                                       JOIN doctors doc ON sl.doctor_id = doc.doctor_id
                                       JOIN users d ON doc.user_id = d.user_id";

                    string fullQuery = baseQuery + filterQuery + " ORDER BY sl.start_date DESC";

                    var sickLeaves = new List<SickLeaveViewItem>();
                    using (var command = new MySqlCommand(fullQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sickLeaves.Add(new SickLeaveViewItem
                                {
                                    SickLeaveId = reader.GetInt32("sick_leave_id"),
                                    PatientName = reader.GetString("patient_name"),
                                    DoctorName = reader.GetString("doctor_name"),
                                    IssueDate = reader.GetDateTime("issue_date"),
                                    StartDate = reader.GetDateTime("start_date"),
                                    EndDate = reader.GetDateTime("end_date"),
                                    Status = reader.GetString("status")
                                });
                            }
                        }
                    }
                    SickLeavesGrid.ItemsSource = sickLeaves;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки больничных листов: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFilters()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string patientsQuery = @"SELECT patient_id, CONCAT(last_name, ' ', first_name, ' ', 
                                           IFNULL(middle_name, '')) as full_name 
                                           FROM patients";
                    var patients = new List<PatientItem>();
                    using (var command = new MySqlCommand(patientsQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                patients.Add(new PatientItem
                                {
                                    UserId = reader.GetInt32("patient_id"),
                                    FullName = reader.GetString("full_name")
                                });
                            }
                        }
                    }
                    PatientFilter.ItemsSource = patients;

                    string doctorsQuery = @"SELECT doc.doctor_id, CONCAT(u.last_name, ' ', u.first_name, ' ', 
                                          IFNULL(u.middle_name, '')) as full_name
                                          FROM doctors doc
                                          JOIN users u ON doc.user_id = u.user_id";
                    var doctors = new List<DoctorItem>();
                    using (var command = new MySqlCommand(doctorsQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                doctors.Add(new DoctorItem
                                {
                                    DoctorId = reader.GetInt32("doctor_id"),
                                    FullName = reader.GetString("full_name")
                                });
                            }
                        }
                    }
                    DoctorFilter.ItemsSource = doctors;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            string filterQuery = "";
            List<string> filters = new List<string>();

            if (PatientFilter.SelectedItem != null)
            {
                int patientId = ((PatientItem)PatientFilter.SelectedItem).UserId;
                filters.Add($"sl.patient_id = {patientId}");
            }

            if (DoctorFilter.SelectedItem != null)
            {
                int doctorId = ((DoctorItem)DoctorFilter.SelectedItem).DoctorId;
                filters.Add($"sl.doctor_id = {doctorId}");
            }

            if (StatusFilter.SelectedIndex > 0)
            {
                string statusCondition = StatusFilter.SelectedIndex == 1 ?
                    "sl.closed_date IS NULL" : "sl.closed_date IS NOT NULL";
                filters.Add(statusCondition);
            }

            if (filters.Count > 0)
                filterQuery = " WHERE " + string.Join(" AND ", filters);

            LoadSickLeaves(filterQuery);
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            PatientFilter.SelectedIndex = -1;
            DoctorFilter.SelectedIndex = -1;
            StatusFilter.SelectedIndex = 0;
            LoadSickLeaves();
        }

        private void CreateSickLeave_Click(object sender, RoutedEventArgs e)
        {
            var formWindow = new SickLeaveFormWindow();
            if (formWindow.ShowDialog() == true)
                LoadSickLeaves();
        }

        private void EditSickLeave_Click(object sender, RoutedEventArgs e)
        {
            if (SickLeavesGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите больничный лист для редактирования", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selected = (SickLeaveViewItem)SickLeavesGrid.SelectedItem;
            var formWindow = new SickLeaveFormWindow(selected.SickLeaveId);
            if (formWindow.ShowDialog() == true)
                LoadSickLeaves();
        }

        private void CloseSickLeave_Click(object sender, RoutedEventArgs e)
        {
            if (SickLeavesGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите больничный лист для закрытия", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selected = (SickLeaveViewItem)SickLeavesGrid.SelectedItem;
            if (selected.Status == "Закрытый")
            {
                MessageBox.Show("Этот больничный лист уже закрыт", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Закрыть больничный лист #{selected.SickLeaveId}?", "Подтверждение",
                             MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        string query = "UPDATE sickleaves SET closed_date = NOW() WHERE sick_leave_id = @sickLeaveId";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@sickLeaveId", selected.SickLeaveId);
                            command.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Больничный лист успешно закрыт", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSickLeaves();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при закрытии больничного: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PrintSickLeave_Click(object sender, RoutedEventArgs e)
        {
            if (SickLeavesGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите больничный лист для печати", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show("Функция печати будет реализована в следующей версии", "Информация",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class SickLeaveViewItem
    {
        public int SickLeaveId { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}