using System.Windows.Controls;
using System.Windows.Media;

namespace TridionDesktopTools.Core
{
    public static class UIHelpers
    {

        public static void SetEnabled(this Button buton)
        {
            buton.IsEnabled = true;
            buton.Foreground = new SolidColorBrush(Colors.Black);
        }

        public static void SetEnabledGreen(this Button buton)
        {
            buton.IsEnabled = true;
            buton.Foreground = new SolidColorBrush(Colors.Green);
        }

        public static void SetEnabledRed(this Button buton)
        {
            buton.IsEnabled = true;
            buton.Foreground = new SolidColorBrush(Colors.Red);
        }

        public static void SetDisabled(this Button buton)
        {
            buton.IsEnabled = false;
            buton.Foreground = new SolidColorBrush(Colors.Gray);
        }
    }
}
