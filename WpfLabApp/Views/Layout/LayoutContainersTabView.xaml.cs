using System.Windows;
using System.Windows.Controls;

namespace WpfLabApp.Views
{
    public partial class LayoutContainersTabView : UserControl
    {
        public LayoutContainersTabView()
        {
            InitializeComponent();
        }

        private void NavStackPanel_Click(object sender, RoutedEventArgs e) => CardStackPanel?.BringIntoView();
        private void NavGrid_Click(object sender, RoutedEventArgs e) => CardGrid?.BringIntoView();
        private void NavWrapPanel_Click(object sender, RoutedEventArgs e) => CardWrapPanel?.BringIntoView();
        private void NavDockPanel_Click(object sender, RoutedEventArgs e) => CardDockPanel?.BringIntoView();
        private void NavCanvas_Click(object sender, RoutedEventArgs e) => CardCanvas?.BringIntoView();
        private void NavUniformGrid_Click(object sender, RoutedEventArgs e) => CardUniformGrid?.BringIntoView();
    }
}
