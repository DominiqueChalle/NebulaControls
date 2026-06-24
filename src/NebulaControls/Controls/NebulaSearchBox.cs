// Nom: NebulaSearchBox
// Version: V1.02
// Description: SearchBox control exposing search text and clear command behavior.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaControls.Controls;

public class NebulaSearchBox : Control
{
    private readonly NebulaRelayCommand clearCommand;
    private readonly NebulaRelayCommand submitSearchCommand;
    private TextBox? textBox;
    private bool isUpdatingText;

    public event EventHandler? SearchSubmitted;

    public event EventHandler? SearchTextChanged;

    static NebulaSearchBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NebulaSearchBox),
            new FrameworkPropertyMetadata(typeof(NebulaSearchBox)));
    }

    public NebulaSearchBox()
    {
        clearCommand = new NebulaRelayCommand(_ => ClearSearch(), _ => HasText && IsEnabled);
        submitSearchCommand = new NebulaRelayCommand(_ => SubmitSearch());
    }

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(
            nameof(SearchText),
            typeof(string),
            typeof(NebulaSearchBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(NebulaSearchBox),
            new PropertyMetadata(string.Empty));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty SearchCommandProperty =
        DependencyProperty.Register(
            nameof(SearchCommand),
            typeof(ICommand),
            typeof(NebulaSearchBox),
            new PropertyMetadata(null));

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public static readonly DependencyProperty SearchCommandParameterProperty =
        DependencyProperty.Register(
            nameof(SearchCommandParameter),
            typeof(object),
            typeof(NebulaSearchBox),
            new PropertyMetadata(null));

    public object? SearchCommandParameter
    {
        get => GetValue(SearchCommandParameterProperty);
        set => SetValue(SearchCommandParameterProperty, value);
    }

    public static readonly DependencyProperty HasTextProperty =
        DependencyProperty.Register(
            nameof(HasText),
            typeof(bool),
            typeof(NebulaSearchBox),
            new PropertyMetadata(false));

    public bool HasText
    {
        get => (bool)GetValue(HasTextProperty);
        private set => SetValue(HasTextProperty, value);
    }

    public ICommand ClearCommand => clearCommand;

    public ICommand SubmitSearchCommand => submitSearchCommand;

    public override void OnApplyTemplate()
    {
        if (textBox is not null)
        {
            textBox.TextChanged -= TextBox_TextChanged;
            textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
        }

        base.OnApplyTemplate();

        textBox = GetTemplateChild("PART_TextBox") as TextBox;

        if (textBox is not null)
        {
            textBox.TextChanged += TextBox_TextChanged;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
        }

        UpdateTextBox();
        UpdateHasText();
    }

    private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NebulaSearchBox searchBox)
        {
            searchBox.UpdateTextBox();
            searchBox.UpdateHasText();
            searchBox.SearchTextChanged?.Invoke(searchBox, EventArgs.Empty);
        }
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (isUpdatingText || textBox is null)
        {
            return;
        }

        SearchText = textBox.Text;
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SubmitSearch();
            e.Handled = true;
        }

        if (e.Key == Key.Escape && HasText)
        {
            ClearSearch();
            e.Handled = true;
        }
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
        textBox?.Focus();
    }

    private void SubmitSearch()
    {
        var parameter = SearchCommandParameter ?? SearchText;

        if (SearchCommand?.CanExecute(parameter) == true)
        {
            SearchCommand.Execute(parameter);
        }

        SearchSubmitted?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateTextBox()
    {
        if (textBox is null || textBox.Text == SearchText)
        {
            return;
        }

        isUpdatingText = true;
        textBox.Text = SearchText;
        textBox.CaretIndex = textBox.Text.Length;
        isUpdatingText = false;
    }

    private void UpdateHasText()
    {
        HasText = !string.IsNullOrEmpty(SearchText);
        clearCommand.RaiseCanExecuteChanged();
    }
}
