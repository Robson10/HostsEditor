using HostsEditorA.Areas.ViewModels.MainWindow;
using System.Windows;

namespace HostsEditor.Areas.MainWindow.Views
{
    /// <summary>
    /// Interaction logic for MainWindowV.xaml
    /// </summary>
    public partial class MainWindowV : Window
    {
        public MainWindowV()
        {
            InitializeComponent();
            DataContext = new MainWindowVM();
        }
    }
}
