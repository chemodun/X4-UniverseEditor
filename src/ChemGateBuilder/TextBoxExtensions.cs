using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChemGateBuilder
{
    public static class TextBoxExtensions
    {
        // Existing IsIntegerOnly attached property
        public static readonly DependencyProperty IsIntegerOnlyProperty =
            DependencyProperty.RegisterAttached(
                "IsIntegerOnly",
                typeof(bool),
                typeof(TextBoxExtensions),
                new UIPropertyMetadata(false, OnIsIntegerOnlyChanged));

        public static bool GetIsIntegerOnly(TextBox textBox)
        {
            return (bool)textBox.GetValue(IsIntegerOnlyProperty);
        }

        public static void SetIsIntegerOnly(TextBox textBox, bool value)
        {
            textBox.SetValue(IsIntegerOnlyProperty, value);
        }

        // New MinValue attached property
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.RegisterAttached(
                "MinValue",
                typeof(int),
                typeof(TextBoxExtensions),
                new UIPropertyMetadata(int.MinValue));

        public static int GetMinValue(TextBox textBox)
        {
            return (int)textBox.GetValue(MinValueProperty);
        }

        public static void SetMinValue(TextBox textBox, int value)
        {
            textBox.SetValue(MinValueProperty, value);
        }

        // New MaxValue attached property
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.RegisterAttached(
                "MaxValue",
                typeof(int),
                typeof(TextBoxExtensions),
                new UIPropertyMetadata(int.MaxValue));

        public static int GetMaxValue(TextBox textBox)
        {
            return (int)textBox.GetValue(MaxValueProperty);
        }

        public static void SetMaxValue(TextBox textBox, int value)
        {
            textBox.SetValue(MaxValueProperty, value);
        }

        private static void OnIsIntegerOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                bool isIntegerOnly = (bool)e.NewValue;

                if (isIntegerOnly)
                {
                    textBox.PreviewTextInput += TextBox_PreviewTextInput;
                    DataObject.AddPastingHandler(textBox, TextBox_Pasting);
                }
                else
                {
                    textBox.PreviewTextInput -= TextBox_PreviewTextInput;
                    DataObject.RemovePastingHandler(textBox, TextBox_Pasting);
                }
            }
        }

        // Event to communicate validation messages
        public static event Action<string>? OnValidationError;
        // Updated regex to allow optional leading minus sign for negative integers
        private static readonly Regex _regex = new Regex(@"^-?[0-9]*$");

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string fullText = GetFullTextAfterInput(textBox, e.Text);
                if (!_regex.IsMatch(fullText))
                {
                    OnValidationError?.Invoke("Only integer values are allowed.");
                    e.Handled = true;
                    return;
                }

                // Attempt to parse the full text
                if (int.TryParse(fullText, out int value))
                {
                    int min = GetMinValue(textBox);
                    int max = GetMaxValue(textBox);

                    if (value < min || value > max)
                    {
                        OnValidationError?.Invoke($"Value must be between {min} and {max}.");
                        e.Handled = true;
                    } else {
                        OnValidationError?.Invoke(string.Empty);
                    }
                }
                else
                {
                    // If parsing fails (e.g., only a minus sign), allow input
                    // unless it's just "-", which is invalid but might be intermediate input
                    if (fullText == "-")
                    {
                        e.Handled = false;
                        OnValidationError?.Invoke(string.Empty);
                    }
                    else
                    {
                        OnValidationError?.Invoke("Invalid input.");
                        e.Handled = true;
                    }
                }
            }
        }

        private static void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (e.DataObject.GetDataPresent(typeof(string)))
                {
                    string pasteText = (string)e.DataObject.GetData(typeof(string));
                    string fullText = GetFullTextAfterPaste(textBox, pasteText);

                    if (!_regex.IsMatch(fullText))
                    {
                        OnValidationError?.Invoke("Only integer values are allowed.");
                        e.CancelCommand();
                        return;
                    }

                    if (int.TryParse(fullText, out int value))
                    {
                        int min = GetMinValue(textBox);
                        int max = GetMaxValue(textBox);

                        if (value < min || value > max)
                        {
                            OnValidationError?.Invoke($"Value must be between {min} and {max}.");
                            e.CancelCommand();
                        }
                        else
                        {
                            OnValidationError?.Invoke(string.Empty);
                        }
                    }
                    else
                    {
                        // If parsing fails, cancel paste
                        OnValidationError?.Invoke("Invalid input.");
                        e.CancelCommand();
                    }
                }
                else
                {
                    OnValidationError?.Invoke("Pasting is not allowed.");
                    e.CancelCommand();
                }
            }
        }

        // Helper method to get the full text after input
        private static string GetFullTextAfterInput(TextBox textBox, string input)
        {
            return textBox.Text.Insert(textBox.CaretIndex, input);
        }

        // Helper method to get the full text after paste
        private static string GetFullTextAfterPaste(TextBox textBox, string pasteText)
        {
            return textBox.Text.Insert(textBox.SelectionStart, pasteText);
        }
    }
}