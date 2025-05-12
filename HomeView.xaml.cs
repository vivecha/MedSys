using System.Windows;
using System.Windows.Controls;

namespace MedicalSystem.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
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

        // Обработчики для кнопок быстрых действий
        private void CreateSickLeave_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToView("SickLeavesView");
        }

        private void RegisterPatient_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToView("PatientsView");
        }

        private void ScheduleAppointment_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToView("AppointmentsView");
        }
    }
}