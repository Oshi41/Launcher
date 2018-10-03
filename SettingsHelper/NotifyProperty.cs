using System;
using System.Windows;

namespace SettingsHelper
{
    public static class NotifyProperty 
    {
        public static readonly DependencyProperty IsErrorProperty = DependencyProperty.RegisterAttached(
            "IsError", typeof(bool), typeof(NotifyProperty), new PropertyMetadata(default(bool)));

        public static void SetIsError(DependencyObject element, bool value)
        {
            element.SetValue(IsErrorProperty, value);
        }

        public static bool GetIsError(DependencyObject element)
        {
            return (bool) element.GetValue(IsErrorProperty);
        }


        public static readonly DependencyProperty IsSuccessProperty = DependencyProperty.RegisterAttached(
            "IsSuccess", typeof(bool), typeof(NotifyProperty), new PropertyMetadata(default(bool)));

        public static void SetIsSuccess(DependencyObject element, bool value)
        {
            element.SetValue(IsSuccessProperty, value);
        }

        public static bool GetIsSuccess(DependencyObject element)
        {
            return (bool)element.GetValue(IsSuccessProperty);
        }

    }
}
