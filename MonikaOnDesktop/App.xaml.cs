using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace MonikaOnDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppContext.SetSwitch("Switch.System.Windows.Controls.Text.UseAdornerForTextboxSelectionRendering", false);
        }
    }

}
