namespace Tools.ObjectHandlers
{
    public static class Properties
    {
        public static void RestoreColor(object sender, TextChangedEventArgs e)
        {
#if WINDOWS
        if(senser is TextBox textBox)
        {
            textBox.ClearValue(BorderBrushProperty);
        }
#else
        if (sender is Entry entry)
        {
            entry.BackgroundColor = Colors.Transparent;
        }
#endif
        }
    }
}
