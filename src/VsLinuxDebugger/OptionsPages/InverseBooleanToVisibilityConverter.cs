using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  /// <summary>Converts a bool to <see cref="Visibility"/>, inverted: <c>false</c> maps to
  /// <see cref="Visibility.Visible"/> and <c>true</c> maps to <see cref="Visibility.Collapsed"/>.
  /// Used to hide/show whole label+control rows (not just the input) based on another
  /// property's live value, i.e. the password field on the Remote Credentials page.</summary>
  public sealed class InverseBooleanToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      => (value is bool b && b) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      => value is Visibility v && v == Visibility.Visible;
  }
}
