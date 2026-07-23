using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NebulaControls.Demo.Views;

public partial class DemoCodeBlock : UserControl
{
    public static readonly DependencyProperty CodeProperty =
        DependencyProperty.Register(
            nameof(Code),
            typeof(string),
            typeof(DemoCodeBlock),
            new PropertyMetadata(string.Empty, OnCodeChanged));

    public static readonly DependencyProperty CodeLanguageProperty =
        DependencyProperty.Register(
            nameof(CodeLanguage),
            typeof(DemoCodeLanguage),
            typeof(DemoCodeBlock),
            new PropertyMetadata(DemoCodeLanguage.Xaml, OnCodeChanged));

    public DemoCodeBlock()
    {
        InitializeComponent();
        RenderCode();
    }

    public string Code
    {
        get => (string)GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    public DemoCodeLanguage CodeLanguage
    {
        get => (DemoCodeLanguage)GetValue(CodeLanguageProperty);
        set => SetValue(CodeLanguageProperty, value);
    }

    private static void OnCodeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is DemoCodeBlock codeBlock)
        {
            codeBlock.RenderCode();
        }
    }

    private void RenderCode()
    {
        if (CodeBox is null)
        {
            return;
        }

        CodeBox.Document = new FlowDocument
        {
            PagePadding = new Thickness(0)
        };

        var paragraph = new Paragraph
        {
            Margin = new Thickness(0),
            LineHeight = 17
        };

        CodeBox.Document.Blocks.Add(paragraph);

        foreach (var rawLine in Code.Replace("\r\n", "\n").Split('\n'))
        {
            AddHighlightedLine(paragraph, rawLine.TrimEnd());
            paragraph.Inlines.Add(new LineBreak());
        }
    }

    private void AddHighlightedLine(Paragraph paragraph, string line)
    {
        if (CodeLanguage == DemoCodeLanguage.Xaml)
        {
            AddXamlLine(paragraph, line);
            return;
        }

        AddCSharpLine(paragraph, line);
    }

    private static void AddXamlLine(Paragraph paragraph, string line)
    {
        if (line.TrimStart().StartsWith("<!--"))
        {
            paragraph.Inlines.Add(CreateRun(line, "Brush.TextMuted"));
            return;
        }

        var index = 0;
        while (index < line.Length)
        {
            var character = line[index];
            if (character is '<' or '>' or '/')
            {
                paragraph.Inlines.Add(CreateRun(character.ToString(), "Brush.BrandSoft"));
                index++;
                continue;
            }

            if (character == '"')
            {
                var end = line.IndexOf('"', index + 1);
                if (end < 0)
                {
                    end = line.Length - 1;
                }

                paragraph.Inlines.Add(CreateRun(line[index..(end + 1)], "Brush.Success"));
                index = end + 1;
                continue;
            }

            if (char.IsLetter(character) || character == ':' || character == '.')
            {
                var start = index;
                while (index < line.Length && (char.IsLetterOrDigit(line[index]) || line[index] is ':' or '.' or '_'))
                {
                    index++;
                }

                var token = line[start..index];
                var brush = char.IsUpper(token[0]) || token.Contains(':')
                    ? "Brush.BrandSoft"
                    : "Brush.TextSecondary";
                paragraph.Inlines.Add(CreateRun(token, brush));
                continue;
            }

            paragraph.Inlines.Add(CreateRun(character.ToString(), "Brush.TextSecondary"));
            index++;
        }
    }

    private static void AddCSharpLine(Paragraph paragraph, string line)
    {
        var trimmed = line.TrimStart();
        if (trimmed.StartsWith("//"))
        {
            paragraph.Inlines.Add(CreateRun(line, "Brush.TextMuted"));
            return;
        }

        var keywords = new HashSet<string>
        {
            "private", "void", "if", "is", "return", "false", "true", "string", "sender", "new"
        };

        var index = 0;
        while (index < line.Length)
        {
            if (line[index] == '"')
            {
                var end = line.IndexOf('"', index + 1);
                if (end < 0)
                {
                    end = line.Length - 1;
                }

                paragraph.Inlines.Add(CreateRun(line[index..(end + 1)], "Brush.Success"));
                index = end + 1;
                continue;
            }

            if (char.IsLetter(line[index]) || line[index] == '_')
            {
                var start = index;
                while (index < line.Length && (char.IsLetterOrDigit(line[index]) || line[index] == '_'))
                {
                    index++;
                }

                var token = line[start..index];
                paragraph.Inlines.Add(CreateRun(token, keywords.Contains(token) ? "Brush.BrandSoft" : "Brush.TextSecondary"));
                continue;
            }

            paragraph.Inlines.Add(CreateRun(line[index].ToString(), "Brush.TextSecondary"));
            index++;
        }
    }

    private static Run CreateRun(string text, string brushResourceKey)
    {
        return new Run(text)
        {
            Foreground = (Brush)Application.Current.FindResource(brushResourceKey)
        };
    }
}

public enum DemoCodeLanguage
{
    Xaml,
    CSharp
}
