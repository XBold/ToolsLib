namespace Tools.Classes
{
    public class InputFilters
    {
        /// <summary>
        /// Filter spaces from a string inside an "Entry" (MAUI) or a TextBox (WINDOWS, WPF)
        /// </summary>
        /// <param name="sender">Who generated the event</param>
        /// <param name="e">Data from the event</param>
        public static void Spaces(object sender, EventArgs e)
        {
            if (sender == null) return;
#if WINDOWS
            if (sender is System.Windows.Controls.TextBox textBox)
            {
                string filteredText = RemoveSpaces(textBox.Text);
                if (textBox.Text != filteredText)
                {
                    textBox.TextChanged -= Spaces;
                    int cursorPosition = textBox.SelectionStart;
                    textBox.Text = filteredText;
                    textBox.SelectionStart = Math.Min(cursorPosition, filteredText.Length); // Mantieni il cursore
                    textBox.TextChanged += Spaces;
                }
            }
#else
            if (sender is Microsoft.Maui.Controls.Entry entry)
            {
                string filteredText = RemoveSpaces(entry.Text);
                if (entry.Text != filteredText)
                {
                    entry.TextChanged -= Spaces;
                    entry.Text = filteredText;
                    entry.TextChanged += Spaces;
                }
            }
#endif
        }

        /// <summary>
        /// Method used to remove all spaces from a string
        /// </summary>
        /// <param name="input">La stringa da filtrare.</param>
        /// <returns>La stringa filtrata.</returns>
        private static string RemoveSpaces(string input)
        {
            return string.IsNullOrEmpty(input)
                ? string.Empty
                : new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }
    }
}
