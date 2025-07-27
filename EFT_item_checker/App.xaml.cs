using EFT_item_checker.Service;
using System.Configuration;
using System.Data;
using System.Windows;

namespace EFT_item_checker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                TaskManager.Instance.Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"아이템 목록 초기화 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }

}
