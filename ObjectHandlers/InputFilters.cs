using static Tools.GeneralChecks;
namespace Tools.ObjectHandlers
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
#if WPF
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
            if (sender is Entry entry)
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
    }
}
