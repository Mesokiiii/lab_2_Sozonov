using System.Windows;
using System.Windows.Controls;
using WpfLabApp.ViewModels;

namespace WpfLabApp.Views
{
    /// <summary>
    /// Вкладка «2.1. Типы привязки и UpdateSourceTrigger».
    /// </summary>
    public partial class BindingsTabView : UserControl
    {
        public BindingsTabView()
        {
            InitializeComponent();
            DataContext = new BindingsViewModel();
        }

        private void UpdateExplicitBinding_Click(object sender, RoutedEventArgs e)
        {
            ExplicitTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
    }
}
