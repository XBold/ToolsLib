namespace Tools.ObjectHandlers
{
    public static class Properties
    {
        public static void RestoreColor(object sender, EventArgs e)
        {
#if WPF
        if(sender is TextBox textBox)
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
