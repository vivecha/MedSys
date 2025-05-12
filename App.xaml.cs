using MedicalSystem.Helpers;
using System.Configuration;
using System.Data;
using System.Windows;

namespace MedSys
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!DatabaseHelper.TestConnection())
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Приложение будет закрыто.");
                Current.Shutdown();
            }
        }
    }
}
