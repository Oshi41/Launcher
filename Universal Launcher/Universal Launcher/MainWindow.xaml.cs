using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Universal_Launcher.Singleton;
using Universal_Launcher.ViewModels;

namespace Universal_Launcher
{
    /// <summary>
    ///     Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IShowMessage
    {
        // типы контролов, с которыми может взаимодействовать пользователь
        private readonly List<Type> _forbiddenTypes = new List<Type>
        {
            typeof(ButtonBase),
            typeof(TextBoxBase),
            typeof(ComboBox),
            typeof(Slider),
            typeof(Expander),
            typeof(Hyperlink),
            typeof(PasswordBox)
        };

        public MainWindow()
        {
            InitializeComponent();

            IoCContainer.Instanse.RegisterSingleton((IShowMessage)this);
        }

        #region Drag-n-Move

        private void OnDragMove(object sender, MouseButtonEventArgs e)
        {
            if (FindForbiddenParent(e.Device.Target as DependencyObject))
                return;

            if (e.ButtonState == MouseButtonState.Pressed)
            {
                var window = GetWindow(this);
                window.DragMove();
            }
        }

        /// <summary>
        ///     Возможно на запрещенном контроле есть нечто, не входящее в этот список.
        ///     Чтобы контролы в списке были полностью доступны, проверяем их родителей
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        private bool FindForbiddenParent(DependencyObject control)
        {
            if (control == null)
                return false;

            if (_forbiddenTypes.Any(x => x.IsInstanceOfType(control)))
                return true;

            var parent = control is Visual
                ? VisualTreeHelper.GetParent(control)
                : LogicalTreeHelper.GetParent(control);

            return FindForbiddenParent(parent);
        }

        #endregion

        #region IShowMessage

        public async Task<bool> ShowMessageAsync(object content, bool canBeClosedByUser = true, bool isError = false)
        {
            if (!CheckHost())
                return false;

            var data = new MessageViewModel(content.ToString(), isError);
            DialogHost.CloseOnClickAway = canBeClosedByUser;

            await DialogHost.Show(data, "DialogHost");
            return data.Result;
        }

        public async Task<bool> ShowWorkerAsync(object content, Action action)
        {
            if (!CheckHost())
                return false;

            DialogHost.CloseOnClickAway = false;

            DialogHost.Show(content, "DialogHost");

            var result = await Task.Run(() =>
            {
                try
                {
                    action();
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            });

            DialogHost.CloseDialogCommand.Execute(null, null);
            return result;
        }

        private bool CheckHost()
        {
            if (!DialogHost.IsLoaded)
                return false;

            if (DialogHost.IsOpen)
            {
                DialogHost.CloseDialogCommand.Execute(null, null);
                DialogHost.IsOpen = false;
            }

            return true;
        }

        #endregion
    }
}