using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MedicalSystem.Classes
{

    public class Patient
    {
        public int PatientId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }

    public class Doctor
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public int SpecialtyId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime HireDate { get; set; } 
        public DateTime? FireDate { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SpecialtyName { get; set; }
    }


    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Complaints { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public bool HasSickLeave { get; set; }
        public int? SickLeaveId { get; set; }
    }

    public class SickLeave
    {
        public int SickLeaveId { get; set; }
        public int AppointmentId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Diagnosis { get; set; }
        public string Notes { get; set; }
        public DateTime? ClosedDate { get; set; }
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

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
