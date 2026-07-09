// Nom: NebulaProgressBar
// Version: V1.03
// Description: ProgressBar base control styled for Nebula progress indicators.

using System;
using System.Windows;
using System.Windows.Controls;

namespace NebulaControls.Controls;

public class NebulaProgressBar : ProgressBar
{
    public static readonly DependencyProperty ShowValueTextProperty =
        DependencyProperty.Register(
            nameof(ShowValueText),
            typeof(bool),
            typeof(NebulaProgressBar),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty ValueTextProperty =
        DependencyProperty.Register(
            nameof(ValueText),
            typeof(string),
            typeof(NebulaProgressBar),
            new FrameworkPropertyMetadata(string.Empty, OnProgressValueChanged));

    private static readonly DependencyPropertyKey ProgressTextPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ProgressText),
            typeof(string),
            typeof(NebulaProgressBar),
            new FrameworkPropertyMetadata("0%"));

    public static readonly DependencyProperty ProgressTextProperty =
        ProgressTextPropertyKey.DependencyProperty;

    static NebulaProgressBar()
    {
        ValueProperty.OverrideMetadata(
            typeof(NebulaProgressBar),
            new FrameworkPropertyMetadata(0d, OnProgressValueChanged));
        MinimumProperty.OverrideMetadata(
            typeof(NebulaProgressBar),
            new FrameworkPropertyMetadata(0d, OnProgressValueChanged));
        MaximumProperty.OverrideMetadata(
            typeof(NebulaProgressBar),
            new FrameworkPropertyMetadata(100d, OnProgressValueChanged));
    }

    public NebulaProgressBar()
    {
        UpdateProgressText();
    }

    public bool ShowValueText
    {
        get => (bool)GetValue(ShowValueTextProperty);
        set => SetValue(ShowValueTextProperty, value);
    }

    public string ValueText
    {
        get => (string)GetValue(ValueTextProperty);
        set => SetValue(ValueTextProperty, value);
    }

    public string ProgressText
    {
        get => (string)GetValue(ProgressTextProperty);
        private set => SetValue(ProgressTextPropertyKey, value);
    }

    private static void OnProgressValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is NebulaProgressBar progressBar)
        {
            progressBar.UpdateProgressText();
        }
    }

    private void UpdateProgressText()
    {
        if (!string.IsNullOrWhiteSpace(ValueText))
        {
            ProgressText = ValueText;
            return;
        }

        var range = Maximum - Minimum;
        if (range <= 0)
        {
            ProgressText = "0%";
            return;
        }

        var percentage = Math.Round((Value - Minimum) / range * 100d);
        percentage = Math.Max(0d, Math.Min(100d, percentage));
        ProgressText = $"{percentage:0}%";
    }
}
