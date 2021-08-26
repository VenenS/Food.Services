using Food.Data.Entities;
using Food.Services.Extensions;
using Food.Services.Models;
using Food.Services.Services;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Controllers
{
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        NotificationHelper _helper = new NotificationHelper();
        INotificationService _notificationService;

        /// <summary>
        /// Максимально допустимая длина СМС-кода
        /// </summary>
        private const int SmsCodeLengthMax = 18;
        /// <summary>
        /// Минимально допустимая длина СМС-кода
        /// </summary>
        private const int SmsCodeLengthMin = 2;
        /// <summary>
        /// Максимально допустимое время действия СМС-кода (двое суток)
        /// </summary>
        private const int SmsCodeLifeTimeMax = 60 * 60 * 24 * 2;
        /// <summary>
        /// Минимально допустимое время действия СМС-кода - 10 секунд
        /// </summary>
        private const int SmsCodeLifeTimeMin = 10;

        /// <summary>
        /// Длина СМС-кода
        /// </summary>
        private readonly int SmsCodeLength = 6;
        /// <summary>
        /// Время действия СМС-кода в секундах
        /// </summary>
        private readonly int SmsCodeLifeTime = 120;
        /// <summary>
        /// Тип СМС кода (цифры, буквы или смешанный)
        /// </summary>
        private readonly SMSCodeTypeEnum SmsCodeType = SMSCodeTypeEnum.Numeric;

        public NotificationController(IConfiguration Configuration, INotificationService notificationService)
        {
            // Загрузка настроек СМС-кодов из конфиг-файла
            try
            {
                _notificationService = notificationService;

                int value;
                if (int.TryParse(Configuration.GetSection("SmsCodeLength").Value, out value))
                {
                    if (value >= SmsCodeLengthMin && value <= SmsCodeLengthMax)
                        SmsCodeLength = value;
                }
                if (int.TryParse(Configuration.GetSection("SmsCodeLifeTime").Value, out value))
                {
                    if (value >= SmsCodeLifeTimeMin && value <= SmsCodeLifeTimeMax)
                        SmsCodeLifeTime = value;
                }
                if (int.TryParse(Configuration.GetSection("SmsCodeType").Value, out value))
                {
                    if (value >= ((int)SMSCodeTypeEnum.Numeric) && value <= ((int)SMSCodeTypeEnum.Mixed))
                        SmsCodeType = (SMSCodeTypeEnum)value;
                }
            }
            catch
            { }
        }

        [HttpGet]
        [Route("{id:long}")]
        public IActionResult GetNotificationForUser(long id)
        {
            return Ok(Accessor.Instance.GetCafeIdWithNewOrders(id));
        }

        [HttpPost]
        [Route("stop/{id:long}")]
        public IActionResult StopNotifyUser(long id)
        {
            Accessor.Instance.StopNotifyUser(id);
            return Ok();
        }

        [HttpPost]
        [Route("viewed/{id:long}")]
        public IActionResult SetOrdersViewed(long id)
        {
            Accessor.Instance.SetOrdersOfCafeViewed(id);
            return Ok(true);
        }

        [HttpPost]
        [Route("ordered/{id:long}/deliveryDate/{deliveryDate}")]
        public IActionResult NewOrderForNotification(long id, long deliveryDate)
        {
            var emails = Accessor.Instance.NewOrderForNotification(id, DateTimeExtensions.FromUnixTime(deliveryDate));
            return Ok(emails);
        }

        /// <summary>
        /// Отправить смс код на номер телефона
        /// </summary>
        /// <param name="phone">Телефон</param>
        /// <param name="isConfirming">Это подтверждение телефона или нет</param>
        [HttpGet]
        [Route("SendSmsCode")]
        public async Task<IActionResult> SendSmsCode(string phone, bool isConfirming)
        {
            if (string.IsNullOrEmpty(phone))
                return Ok(new ResponseModel() { Message = "Заполните телефон", Status = 1 });

            phone = phone.Replace("+", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();

            var user = Accessor.Instance.GetUserByPhone(phone);
            if (user == null)
                return Ok(new ResponseModel() { Message = "Пользователь с таким телефоном не зарегистрирован", Status = 1 });

            string code = _helper.GetRandomCode(SmsCodeLength, SmsCodeType);
            SmsCode smsCode = new SmsCode()
            {
                Code = code,
                UserId = user.Id,
                Phone = new string(phone.Where(u => char.IsDigit(u)).ToArray()),
                CreationTime = DateTime.Now,
                ValidTime = DateTime.Now.AddSeconds(SmsCodeLifeTime),
                IsActive = true
            };
            long smsCodeId = await Accessor.Instance.AddSmsCode(smsCode);

            if (smsCodeId < 0)
            {
                return Ok(new ResponseModel() { Message = "Не удалось сгенерировать смс код", Status = 1 });
            }
            else
            {
                ResponseModel response;
                if (user.PhoneNumberConfirmed || isConfirming)
                {
                    response = await _notificationService.SmsSend(phone, $"Ваш код подтверждения {code}");
                    if (response.Status > 0)
                    {
                        response.Message = "Не удалось отправить смс код. " + response.Message;
                    }
                }
                else
                {
                    response = new ResponseModel();
                    response.Status = 1;
                    if (!user.PhoneNumberConfirmed)
                        response.Message = "Данный номер телефона не подтверждён";
                }

                return Ok(response);
            }
        }
    }
}
