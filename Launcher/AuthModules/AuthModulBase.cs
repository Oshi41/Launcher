using System.Threading.Tasks;
using Launcher.Interfaces;
using Launcher.Models;

namespace Launcher.AuthModules
{
    abstract class AuthModulBase
    {
        #region Fields

        protected readonly string _login;
        protected readonly IGetPassword _iPass;

        #endregion

        #region Abstract

        /// <summary>
        /// Пытаемся авторизоваться
        /// </summary>
        /// <returns></returns>
        public abstract Task<AuthResult> TryAuthentificate();

        #endregion

        protected AuthModulBase(string login)
        {
            _login = login;
            _iPass = Singleton.Instance.PasswordService;
        }

        /// <summary>
        /// Записываем полученный токен пользователя
        /// </summary>
        /// <param name="token"></param>
        protected void WriteAccessToken(string token)
        {
            Singleton.Instance.SetAssessToken(token);
        }

        #region Nested Class

        internal class AuthResult
        {
            private readonly bool _isSuccessful;
            private readonly object _result;

            public AuthResult(bool isSuccessful = false, object result = null)
            {
                _isSuccessful = isSuccessful;
                _result = result;
            }

            public bool IsSuccessfull()
            {
                return _isSuccessful;
            }

            public T GetValue<T>() where T : class
            {
                if (_result is T result)
                    return result;

                return null;
            }
        }

        #endregion
    }

    public enum Modules
    {
        Ely,
    }
}
