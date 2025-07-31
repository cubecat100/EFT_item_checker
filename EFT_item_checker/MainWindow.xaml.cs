using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EFT_item_checker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel.MainViewModel();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var vm = DataContext as ViewModel.MainViewModel;

            if (vm != null)
            {
                vm.Dispose();
            }
        }

        private bool isPanelOpen = false;

        private void OpenPanel_Click(object sender, RoutedEventArgs e)
        {
            if (!isPanelOpen)
            {
                SlidePanel.Visibility = Visibility.Visible;

                var openAnim = new DoubleAnimation
                {
                    From = 200,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                SlideTransform.BeginAnimation(TranslateTransform.XProperty, openAnim);
                isPanelOpen = true;
            }
            else
            {
                var closeAnim = new DoubleAnimation
                {
                    From = 0,
                    To = 200,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };

                closeAnim.Completed += (s, a) =>
                {
                    SlidePanel.Visibility = Visibility.Collapsed;
                    isPanelOpen = false;
                };

                SlideTransform.BeginAnimation(TranslateTransform.XProperty, closeAnim);
            }
        }
    }
}