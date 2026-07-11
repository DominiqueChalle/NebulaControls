using System.Collections.ObjectModel;
using System.Windows.Controls;
using NebulaControls.Controls;

namespace NebulaControls.Demo.Views;

public partial class InputsProgressView : UserControl
{
    private readonly SearchDemoItem[] searchableControls =
    [
        new("NebulaTextBox", "Inputs", "Champ texte avec placeholder, états read-only, disabled et validation."),
        new("NebulaLabeledTextBox", "Inputs", "Champ texte avec label, aide, warning et error."),
        new("NebulaPasswordBox", "Inputs", "Saisie de mot de passe alignée sur les tailles de TextBox."),
        new("NebulaLabeledPasswordBox", "Inputs", "Mot de passe avec label, aide et validation côté application."),
        new("NebulaDatePicker", "Pickers", "Sélection d'une date avec calendrier Nebula custom."),
        new("NebulaTimePicker", "Pickers", "Sélection d'une heure en format 24h avec saisie manuelle."),
        new("NebulaNumericUpDown", "Inputs", "Valeur numérique bornée avec boutons plus et moins."),
        new("NebulaSearchBox", "Inputs", "Recherche avec icône, placeholder, clear et validation Entrée."),
        new("NebulaComboBox", "Selection", "Liste déroulante standard, editable ou avec item désactivé."),
        new("NebulaSlider", "Inputs", "Sélection de valeur continue horizontale ou verticale."),
        new("NebulaProgressBar", "Progress", "Progression déterminée ou indéterminée."),
        new("NebulaSpinner", "Progress", "Indicateur d'activité compact.")
    ];

    public InputsProgressView()
    {
        InitializeComponent();
        ApplySearchFilter(string.Empty);
    }

    public ObservableCollection<SearchDemoItem> FilteredSearchItems { get; } = [];

    private void ControlSearchBox_SearchTextChanged(object sender, System.EventArgs e)
    {
        if (sender is not NebulaSearchBox searchBox)
        {
            return;
        }

        ApplySearchFilter(searchBox.SearchText);
    }

    private void ControlSearchBox_SearchSubmitted(object sender, System.EventArgs e)
    {
        if (sender is not NebulaSearchBox searchBox)
        {
            return;
        }

        SearchStatusText.Text = string.IsNullOrWhiteSpace(searchBox.SearchText)
            ? "Recherche réinitialisée."
            : $"Recherche validée : {searchBox.SearchText}";
    }

    private void ApplySearchFilter(string searchText)
    {
        FilteredSearchItems.Clear();

        var matches = string.IsNullOrWhiteSpace(searchText)
            ? searchableControls
            : searchableControls.Where(control =>
                control.Name.Contains(searchText, System.StringComparison.CurrentCultureIgnoreCase)
                || control.Category.Contains(searchText, System.StringComparison.CurrentCultureIgnoreCase)
                || control.Description.Contains(searchText, System.StringComparison.CurrentCultureIgnoreCase));

        foreach (var control in matches)
        {
            FilteredSearchItems.Add(control);
        }

        SearchStatusText.Text = FilteredSearchItems.Count == 0
            ? "Aucun contrôle trouvé."
            : $"{FilteredSearchItems.Count} contrôle(s) affiché(s).";
    }

    private void EmailTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is not NebulaTextBox textBox)
        {
            return;
        }

        var email = textBox.Text.Trim();
        ClearEmailValidation(textBox);

        if (string.IsNullOrWhiteSpace(email))
        {
            textBox.HasError = true;
            textBox.ErrorText = "Email is required.";
            return;
        }

        if (!IsValidEmail(email))
        {
            textBox.HasWarning = true;
            textBox.WarningText = "Email looks incomplete or invalid.";
        }
    }

    private static void ClearEmailValidation(NebulaTextBox textBox)
    {
        textBox.HasError = false;
        textBox.HasWarning = false;
        textBox.ErrorText = string.Empty;
        textBox.WarningText = string.Empty;
    }

    private static bool IsValidEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        var dotIndex = email.LastIndexOf('.');

        return atIndex > 0
            && dotIndex > atIndex + 1
            && dotIndex < email.Length - 1;
    }

    private void PasswordValidationBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is not NebulaLabeledPasswordBox passwordBox)
        {
            return;
        }

        var password = passwordBox.Password;
        ClearPasswordValidation(passwordBox);

        if (string.IsNullOrWhiteSpace(password))
        {
            passwordBox.HasError = true;
            passwordBox.ErrorText = "Password is required.";
            return;
        }

        if (!IsStrongPassword(password))
        {
            passwordBox.HasWarning = true;
            passwordBox.WarningText = "Use at least 8 characters and one special character.";
        }
    }

    private static void ClearPasswordValidation(NebulaLabeledPasswordBox passwordBox)
    {
        passwordBox.HasError = false;
        passwordBox.HasWarning = false;
        passwordBox.ErrorText = string.Empty;
        passwordBox.WarningText = string.Empty;
    }

    private static bool IsStrongPassword(string password)
    {
        return password.Length >= 8
            && password.Any(character => !char.IsLetterOrDigit(character));
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxSelectionText is null || sender is not ComboBox comboBox)
        {
            return;
        }

        var source = comboBox.Tag?.ToString() ?? "ComboBox";
        ComboBoxSelectionText.Text = $"{source} selected value: {GetComboBoxSelectionText(comboBox)}";
    }

    private static string GetComboBoxSelectionText(ComboBox comboBox)
    {
        return comboBox.SelectedItem is ComboBoxItem item
            ? item.Content?.ToString() ?? "none"
            : comboBox.SelectedItem?.ToString() ?? "none";
    }

    private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
    {
        if (SliderValueText is null)
        {
            return;
        }

        SliderValueText.Text =
            $"Slider values: standard {StandardSlider.Value:0}, tick {TickSlider.Value:0}, custom {CustomTicksSlider.Value:0}, vertical {VerticalSlider.Value:0} / {VerticalTickSlider.Value:0}";
    }

    public sealed record SearchDemoItem(string Name, string Category, string Description);
}
