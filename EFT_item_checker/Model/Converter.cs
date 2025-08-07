using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace EFT_item_checker.Model
{
    public class TextLengthToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int length = (int)value;
            return length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyStringToZeroConverter : IValueConverter
    {
        // ViewModel(int) -> View(TextBox.Text)로 값을 변환할 때 호출
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        // View(TextBox.Text) -> ViewModel(int)로 값을 변환할 때 호출
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;

            if (string.IsNullOrEmpty(strValue))
            {
                return 0;
            }

            if (int.TryParse(strValue, out int result))
            {
                return result;
            }

            // 변환에 실패시 ("abc", "-" 등),
            // 바인딩을 업데이트하지 않도록 하여 기존 값을 유지합니다.
            return Binding.DoNothing;
        }
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue == false;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue == false;
            }
            return value;
        }
    }
}
