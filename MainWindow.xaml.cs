using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MedicalSystem.Helpers;
using MedicalSystem.Views;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;

namespace MedicalSystem
{
    public partial class MainWindow : Window
    {
        private string _username;
        public MainWindow(string username)
        {
            InitializeComponent();
            _username = username;
            InitializeNavigation();
            InitializeDateTime();
            UpdateUser();
        }

        private void UpdateUser()
        {
            UserInfoText.Text = $"Пользователь: {_username}";
        }

        private void InitializeNavigation()
        {
            // Загружаем стартовую страницу
            NavigateToView("HomeView");

            // Устанавливаем начальные размеры панели
            NavPanel.Width = 60;
            SetNavPanelCollapsedState(true);
        }

        private void InitializeDateTime()
        {
            // Обновляем время при запуске
            UpdateDateTime();

            // Таймер для обновления времени каждую минуту
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            timer.Tick += (s, e) => UpdateDateTime();
            timer.Start();
        }

        private void UpdateDateTime()
        {
            DateTimeText.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        }

        // Обработчики событий для панели навигации
        private void NavPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            NavPanel.Width = 200;
            SetNavPanelCollapsedState(false);
        }

        private void NavPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            NavPanel.Width = 60;
            SetNavPanelCollapsedState(true);
        }

        private void SetNavPanelCollapsedState(bool isCollapsed)
        {
            if (NavPanel.Child is StackPanel navStack)
            {
                foreach (var child in navStack.Children)
                {
                    if (child is Button button)
                    {
                        if (button.Content is Grid grid && grid.Children.Count > 1)
                        {
                            
                            if (grid.Children[1] is TextBlock textBlock)
                            {
                                textBlock.Visibility = isCollapsed ? Visibility.Collapsed : Visibility.Visible;
                            }
                        }

                        button.HorizontalContentAlignment = isCollapsed ? HorizontalAlignment.Center : HorizontalAlignment.Left;
                    }
                }
            }
        }
        public void NavigateToView(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string viewName)
            {
                NavigateToView(viewName);
            }
        }

        public void NavigateToView(string viewName)
        {
            try
            {
                FrameworkElement view = viewName switch
                {
                    "HomeView" => new HomeView(),
                    "PatientsView" => new PatientsView(),
                    "SickLeavesView" => new SickLeavesView(),
                    "AppointmentsView" => new AppointmentsView(),
                    "DoctorsView" => new DoctorsView(),
                    _ => new HomeView()
                };

                MainFrame.Content = view;
                UpdateStatusText(viewName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DatabaseHelper.TestConnection())
                {
                    StatusText.Text = "Подключение к БД: ✔";
                    StatusText.Foreground = Brushes.Green;
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Подключение к БД: ❌";
                StatusText.Foreground = Brushes.Red;
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatusText(string viewName)
        {
            StatusText.Text = viewName switch
            {
                "HomeView" => "Главная панель",
                "PatientsView" => "Управление пациентами",
                "SickLeavesView" => "Управление больничными листами",
                "AppointmentsView" => "Управление приёмами", // Добавлен новый статус
                _ => "Готов к работе"
            };
        }
    }
}