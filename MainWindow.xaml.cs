using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NebulaUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            GalleryRows = new ObservableCollection<GalleryDataGridRow>
            {
                new("NebulaListBox", "Validated", "Selection"),
                new("NebulaSlider", "Validated", "Input"),
                new("NebulaTabControl", "Validated", "Navigation"),
                new("NebulaExpander", "Validated", "Container"),
                new("NebulaDataGrid", "Draft", "Data")
            };

            InitializeComponent();
            Loaded += (_, _) => UpdateWindowChrome();
            SizeChanged += (_, _) => UpdateWindowChrome();
        }

        public ObservableCollection<GalleryDataGridRow> GalleryRows { get; }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleWindowState();
                return;
            }

            DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleWindowState()
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

            UpdateWindowChrome();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            UpdateWindowChrome();
        }

        private void UpdateWindowChrome()
        {
            if (MainChrome is null || MaximizeRestoreButton is null)
            {
                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                MainChrome.CornerRadius = new CornerRadius(0);
                MainChrome.BorderThickness = new Thickness(0);
                MainChrome.Clip = null;
                MaximizeIcon.Visibility = Visibility.Collapsed;
                RestoreIcon.Visibility = Visibility.Visible;
                return;
            }

            MainChrome.CornerRadius = new CornerRadius(12);
            MainChrome.BorderThickness = new Thickness(1);
            MainChrome.Clip = new RectangleGeometry(
                new Rect(0, 0, MainChrome.ActualWidth, MainChrome.ActualHeight),
                12,
                12);
            MaximizeIcon.Visibility = Visibility.Visible;
            RestoreIcon.Visibility = Visibility.Collapsed;
        }
    }

    public sealed record GalleryDataGridRow(string Component, string Status, string Category);
}
