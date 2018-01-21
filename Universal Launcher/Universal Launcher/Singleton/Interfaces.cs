using System;
using System.Threading.Tasks;
using Universal_Launcher.ViewModels;

namespace Universal_Launcher.Singleton
{
    /// <summary>
    ///     Сервис показа сообщения в Popup
    /// </summary>
    public interface IShowMessage
    {
        /// <summary>
        ///     Показываем окошко с кнопками
        /// </summary>
        /// <param name="content">Контент</param>
        /// <param name="canBeClosedByUser">Может ли пользователь закрыть окно</param>
        /// <returns></returns>
        Task<bool> ShowMessageAsync(object content, bool canBeClosedByUser = true, bool isError = false);

        /// <summary>
        ///     Показываем окошко, которое исчезнет после выполнения action
        /// </summary>
        /// <param name="content">Контент</param>
        /// <param name="action">Действие, выполняемое при показе окошка</param>
        /// <returns></returns>
        Task<bool> ShowWorkerAsync(object content, Action action);

        ///// <summary>
        /////     Показ сообщения, блокирующий поток
        ///// </summary>
        ///// <param name="content">Вид-модель контента</param>
        ///// <param name="canBeClosedByUser">Может ли юзер закрыть popup</param>
        //void Show(object content, bool canBeClosedByUser = true);

        ///// <summary>
        ///// Показ сообщения
        ///// </summary>
        ///// <param name="content">Вид-модель контента</param>
        ///// <param name="canBeClosedByUser">Может ли юзер закрыть popup</param>
        //Task<bool> ShowAsync(object content, bool canBeClosedByUser = true);

        ///// <summary>
        ///// Показ сообщения, которое закроется только после выполнения действия
        ///// </summary>
        ///// <param name="content">Вид-модель контента</param>
        ///// <param name="action">Действие</param>
        //Task<bool> ShowWorkerAsync(object content,Action action);
    }

    /// <summary>
    ///     Сервис для получения корневой папки лаунчера
    /// </summary>
    public interface IGetFolderService
    {
        /// <summary>
        ///     Корневая папка лаунчера
        /// </summary>
        string RootFolder { get; }

        /// <summary>
        ///     Путь к настройкам
        /// </summary>
        string SettingsPath { get; }

        /// <summary>
        ///     Корневая папка изменена
        /// </summary>
        event EventHandler SwitchedFolder;
    }

    /// <summary>
    ///     Сервис для работы с корневой папкой лаунчера
    /// </summary>
    public interface IFolderService : IGetFolderService
    {
        /// <summary>
        ///     Перемещаем папку лаунчера
        /// </summary>
        /// <param name="newPath"></param>
        void ChangeFolder(string newPath);
    }

    /// <summary>
    ///     Сервис для работы с корневым путем лаунчера и выбранным сервером
    /// </summary>
    public interface INameService : IFolderService
    {
        /// <summary>
        ///     Название проекта
        /// </summary>
        string ProjectName { get; }

        /// <summary>
        ///     Возвращает сервер, выбранный в данный момент
        /// </summary>
        ServerViewModel CurrentServer { get; }

        /// <summary>
        ///     Выбранный сервер изменился
        /// </summary>
        event EventHandler ServerChanged;
    }

    /// <summary>
    ///     Сервис возвращающий ТОЛЬКО пароль
    /// </summary>
    public interface IGetPassword
    {
        /// <summary>
        ///     Возвращает дешифрованный пароль
        /// </summary>
        /// <returns></returns>
        string GetPassword();
    }

    /// <summary>
    ///     Сервис для работы с PasswordBox
    /// </summary>
    public interface IPasswordService : IGetPassword
    {
        /// <summary>
        ///     Проверка на незначащий пароль
        /// </summary>
        bool IsNullOrEmpty { get; }

        /// <summary>
        ///     Устанавливает новый пароль
        /// </summary>
        /// <param name="newPassword">Новый пароль</param>
        /// <returns></returns>
        void SetPassword(string newPassword);

        event EventHandler<bool> CheckPassword;
    }
}