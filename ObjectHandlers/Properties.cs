#if WPF
using System.Windows;
using System.Windows.Controls;
#endif
namespace Tools.ObjectHandlers
{
    public static class Properties
    {
#if WPF
        //NEED TO BE TESTED!
        public static DependencyProperty BorderBrushProperty { get; private set; }
#endif

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
