using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChemGateBuilder
{
    public static class TextBoxExtensions
    {
        // Attached property to enable integer-only input
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

        // Regex to match non-integer characters
        private static readonly Regex _regex = new Regex("[^0-9]+");

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _regex.IsMatch(e.Text);
        }

        private static void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (_regex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}