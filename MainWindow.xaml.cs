using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NebulaUI.Controls;

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

        private void ShowInfoDialogButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogDemo(
                "Information",
                "NebulaDialog Info",
                "This dialog keeps the Nebula surface neutral while using a contextual rail and icon.",
                NebulaDialogVariant.Info,
                primaryButtonText: "Got it");
        }

        private void ShowWarningDialogButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogDemo(
                "Warning",
                "Unsaved changes",
                "Some settings have not been saved yet. Review them before leaving this surface.",
                NebulaDialogVariant.Warning,
                primaryButtonText: "Review",
                secondaryButtonText: "Cancel");
        }

        private void ShowDangerDialogButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogDemo(
                "Danger",
                "Delete component",
                "This action cannot be undone. The dialog stays calm, but the context remains clear.",
                NebulaDialogVariant.Danger,
                primaryButtonText: "Delete",
                secondaryButtonText: "Cancel");
        }

        private void ShowThreeButtonDialogButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogDemo(
                "Three-button dialog",
                "Resolve pending action",
                "A NebulaDialog can expose a tertiary action when a classic two-button result is not enough.",
                NebulaDialogVariant.Success,
                primaryButtonText: "Apply",
                secondaryButtonText: "Cancel",
                tertiaryButtonText: "Later");
        }

        private NebulaDialogResult ShowDialogDemo(
            string title,
            string dialogTitle,
            string message,
            NebulaDialogVariant variant,
            string primaryButtonText,
            string secondaryButtonText = "Cancel",
            string? tertiaryButtonText = null)
        {
            var dialog = new NebulaDialog
            {
                Owner = this,
                Title = title,
                DialogTitle = dialogTitle,
                Message = message,
                Variant = variant,
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText,
                TertiaryButtonText = tertiaryButtonText
            };

            dialog.ShowDialog();
            return dialog.Result;
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
