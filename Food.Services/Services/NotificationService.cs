using Food.Services.Config;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Food.Services.Services
{
    public interface ISmsSender
    {
        Task<ResponseModel> SmsSend(string phone, string text, int attempts = 0);
    }

    /// <summary>
    /// Клиент для общения с Сервисом СМС
    /// </summary>
    public class NotificationService : INotificationService
    {
        const string token = "notification_service_token";
        readonly string clientDomain;
        private readonly IConfigureSettings _configureSettings;

        HttpClient HttpClient { get; set; }

        public NotificationService(IConfigureSettings config)
        {
            //clientDomain = ConfigurationManager.AppSettings["ClientDomain"];
            //в sms aero настроен только этот sign
            clientDomain = "edovoz.com";
            _configureSettings = config;
            string uri = _configureSettings.NotificationServiceSms;
            HttpClient = new HttpClient() { BaseAddress = new Uri(uri) };
            var tokenCache = MemoryCache.Default.Get(token);
            if (tokenCache != null)
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenCache.ToString());
        }

        /// <summary>
        /// Отправить смс на номер телефона
        /// </summary>
        /// <param name="phone">Телефон</param>
        /// <param name="text">Текст смс сообщения</param>
        /// <param name="attempts">Количество попыток авторизации</param>
        public async Task<ResponseModel> SmsSend(string phone, string text, int attempts = 0)
        {
            var httpResponse = HttpClient.GetAsync($"/sms/messages/send?phone={phone}&text={text}&sign={clientDomain}").GetAwaiter().GetResult();
            if (httpResponse.IsSuccessStatusCode) {
                return await httpResponse.Content.ReadAsAsync<ResponseModel>();
            } else {
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized) {
                    //Отсутствует/Просрочен токен, запрашиваем новый
                    var responseAuthenticate = await Authenticate();
                    if (responseAuthenticate.Status == 0) {
                        //если количество не удачных попыток авторизации меньше 2, то пробуем еще раз получить токен
                        //иначе выдаем ошибку

                        if (attempts < 2)
                            return await SmsSend(phone, text, attempts + 1);
                    }
                }

                return new ResponseModel() { Status = 1, Message = $"Ошибка при обращении к сервису смс. {httpResponse.StatusCode}" };
            }
        }

        /// <summary>
        /// Запрашиваем токен доступа для сервиса смс
        /// </summary>
        private async Task<ResponseModel> Authenticate()
        {
            string applicationId = _configureSettings.NotificationServiceSmsApplicationId;
            string secretKey = _configureSettings.NotificationServiceSmsSecretKey;

            var httpResponse = HttpClient.GetAsync($"users/authenticate?applicationId={applicationId}&secretKey={secretKey}").GetAwaiter().GetResult();
            if (httpResponse.IsSuccessStatusCode) {
                var responseToken = await httpResponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseToken)) {
                    MemoryCache.Default.Set(token, responseToken, DateTime.Now.AddDays(1));
                    HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responseToken);
                    return new ResponseModel();
                } else {
                    return new ResponseModel() { Status = 1, Message = $"Не удалось получить токен доступа к сервису смс" };
                }
            } else {
                return new ResponseModel() { Status = 1, Message = $"Ошибка при обращении к сервису смс. {httpResponse.StatusCode}" };
            }
        }
    }
}
