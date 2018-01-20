using System;
using System.Net.Http;
using System.Threading.Tasks;
using Json;
using Universal_Launcher.Singleton;

namespace Universal_Launcher.Models
{
    public class AuthorizationChecker
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly IGetPassword _iPass;
        private readonly string _login;

        /// <summary>
        /// </summary>
        /// <param name="login">Имя пользователя</param>
        /// <param name="iPass">Парольный интерфейс</param>
        public AuthorizationChecker(string login, IGetPassword iPass)
        {
            _login = login;
            _iPass = iPass;
        }

        /// <summary>
        ///     Уникальный ID пользователя по имени
        /// </summary>
        /// <returns></returns>
        public async Task<Guid> GetID()
        {
            var loginString = "https://authserver.ely.by/api/users/profiles/minecraft/" + _login;
            var response = await _client.GetAsync(loginString);
            var jsonString = await response.Content.ReadAsStringAsync();
            try
            {
                var dict = JsonParser.FromJson(jsonString);
                Guid guid;
                Guid.TryParse(dict["id"].ToString(), out guid);
                return guid;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        ///     Своебразный пароль для регистрации
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<string> GetAccessToken(Guid guid)
        {
            if (guid == Guid.Empty)
                return null;

            var message = $"username={_login}&password={_iPass.GetPassword()}&clientToken={guid}";
            var content = new StringContent(message);
            var response = await _client.PostAsync("https://authserver.ely.by/auth/authenticate", content);
            var jsonString = await response.Content.ReadAsStringAsync();
            var json = JsonParser.FromJson(jsonString);
            return json.ContainsKey("accessToken")
                ? json["accessToken"].ToString()
                : null;
        }
    }
}