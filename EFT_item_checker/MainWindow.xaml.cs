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

            SetAnimation();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var vm = DataContext as ViewModel.MainViewModel;

            if (vm != null)
            {
                vm.Dispose();
            }
        }

        private bool isRightPanelOpen = false;
        private bool isLeftPanelOpen = false;

        private DoubleAnimation openRightAnim;
        private DoubleAnimation closeRightAnim;

        private DoubleAnimation openLeftAnim;
        private DoubleAnimation closeLeftAnim;

        private void SetAnimation()
        {
            openRightAnim = new DoubleAnimation
            {
                From = 200,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            closeRightAnim = new DoubleAnimation
            {
                From = 0,
                To = 200,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            openLeftAnim = new DoubleAnimation
            {
                From = -200,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            closeLeftAnim = new DoubleAnimation
            {
                From = 0,
                To = -200,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

        }

        private void OpenRightPanel_Click(object sender, RoutedEventArgs e)
        {
            if (!isRightPanelOpen)
            {
                RightSlidePanel.Visibility = Visibility.Visible;

                RightSlideTransform.BeginAnimation(TranslateTransform.XProperty, openRightAnim);
                isRightPanelOpen = true;
            }
            else
            {
                closeRightAnim.Completed += (s, a) =>
                {
                    RightSlidePanel.Visibility = Visibility.Collapsed;
                    isRightPanelOpen = false;
                };

                RightSlideTransform.BeginAnimation(TranslateTransform.XProperty, closeRightAnim);
            }
        }

        private void OpenLeftPanel_Click(object sender, RoutedEventArgs e)
        {
            if (!isLeftPanelOpen)
            {
                LeftSlidePanel.Visibility = Visibility.Visible;

                LeftSlideTransform.BeginAnimation(TranslateTransform.XProperty, openLeftAnim);
                isLeftPanelOpen = true;
            }
            else
            {
                closeLeftAnim.Completed += (s, a) =>
                {
                    LeftSlidePanel.Visibility = Visibility.Collapsed;
                    isLeftPanelOpen = false;
                };

                LeftSlideTransform.BeginAnimation(TranslateTransform.XProperty, closeLeftAnim);
            }
        }
    }
}