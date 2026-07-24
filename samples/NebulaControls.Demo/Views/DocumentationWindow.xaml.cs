using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NebulaControls.Controls;

namespace NebulaControls.Demo.Views;

public partial class DocumentationWindow : NebulaWindow
{
    private string currentTreeViewSelection = "NebulaTreeView";
    private string lastCommittedTreeViewLeaf = "NebulaTreeView";

    private readonly SearchDocumentationItem[] searchCatalog =
    [
        new("NebulaTextBox", "Inputs", "Text input with placeholder, labels, helper text and validation states."),
        new("NebulaEmailTextBox", "Inputs", "Email input with built-in required and format validation."),
        new("NebulaPasswordBox", "Inputs", "Password input with reveal support and configurable password rules."),
        new("NebulaNumericUpDown", "Inputs", "Numeric value entry with range clamping and keyboard support."),
        new("NebulaSearchBox", "Inputs", "Search input with icon, clear action and submit event."),
        new("NebulaSlider", "Inputs", "Range input with keyboard and mouse wheel support."),
        new("NebulaProgressBar", "Progress", "Determinate and indeterminate progress indicator."),
        new("NebulaComboBox", "Selection", "Dropdown selection with editable and disabled item scenarios."),
        new("NebulaListBox", "Collections", "Selectable list with keyboard navigation and disabled items."),
        new("NebulaTreeView", "Collections", "Hierarchical selection with branch navigation and committed leaf actions."),
        new("NebulaDataGrid", "Data", "Editable grid with add row, validation and database-oriented behavior.")
    ];

    public DocumentationWindow()
    {
        InitializeComponent();
        SearchResultsList.ItemsSource = SearchPreviewItems;
        ApplySearchDocumentationFilter(string.Empty);
        ShowTextBoxDocumentation();
    }

    public ObservableCollection<SearchDocumentationItem> SearchPreviewItems { get; } = [];

    private void DocumentationTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is not TreeViewItem { Tag: string key })
        {
            return;
        }

        switch (key)
        {
            case "Buttons":
                ShowButtonsDocumentation();
                return;
            case "ChoiceInputs":
                ShowChoiceInputsDocumentation();
                return;
            case "ComboBox":
                ShowComboBoxDocumentation();
                return;
            case "EmailTextBox":
                ShowEmailTextBoxDocumentation();
                return;
            case "ListBox":
                ShowListBoxDocumentation();
                return;
            case "NumericUpDown":
                ShowNumericUpDownDocumentation();
                return;
            case "PasswordBox":
                ShowPasswordBoxDocumentation();
                return;
            case "ProgressBar":
                ShowProgressBarDocumentation();
                return;
            case "SearchBox":
                ShowSearchBoxDocumentation();
                return;
            case "Slider":
                ShowSliderDocumentation();
                return;
            case "TreeView":
                ShowTreeViewDocumentation();
                return;
            default:
                ShowTextBoxDocumentation();
                return;
        }
    }

    private void ShowTextBoxDocumentation()
    {
        if (PageTitleText is null)
        {
            return;
        }

        PageTitleText.Text = "NebulaTextBox";
        PageDescriptionText.Text = "Text input styles with label, placeholder, helper text and validation states.";
        PageMetaText.Text = "Version v1.03 - Validated";

        ShowPreviewPanel(TextBoxPreviewPanel);

        XamlCodeBlock.Code =
            """
            <!-- Standard WPF TextBox styled by NebulaControls -->
            <TextBox x:Name="DocumentationNameTextBox"
                     Style="{StaticResource NebulaTextBox}"
                     Text="Nebula text value"
                     Width="320"/>

            <!-- NebulaTextBox custom control with integrated label -->
            <nebula:NebulaTextBox x:Name="DocumentationUsernameTextBox"
                                  Style="{StaticResource NebulaLabeledTextBox}"
                                  Label="Username"
                                  Placeholder="Enter username"
                                  Text="domin"
                                  Width="320"/>

            <!-- Validation state controlled by your application code -->
            <nebula:NebulaTextBox x:Name="DocumentationRequiredTextBox"
                                  Style="{StaticResource NebulaLabeledTextBox}"
                                  Label="Required value"
                                  HelperText="Click the button below to validate this value."
                                  ErrorText="This field is required."
                                  Width="320"/>

            <Button Content="Read values"
                    Style="{StaticResource NebulaPrimaryButton}"
                    Click="ReadTextBoxValuesButton_Click"/>
            """;

        CSharpCodeBlock.Code =
            """
            private void ReadTextBoxValuesButton_Click(object sender, RoutedEventArgs e)
            {
                DocumentationRequiredTextBox.HasError =
                    string.IsNullOrWhiteSpace(DocumentationRequiredTextBox.Text);

                if (DocumentationRequiredTextBox.HasError)
                {
                    return;
                }

                var name = DocumentationNameTextBox.Text;
                var username = DocumentationUsernameTextBox.Text;
                var requiredValue = DocumentationRequiredTextBox.Text;

                TextBoxPreviewStatusText.Text =
                    $"Name: {name}, username: {username}, required: {requiredValue}";
            }
            """;
    }

    private void ShowEmailTextBoxDocumentation()
    {
        if (PageTitleText is null)
        {
            return;
        }

        PageTitleText.Text = "NebulaEmailTextBox";
        PageDescriptionText.Text = "Specialized email input with built-in format validation for common email-like values.";
        PageMetaText.Text = "Version v1.00 - Validated";

        ShowPreviewPanel(EmailTextBoxPreviewPanel);

        XamlCodeBlock.Code =
            """
            <nebula:NebulaEmailTextBox x:Name="DocumentationRequiredEmailTextBox"
                                       Style="{StaticResource NebulaLabeledEmailTextBox}"
                                       Label="Email"
                                       Placeholder="name@example.com"
                                       HelperText="Leave the field to validate the email address."
                                       IsEmailRequired="True"
                                       Width="320"/>

            <nebula:NebulaEmailTextBox x:Name="DocumentationOptionalEmailTextBox"
                                       Style="{StaticResource NebulaLabeledEmailTextBox}"
                                       Label="Recovery email"
                                       Placeholder="optional@example.com"
                                       HelperText="Empty value is accepted when IsEmailRequired is false."
                                       IsEmailRequired="False"
                                       Width="320"/>

            <Button Content="Validate emails"
                    Style="{StaticResource NebulaPrimaryButton}"
                    Click="ValidateEmailValuesButton_Click"/>
            """;

        CSharpCodeBlock.Code =
            """
            private void ValidateEmailValuesButton_Click(object sender, RoutedEventArgs e)
            {
                DocumentationRequiredEmailTextBox.ValidateEmail();
                DocumentationOptionalEmailTextBox.ValidateEmail();

                if (!DocumentationRequiredEmailTextBox.IsEmailValid
                    || !DocumentationOptionalEmailTextBox.IsEmailValid)
                {
                    EmailPreviewStatusText.Text = "At least one email is invalid.";
                    return;
                }

                EmailPreviewStatusText.Text =
                    $"Email: {DocumentationRequiredEmailTextBox.Text}; "
                    + $"Recovery: {DocumentationOptionalEmailTextBox.Text}";
            }
            """;
    }

    private void ShowButtonsDocumentation()
    {
        if (PageTitleText is null)
        {
            return;
        }

        PageTitleText.Text = "NebulaButtons";
        PageDescriptionText.Text = "Action button styles for primary, secondary, ghost, sizes and disabled states.";
        PageMetaText.Text = "Version v1.04 - Validated";

        ShowPreviewPanel(ButtonsPreviewPanel);
        ButtonsPreviewStatusText.Text = "No button action yet.";

        XamlCodeBlock.Code =
            """
            <Button x:Name="PreviewSaveButton"
                    Content="Save profile"
                    Style="{StaticResource NebulaPrimaryButton}"
                    Click="PreviewSaveButton_Click"/>

            <Button x:Name="PreviewCancelButton"
                    Content="Cancel"
                    Style="{StaticResource NebulaSecondaryButton}"
                    Click="PreviewCancelButton_Click"/>

            <Button x:Name="PreviewResetButton"
                    Content="Reset"
                    Style="{StaticResource NebulaGhostButton}"
                    Click="PreviewResetButton_Click"/>

            <Button Content="Small"
                    Style="{StaticResource NebulaPrimarySmallButton}"/>

            <Button Content="Large"
                    Style="{StaticResource NebulaPrimaryLargeButton}"/>
            """;

        CSharpCodeBlock.Code =
            """
            private void PreviewSaveButton_Click(object sender, RoutedEventArgs e)
            {
                ButtonsPreviewStatusText.Text = "Save profile clicked.";
            }

            private void PreviewCancelButton_Click(object sender, RoutedEventArgs e)
            {
                ButtonsPreviewStatusText.Text = "Cancel clicked.";
            }

            private void PreviewResetButton_Click(object sender, RoutedEventArgs e)
            {
                ButtonsPreviewStatusText.Text = "No button action yet.";
            }
            """;
    }

    private void ShowComboBoxDocumentation()
    {
        if (PageTitleText is null)
        {
            return;
        }

        PageTitleText.Text = "NebulaComboBox";
        PageDescriptionText.Text = "Dropdown selection control with standard, editable, disabled-item and disabled-control scenarios.";
        PageMetaText.Text = "Version v1.06 - Validated";

        ShowPreviewPanel(ComboBoxPreviewPanel);
        ComboBoxPreviewStatusText.Text = $"Selected value: {GetComboBoxSelectionText(ThemeComboBox)}";

        XamlCodeBlock.Code =
            """
            <nebula:NebulaComboBox x:Name="ThemeComboBox"
                                   Style="{StaticResource NebulaComboBox}"
                                   SelectedIndex="0"
                                   Tag="Theme"
                                   SelectionChanged="DocumentationComboBox_SelectionChanged"
                                   Width="300">
                <ComboBoxItem Content="Nebula Dark Purple"/>
                <ComboBoxItem Content="Nebula Dark Blue"/>
                <ComboBoxItem Content="Nebula Light Purple"/>
            </nebula:NebulaComboBox>

            <nebula:NebulaComboBox x:Name="StatusComboBox"
                                   Style="{StaticResource NebulaComboBox}"
                                   SelectedIndex="0"
                                   Tag="Status"
                                   SelectionChanged="DocumentationComboBox_SelectionChanged"
                                   Width="300">
                <ComboBoxItem Content="Ready"/>
                <ComboBoxItem Content="Archived"
                              IsEnabled="False"/>
                <ComboBoxItem Content="Reviewing"/>
            </nebula:NebulaComboBox>

            <nebula:NebulaComboBox x:Name="EditableThemeComboBox"
                                   Style="{StaticResource NebulaComboBox}"
                                   IsEditable="True"
                                   SelectedIndex="0"
                                   Tag="Editable theme"
                                   SelectionChanged="DocumentationComboBox_SelectionChanged"
                                   Width="300">
                <ComboBoxItem Content="Dark Purple"/>
                <ComboBoxItem Content="Dark Blue"/>
                <ComboBoxItem Content="Light Purple"/>
            </nebula:NebulaComboBox>

            <Button Content="Read selected values"
                    Style="{StaticResource NebulaPrimaryButton}"
                    Click="ReadComboBoxValuesButton_Click"/>
            """;

        CSharpCodeBlock.Code =
            """
            private void DocumentationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (ComboBoxPreviewStatusText is null || sender is not ComboBox comboBox)
                {
                    return;
                }

                var source = comboBox.Tag?.ToString() ?? "ComboBox";
                ComboBoxPreviewStatusText.Text =
                    $"{source} selected value: {GetComboBoxSelectionText(comboBox)}";
            }

            private void ReadComboBoxValuesButton_Click(object sender, RoutedEventArgs e)
            {
                var theme = GetComboBoxSelectionText(ThemeComboBox);
                var status = GetComboBoxSelectionText(StatusComboBox);
                var editableTheme = GetComboBoxSelectionText(EditableThemeComboBox);

                ComboBoxPreviewStatusText.Text =
                    $"Theme: {theme}; status: {status}; editable: {editableTheme}";
            }

            private static string GetComboBoxSelectionText(ComboBox comboBox)
            {
                return comboBox.SelectedItem is ComboBoxItem item
                    ? item.Content?.ToString() ?? "none"
                    : comboBox.SelectedItem?.ToString() ?? comboBox.Text;
            }
            """;
    }

    private void ShowChoiceInputsDocumentation()
    {
        if (PageTitleText is null)
        {
            return;
        }

        PageTitleText.Text = "Nebula choice inputs";
        PageDescriptionText.Text = "Boolean and exclusive selection styles using WPF CheckBox, RadioButton and ToggleButton with Nebula visual resources.";
        PageMetaText.Text = "Version v1.05 - Validated";

        ShowPreviewPanel(ChoiceInputsPreviewPanel);
        ChoiceInputsPreviewStatusText.Text = "Choice values have not been read yet.";

        XamlCodeBlock.Code =
            """
            <CheckBox x:Name="ReceiveUpdatesCheckBox"
                      Content="Receive product updates"
                      Style="{StaticResource NebulaCheckBox}"
                      IsChecked="True"/>

            <CheckBox x:Name="EnableTelemetryCheckBox"
                      Content="Share anonymous diagnostics"
                      Style="{StaticResource NebulaCheckBox}"/>

            <RadioButton x:Name="CompactLayoutRadioButton"
                         Content="Compact layout"
                         GroupName="DocumentationLayoutMode"
                         Style="{StaticResource NebulaRadioButton}"
                         IsChecked="True"/>

            <RadioButton x:Name="ComfortableLayoutRadioButton"
                         Content="Comfortable layout"
                         GroupName="DocumentationLayoutMode"
                         Style="{StaticResource NebulaRadioButton}"/>

            <ToggleButton x:Name="NotificationsToggleButton"
                          Content="Notifications enabled"
                          Style="{StaticResource NebulaToggleButton}"
                          IsChecked="True"/>

            <ToggleButton x:Name="SyncToggleButton"
                          Content="Cloud sync"
                          Style="{StaticResource NebulaToggleButton}"/>

            <Button Content="Read choice values"
                    Style="{StaticResource NebulaPrimaryButton}"
                    Click="ReadChoiceValuesButton_Click"/>
            """;

        CSharpCodeBlock.Code =
            """
            private void ReadChoiceValuesButton_Click(object sender, RoutedEventArgs e)
            {
                var receiveUpdates = ReceiveUpdatesCheckBox.IsChecked == true;
                var telemetryEnabled = EnableTelemetryCheckBox.IsChecked == true;
                var layout = CompactLayoutRadioButton.IsChecked == true
                    ? "Compact"
                    : "Comfortable";
                var notificationsEnabled = NotificationsToggleButton.IsChecked == true;
                var syncEnabled = SyncToggleButton.IsChecked == true;

                ChoiceInputsPreviewStatusText.Text =
                    $"Receive updates: {receiveUpdates}\n"
                    + $"Share diagnostics: {telemetryEnabled}\n"
                    + $"Layout: {layout}\n"
                    + $"Notifications: {notificationsEnabled}\n"
                    + $"Cloud sync: {syncEnabled}";
            }
            """;
    }

    private void ShowNumericUpDownDocumentation()
    {
        if (PageTitleText is null)
        {
            return;
        }

        PageTitleText.Text = "NebulaNumericUpDown";
        PageDescriptionText.Text = "Numeric input with min/max range, step buttons, keyboard handling, mouse wheel support and automatic range clamping.";
        PageMetaText.Text = "Version v1.04 - Validated";

        ShowPreviewPanel(NumericUpDownPreviewPanel);
        NumericPreviewStatusText.Text = "Numeric values have not been calculated yet.";

        XamlCodeBlock.Code =
            """
            <nebula:NebulaNumericUpDown x:Name="CartQuantityInput"
                                        Style="{StaticResource NebulaNumericUpDown}"
                                        Value="2"
                                        Minimum="0"
                                        Maximum="10"
                                        Step="1"
                                        Placeholder="Quantity"
                                        Width="180"/>

            <nebula:NebulaNumericUpDown x:Name="DiscountPercentInput"
                                        Style="{StaticResource NebulaNumericUpDown}"
                                        Value="5"
                                        Minimum="0"
                                        Maximum="50"
                                        Step="0.5"
                                        Placeholder="Discount"
                                        Width="180"/>

            <nebula:NebulaNumericUpDown x:Name="AvailableStockInput"
                                        Style="{StaticResource NebulaNumericUpDown}"
                                        Value="24"
                                        Minimum="0"
                                        Maximum="999"
                                        IsReadOnly="True"
                                        Width="180"/>

            <Button Content="Calculate total"
                    Style="{StaticResource NebulaPrimaryButton}"
                    Click="CalculateNumericValuesButton_Click"/>
            """;

        CSharpCodeBlock.Code =
            """
            private void CalculateNumericValuesButton_Click(object sender, RoutedEventArgs e)
            {
                var unitPrice = 19.99d;
                var quantity = CartQuantityInput.Value ?? 0d;
                var discountPercent = DiscountPercentInput.Value ?? 0d;
                var availableStock = AvailableStockInput.Value ?? 0d;

                var subtotal = unitPrice * quantity;
                var total = subtotal * (1d - discountPercent / 100d);

                NumericPreviewStatusText.Text =
                    $"Quantity: {quantity}; stock: {availableStock}; "
                    + $"discount: {discountPercent}%; total: {total:0.00}";
            }
            """;
    }

    private void ShowPasswordBoxDocumentation()
    {
        if (PageTitleText is null)
        {
            return;
        }

        PageTitleText.Text = "NebulaPasswordBox";
        PageDescriptionText.Text = "Password input styles with a styled WPF PasswordBox and a Nebula labeled variant for helper text, reveal and configurable password rules.";
        PageMetaText.Text = "Version v1.04 - Validated";

        ShowPreviewPanel(PasswordBoxPreviewPanel);
        PasswordPreviewStatusText.Text = "Password values have not been read yet.";

        XamlCodeBlock.Code =
            """
            <!-- Standard WPF PasswordBox styled by NebulaControls -->
            <PasswordBox x:Name="LoginPasswordBox"
                         Style="{StaticResource NebulaPasswordBox}"
                         Password="nebula"
                         Width="320"/>

            <!-- Nebula custom control with label, helper text and reveal support -->
            <nebula:NebulaLabeledPasswordBox x:Name="SecurePasswordBox"
                                             Style="{StaticResource NebulaLabeledPasswordBox}"
                                             Label="Secure password"
                                             Password="nebula!"
                                             HelperText="Use at least 8 characters and one special character."
                                             IsPasswordRequired="True"
                                             PasswordRule="min:8;special"
                                             InvalidPasswordText="Use at least 8 characters and one special character."
                                             Width="320"/>

            <nebula:NebulaLabeledPasswordBox x:Name="AccountPasswordBox"
                                             Style="{StaticResource NebulaLabeledPasswordBox}"
                                             Label="Account password"
                                             Password="Nebula@1"
                                             HelperText="Use 8+ chars with upper, lower, digit and special character."
                                             IsPasswordRequired="True"
                                             PasswordRule="min:8;upper;lower;digit;special"
                                             InvalidPasswordText="Use 8+ chars with upper, lower, digit and special character."
                                             Width="320"/>

            <Button Content="Read password values"
                    Style="{StaticResource NebulaPrimaryButton}"
                    Click="ReadPasswordValuesButton_Click"/>
            """;

        CSharpCodeBlock.Code =
            """
            private void ReadPasswordValuesButton_Click(object sender, RoutedEventArgs e)
            {
                var loginPassword = LoginPasswordBox.Password;
                var securePassword = SecurePasswordBox.Password;
                var accountPassword = AccountPasswordBox.Password;

                SecurePasswordBox.ValidatePassword();
                AccountPasswordBox.ValidatePassword();

                if (!SecurePasswordBox.IsPasswordValid
                    || !AccountPasswordBox.IsPasswordValid)
                {
                    PasswordPreviewStatusText.Text = "At least one password does not match its rule.";
                    return;
                }

                PasswordPreviewStatusText.Text =
                    $"Login: {loginPassword}; secure: {securePassword}; account: {accountPassword}";
            }
            """;
    }

    private void ShowSearchBoxDocumentation()
    {
        if (PageTitleText is null)
        {
            return;
        }

        PageTitleText.Text = "NebulaSearchBox";
        PageDescriptionText.Text = "Search input with placeholder, clear action, live text change notification and submit behavior on Enter.";
        PageMetaText.Text = "Version v1.02 - Validated";

        ShowPreviewPanel(SearchBoxPreviewPanel);
        ApplySearchDocumentationFilter(ControlCatalogSearchBox.SearchText);

        XamlCodeBlock.Code =
            """
            <nebula:NebulaSearchBox x:Name="ControlCatalogSearchBox"
                                    Style="{StaticResource NebulaSearchBox}"
                                    Placeholder="Search name, category, description..."
                                    SearchTextChanged="ControlCatalogSearchBox_SearchTextChanged"
                                    SearchSubmitted="ControlCatalogSearchBox_SearchSubmitted"
                                    Width="360"/>

            <TextBlock x:Name="SearchBoxPreviewStatusText"
                       Text="Type to filter the list, press Enter to submit, Esc to clear."/>

            <nebula:NebulaListBox x:Name="SearchResultsList"
                                  Style="{StaticResource NebulaListBox}"
                                  Width="360"
                                  Height="230"/>
            """;

        CSharpCodeBlock.Code =
            """
            public ObservableCollection<SearchDocumentationItem> SearchPreviewItems { get; } = [];

            public DocumentationWindow()
            {
                InitializeComponent();
                SearchResultsList.ItemsSource = SearchPreviewItems;
                ApplySearchDocumentationFilter(string.Empty);
            }

            private void ControlCatalogSearchBox_SearchTextChanged(object sender, EventArgs e)
            {
                if (sender is not NebulaSearchBox searchBox)
                {
                    return;
                }

                ApplySearchDocumentationFilter(searchBox.SearchText);
            }

            private void ControlCatalogSearchBox_SearchSubmitted(object sender, EventArgs e)
            {
                if (sender is not NebulaSearchBox searchBox)
                {
                    return;
                }

                SearchBoxPreviewStatusText.Text = string.IsNullOrWhiteSpace(searchBox.SearchText)
                    ? "Search reset."
                    : $"Submitted search: {searchBox.SearchText}";
            }

            private void ApplySearchDocumentationFilter(string searchText)
            {
                SearchPreviewItems.Clear();

                var matches = string.IsNullOrWhiteSpace(searchText)
                    ? searchCatalog
                    : searchCatalog.Where(control =>
                        control.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)
                        || control.Category.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)
                        || control.Description.Contains(searchText, StringComparison.CurrentCultureIgnoreCase));

                foreach (var control in matches)
                {
                    SearchPreviewItems.Add(control);
                }

                SearchBoxPreviewStatusText.Text = SearchPreviewItems.Count == 0
                    ? "No control found."
                    : $"{SearchPreviewItems.Count} control(s) displayed.";
            }

            public sealed record SearchDocumentationItem(
                string Name,
                string Category,
                string Description);
            """;
    }

    private void ShowSliderDocumentation()
    {
        if (PageTitleText is null)
        {
            return;
        }

        PageTitleText.Text = "NebulaSlider";
        PageDescriptionText.Text = "Range input for selecting numeric values with keyboard, mouse and mouse wheel support.";
        PageMetaText.Text = "Version v1.07 - Validated";

        ShowPreviewPanel(SliderPreviewPanel);
        UpdateSliderPreviewStatus();

        XamlCodeBlock.Code =
            """
            <nebula:NebulaSlider x:Name="DocumentationVolumeSlider"
                                 Style="{StaticResource NebulaSlider}"
                                 Minimum="0"
                                 Maximum="100"
                                 Value="42"
                                 TickFrequency="10"
                                 TickPlacement="BottomRight"
                                 ValueChanged="DocumentationSlider_ValueChanged"
                                 Width="340"/>

            <nebula:NebulaSlider x:Name="DocumentationBrightnessSlider"
                                 Style="{StaticResource NebulaSlider}"
                                 Minimum="0"
                                 Maximum="100"
                                 Value="60"
                                 Ticks="0,25,50,75,100"
                                 TickPlacement="BottomRight"
                                 ValueChanged="DocumentationSlider_ValueChanged"
                                 Width="340"/>

            <Button Content="Read slider values"
                    Style="{StaticResource NebulaPrimaryButton}"
                    Click="ReadSliderValuesButton_Click"/>
            """;

        CSharpCodeBlock.Code =
            """
            private void DocumentationSlider_ValueChanged(
                object sender,
                RoutedPropertyChangedEventArgs<double> e)
            {
                UpdateSliderPreviewStatus();
            }

            private void ReadSliderValuesButton_Click(object sender, RoutedEventArgs e)
            {
                UpdateSliderPreviewStatus();
            }

            private void UpdateSliderPreviewStatus()
            {
                SliderPreviewStatusText.Text =
                    $"Slider values: volume {DocumentationVolumeSlider.Value:0}, "
                    + $"brightness {DocumentationBrightnessSlider.Value:0}.";
            }
            """;
    }

    private void ShowProgressBarDocumentation()
    {
        if (PageTitleText is null)
        {
            return;
        }

        PageTitleText.Text = "NebulaProgressBar";
        PageDescriptionText.Text = "Progress indicator for determinate values, indeterminate activity and disabled states.";
        PageMetaText.Text = "Version v1.03 - Validated";

        ShowPreviewPanel(ProgressBarPreviewPanel);
        UpdateProgressPreviewStatus();

        XamlCodeBlock.Code =
            """
            <nebula:NebulaProgressBar x:Name="DocumentationProgressBar"
                                      Style="{StaticResource NebulaProgressBar}"
                                      Minimum="0"
                                      Maximum="100"
                                      Value="64"
                                      ShowValueText="True"
                                      Width="340"/>

            <Button Content="Advance"
                    Style="{StaticResource NebulaPrimaryButton}"
                    Click="AdvanceProgressButton_Click"/>

            <Button Content="Reset"
                    Style="{StaticResource NebulaSecondaryButton}"
                    Click="ResetProgressButton_Click"/>

            <nebula:NebulaProgressBar Style="{StaticResource NebulaProgressBar}"
                                      IsIndeterminate="True"
                                      Width="340"/>
            """;

        CSharpCodeBlock.Code =
            """
            private void AdvanceProgressButton_Click(object sender, RoutedEventArgs e)
            {
                DocumentationProgressBar.Value =
                    DocumentationProgressBar.Value >= DocumentationProgressBar.Maximum
                        ? DocumentationProgressBar.Maximum
                        : DocumentationProgressBar.Value + 10;

                UpdateProgressPreviewStatus();
            }

            private void ResetProgressButton_Click(object sender, RoutedEventArgs e)
            {
                DocumentationProgressBar.Value = 0;
                UpdateProgressPreviewStatus();
            }

            private void UpdateProgressPreviewStatus()
            {
                ProgressPreviewStatusText.Text =
                    $"Progress value: {DocumentationProgressBar.Value:0}%.";
            }
            """;
    }

    private void ShowListBoxDocumentation()
    {
        if (PageTitleText is null)
        {
            return;
        }

        PageTitleText.Text = "NebulaListBox";
        PageDescriptionText.Text = "Selectable list control with Nebula item styling, keyboard navigation, disabled items and smooth item scrolling.";
        PageMetaText.Text = "Version v1.05 - Validated";

        ShowPreviewPanel(ListBoxPreviewPanel);
        UpdateListBoxPreviewStatus();

        XamlCodeBlock.Code =
            """
            <nebula:NebulaListBox x:Name="DocumentationListBox"
                                  Style="{StaticResource NebulaListBox}"
                                  SelectedIndex="1"
                                  SelectionChanged="DocumentationListBox_SelectionChanged"
                                  Width="340"
                                  Height="180">
                <ListBoxItem Content="Buttons"/>
                <ListBoxItem Content="Inputs"/>
                <ListBoxItem Content="Selection"/>
                <ListBoxItem Content="Data"/>
                <ListBoxItem Content="Navigation"/>
                <ListBoxItem Content="Disabled"
                             IsEnabled="False"/>
                <ListBoxItem Content="After disabled"/>
            </nebula:NebulaListBox>

            <Button Content="Read selected item"
                    Style="{StaticResource NebulaPrimaryButton}"
                    Click="ReadListBoxSelectionButton_Click"/>
            """;

        CSharpCodeBlock.Code =
            """
            private void DocumentationListBox_SelectionChanged(
                object sender,
                SelectionChangedEventArgs e)
            {
                UpdateListBoxPreviewStatus();
            }

            private void ReadListBoxSelectionButton_Click(object sender, RoutedEventArgs e)
            {
                UpdateListBoxPreviewStatus();
            }

            private void UpdateListBoxPreviewStatus()
            {
                var selectedText = DocumentationListBox.SelectedItem is ListBoxItem item
                    ? item.Content?.ToString() ?? "none"
                    : DocumentationListBox.SelectedItem?.ToString() ?? "none";

                ListBoxPreviewStatusText.Text = $"Selected item: {selectedText}.";
            }
            """;
    }

    private void ShowTreeViewDocumentation()
    {
        if (PageTitleText is null)
        {
            return;
        }

        PageTitleText.Text = "NebulaTreeView";
        PageDescriptionText.Text = "Hierarchical navigation control where branches expand/collapse and leaf items can commit an application action.";
        PageMetaText.Text = "Version v1.06 - Validated";

        ShowPreviewPanel(TreeViewPreviewPanel);
        currentTreeViewSelection = GetTreeViewSelectionText(DocumentationPreviewTreeView.SelectedItem);
        lastCommittedTreeViewLeaf = "NebulaTreeView";
        UpdateTreeViewPreviewStatus();

        XamlCodeBlock.Code =
            """
            <nebula:NebulaTreeView x:Name="DocumentationPreviewTreeView"
                                   Style="{StaticResource NebulaTreeView}"
                                   CommitBranchItems="False"
                                   SelectedItemChanged="DocumentationPreviewTreeView_SelectedItemChanged"
                                   SelectionCommitted="DocumentationPreviewTreeView_SelectionCommitted"
                                   Width="360"
                                   Height="250">
                <TreeViewItem Header="NebulaControls"
                              IsExpanded="True"
                              Style="{StaticResource NebulaTreeViewItem}">
                    <TreeViewItem Header="Inputs"
                                  IsExpanded="True"
                                  Style="{StaticResource NebulaTreeViewItem}">
                        <TreeViewItem Header="NebulaTextBox"
                                      Style="{StaticResource NebulaTreeViewItem}"/>
                        <TreeViewItem Header="NebulaPasswordBox"
                                      Style="{StaticResource NebulaTreeViewItem}"/>
                    </TreeViewItem>
                    <TreeViewItem Header="Collections"
                                  IsExpanded="True"
                                  Style="{StaticResource NebulaTreeViewItem}">
                        <TreeViewItem Header="NebulaListBox"
                                      Style="{StaticResource NebulaTreeViewItem}"/>
                        <TreeViewItem Header="NebulaTreeView"
                                      Style="{StaticResource NebulaTreeViewItem}"/>
                    </TreeViewItem>
                </TreeViewItem>
            </nebula:NebulaTreeView>
            """;

            CSharpCodeBlock.Code =
            """
            private string currentTreeViewSelection = "NebulaTreeView";
            private string lastCommittedTreeViewLeaf = "NebulaTreeView";

            private void DocumentationPreviewTreeView_SelectedItemChanged(
                object sender,
                RoutedPropertyChangedEventArgs<object> e)
            {
                currentTreeViewSelection = GetTreeViewSelectionText(e.NewValue);
                UpdateTreeViewPreviewStatus();
            }

            private void DocumentationPreviewTreeView_SelectionCommitted(
                object sender,
                RoutedPropertyChangedEventArgs<object> e)
            {
                lastCommittedTreeViewLeaf = GetTreeViewSelectionText(e.NewValue);
                UpdateTreeViewPreviewStatus();
            }

            private void UpdateTreeViewPreviewStatus()
            {
                TreeViewPreviewStatusText.Text =
                    $"Current selection: {currentTreeViewSelection}. "
                    + $"Last committed leaf: {lastCommittedTreeViewLeaf}.";
            }

            private static string GetTreeViewSelectionText(object? item)
            {
                return item is TreeViewItem treeViewItem
                    ? treeViewItem.Header?.ToString() ?? "Tree item"
                    : "none";
            }
            """;
    }

    private void ReadTextBoxValuesButton_Click(object sender, RoutedEventArgs e)
    {
        DocumentationRequiredTextBox.HasError =
            string.IsNullOrWhiteSpace(DocumentationRequiredTextBox.Text);

        if (DocumentationRequiredTextBox.HasError)
        {
            TextBoxPreviewStatusText.Text = "Required value is missing.";
            return;
        }

        var name = DocumentationNameTextBox.Text;
        var username = DocumentationUsernameTextBox.Text;
        var requiredValue = DocumentationRequiredTextBox.Text;

        TextBoxPreviewStatusText.Text =
            $"Name: {name}, username: {username}, required: {requiredValue}";
    }

    private void ValidateEmailValuesButton_Click(object sender, RoutedEventArgs e)
    {
        DocumentationRequiredEmailTextBox.ValidateEmail();
        DocumentationOptionalEmailTextBox.ValidateEmail();

        if (!DocumentationRequiredEmailTextBox.IsEmailValid
            || !DocumentationOptionalEmailTextBox.IsEmailValid)
        {
            EmailPreviewStatusText.Text = "At least one email is invalid.";
            return;
        }

        EmailPreviewStatusText.Text =
            $"Email: {DocumentationRequiredEmailTextBox.Text}; "
            + $"Recovery: {DocumentationOptionalEmailTextBox.Text}";
    }

    private void ReadPasswordValuesButton_Click(object sender, RoutedEventArgs e)
    {
        var loginPassword = LoginPasswordBox.Password;
        var securePassword = SecurePasswordBox.Password;
        var accountPassword = AccountPasswordBox.Password;

        SecurePasswordBox.ValidatePassword();
        AccountPasswordBox.ValidatePassword();

        if (!SecurePasswordBox.IsPasswordValid
            || !AccountPasswordBox.IsPasswordValid)
        {
            PasswordPreviewStatusText.Text = "At least one password does not match its rule.";
            return;
        }

        PasswordPreviewStatusText.Text =
            $"Login: {loginPassword}; secure: {securePassword}; account: {accountPassword}";
    }

    private void CalculateNumericValuesButton_Click(object sender, RoutedEventArgs e)
    {
        var unitPrice = 19.99d;
        var quantity = CartQuantityInput.Value ?? 0d;
        var discountPercent = DiscountPercentInput.Value ?? 0d;
        var availableStock = AvailableStockInput.Value ?? 0d;

        var subtotal = unitPrice * quantity;
        var total = subtotal * (1d - discountPercent / 100d);

        NumericPreviewStatusText.Text =
            $"Quantity: {quantity}; stock: {availableStock}; "
            + $"discount: {discountPercent}%; total: {total:0.00}";
    }

    private void ControlCatalogSearchBox_SearchTextChanged(object sender, System.EventArgs e)
    {
        if (sender is not NebulaSearchBox searchBox)
        {
            return;
        }

        ApplySearchDocumentationFilter(searchBox.SearchText);
    }

    private void ControlCatalogSearchBox_SearchSubmitted(object sender, System.EventArgs e)
    {
        if (sender is not NebulaSearchBox searchBox)
        {
            return;
        }

        SearchBoxPreviewStatusText.Text = string.IsNullOrWhiteSpace(searchBox.SearchText)
            ? "Search reset."
            : $"Submitted search: {searchBox.SearchText}";
    }

    private void DocumentationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxPreviewStatusText is null || sender is not ComboBox comboBox)
        {
            return;
        }

        var source = comboBox.Tag?.ToString() ?? "ComboBox";
        ComboBoxPreviewStatusText.Text =
            $"{source} selected value: {GetComboBoxSelectionText(comboBox)}";
    }

    private void ReadComboBoxValuesButton_Click(object sender, RoutedEventArgs e)
    {
        var theme = GetComboBoxSelectionText(ThemeComboBox);
        var status = GetComboBoxSelectionText(StatusComboBox);
        var editableTheme = GetComboBoxSelectionText(EditableThemeComboBox);

        ComboBoxPreviewStatusText.Text =
            $"Theme: {theme}; status: {status}; editable: {editableTheme}";
    }

    private void ReadChoiceValuesButton_Click(object sender, RoutedEventArgs e)
    {
        var receiveUpdates = ReceiveUpdatesCheckBox.IsChecked == true;
        var telemetryEnabled = EnableTelemetryCheckBox.IsChecked == true;
        var layout = CompactLayoutRadioButton.IsChecked == true
            ? "Compact"
            : "Comfortable";
        var notificationsEnabled = NotificationsToggleButton.IsChecked == true;
        var syncEnabled = SyncToggleButton.IsChecked == true;

        ChoiceInputsPreviewStatusText.Text =
            $"Receive updates: {receiveUpdates}\n"
            + $"Share diagnostics: {telemetryEnabled}\n"
            + $"Layout: {layout}\n"
            + $"Notifications: {notificationsEnabled}\n"
            + $"Cloud sync: {syncEnabled}";
    }

    private void DocumentationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SliderPreviewStatusText is null)
        {
            return;
        }

        UpdateSliderPreviewStatus();
    }

    private void ReadSliderValuesButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateSliderPreviewStatus();
    }

    private void AdvanceProgressButton_Click(object sender, RoutedEventArgs e)
    {
        DocumentationProgressBar.Value =
            DocumentationProgressBar.Value >= DocumentationProgressBar.Maximum
                ? DocumentationProgressBar.Maximum
                : DocumentationProgressBar.Value + 10;

        UpdateProgressPreviewStatus();
    }

    private void ResetProgressButton_Click(object sender, RoutedEventArgs e)
    {
        DocumentationProgressBar.Value = 0;
        UpdateProgressPreviewStatus();
    }

    private void DocumentationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListBoxPreviewStatusText is null)
        {
            return;
        }

        UpdateListBoxPreviewStatus();
    }

    private void ReadListBoxSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateListBoxPreviewStatus();
    }

    private void DocumentationPreviewTreeView_SelectionCommitted(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (TreeViewPreviewStatusText is null)
        {
            return;
        }

        lastCommittedTreeViewLeaf = GetTreeViewSelectionText(e.NewValue);
        UpdateTreeViewPreviewStatus();
    }

    private void DocumentationPreviewTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (TreeViewPreviewStatusText is null)
        {
            return;
        }

        currentTreeViewSelection = GetTreeViewSelectionText(e.NewValue);
        UpdateTreeViewPreviewStatus();
    }

    private void PreviewSaveButton_Click(object sender, RoutedEventArgs e)
    {
        ButtonsPreviewStatusText.Text = "Save profile clicked.";
    }

    private void PreviewCancelButton_Click(object sender, RoutedEventArgs e)
    {
        ButtonsPreviewStatusText.Text = "Cancel clicked.";
    }

    private void PreviewResetButton_Click(object sender, RoutedEventArgs e)
    {
        ButtonsPreviewStatusText.Text = "No button action yet.";
    }

    private void ShowPreviewPanel(UIElement visiblePanel)
    {
        TextBoxPreviewPanel.Visibility = Visibility.Collapsed;
        ChoiceInputsPreviewPanel.Visibility = Visibility.Collapsed;
        ComboBoxPreviewPanel.Visibility = Visibility.Collapsed;
        EmailTextBoxPreviewPanel.Visibility = Visibility.Collapsed;
        NumericUpDownPreviewPanel.Visibility = Visibility.Collapsed;
        PasswordBoxPreviewPanel.Visibility = Visibility.Collapsed;
        SearchBoxPreviewPanel.Visibility = Visibility.Collapsed;
        ButtonsPreviewPanel.Visibility = Visibility.Collapsed;
        ProgressBarPreviewPanel.Visibility = Visibility.Collapsed;
        SliderPreviewPanel.Visibility = Visibility.Collapsed;
        ListBoxPreviewPanel.Visibility = Visibility.Collapsed;
        TreeViewPreviewPanel.Visibility = Visibility.Collapsed;

        visiblePanel.Visibility = Visibility.Visible;
    }

    private void UpdateSliderPreviewStatus()
    {
        if (SliderPreviewStatusText is null)
        {
            return;
        }

        SliderPreviewStatusText.Text =
            $"Slider values: volume {DocumentationVolumeSlider.Value:0}, "
            + $"brightness {DocumentationBrightnessSlider.Value:0}.";
    }

    private void UpdateProgressPreviewStatus()
    {
        if (ProgressPreviewStatusText is null)
        {
            return;
        }

        ProgressPreviewStatusText.Text =
            $"Progress value: {DocumentationProgressBar.Value:0}%.";
    }

    private void UpdateListBoxPreviewStatus()
    {
        if (ListBoxPreviewStatusText is null)
        {
            return;
        }

        var selectedText = DocumentationListBox.SelectedItem is ListBoxItem item
            ? item.Content?.ToString() ?? "none"
            : DocumentationListBox.SelectedItem?.ToString() ?? "none";

        ListBoxPreviewStatusText.Text = $"Selected item: {selectedText}.";
    }

    private void UpdateTreeViewPreviewStatus()
    {
        if (TreeViewPreviewStatusText is null)
        {
            return;
        }

        TreeViewPreviewStatusText.Text =
            $"Current selection: {currentTreeViewSelection}. "
            + $"Last committed leaf: {lastCommittedTreeViewLeaf}.";
    }

    private static string GetTreeViewSelectionText(object? item)
    {
        return item is TreeViewItem treeViewItem
            ? treeViewItem.Header?.ToString() ?? "Tree item"
            : "none";
    }

    private void ApplySearchDocumentationFilter(string searchText)
    {
        SearchPreviewItems.Clear();

        var matches = string.IsNullOrWhiteSpace(searchText)
            ? searchCatalog
            : searchCatalog.Where(control =>
                control.Name.Contains(searchText, System.StringComparison.CurrentCultureIgnoreCase)
                || control.Category.Contains(searchText, System.StringComparison.CurrentCultureIgnoreCase)
                || control.Description.Contains(searchText, System.StringComparison.CurrentCultureIgnoreCase));

        foreach (var control in matches)
        {
            SearchPreviewItems.Add(control);
        }

        SearchBoxPreviewStatusText.Text = SearchPreviewItems.Count == 0
            ? "No control found."
            : $"{SearchPreviewItems.Count} control(s) displayed.";
    }

    private static string GetComboBoxSelectionText(ComboBox comboBox)
    {
        return comboBox.SelectedItem is ComboBoxItem item
            ? item.Content?.ToString() ?? "none"
            : comboBox.SelectedItem?.ToString() ?? comboBox.Text;
    }
}

public sealed record SearchDocumentationItem(string Name, string Category, string Description);
