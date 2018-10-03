using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Launcher.Properties;
using Newtonsoft.Json.Linq;

namespace Launcher.AuthModules
{
    class ElyAuthModule : AuthModulBase
    {
        private readonly HttpClient _client;

        public ElyAuthModule(string login)
            : base(login)
        {
            _client = new HttpClient();
        }

        #region Override

        public override async Task<AuthResult> TryAuthentificate()
        {
            if (string.IsNullOrWhiteSpace(_login) || string.IsNullOrWhiteSpace(_iPass.GetPassword()))
                return new AuthResult();

            var guid = await GetID();
            if (guid == Guid.Empty)
                return new AuthResult();

            var token = await GetAccessToken(guid);
            var result = new AuthResult(string.IsNullOrEmpty(token), token);

            if (result.IsSuccessfull())
            {
                WriteAccessToken(token);
            }

            return result;
        }

        #endregion

        #region Private

        /// <summary>
        ///     Уникальный ID пользователя по имени
        /// </summary>
        /// <returns></returns>
        private async Task<Guid> GetID()
        {
            var loginQuerry = Resources.ElyAuthorizationLink + _login;
            try
            {
                var responseRaw = await _client.GetAsync(loginQuerry);
                var json = JObject.Parse(await responseRaw.Content.ReadAsStringAsync());
                if (json.ContainsKey("id"))
                {
                    if (Guid.TryParse(json["id"].ToString(), out var guid))
                        return guid;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }

            return Guid.Empty;
        }

        /// <summary>
        ///     Своебразный пароль для регистрации
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private async Task<string> GetAccessToken(Guid guid)
        {
            if (guid == Guid.Empty)
                return null;

            var message = $"username={_login}&password={_iPass.GetPassword()}&clientToken={guid}";
            var content = new StringContent(message);
            var response = await _client.PostAsync(Resources.ElySignInLink, content);
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            return json["accessToken"]?.ToString();
        }

        #endregion
    }

    
}
