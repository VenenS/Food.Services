using Food.Data;
using Food.Data.Entities;
using Food.Services.Config;
using Food.Services.Contracts;
using Food.Services.ExceptionHandling;
using Food.Services.Extensions;
using ITWebNet.Food.Core.DataContracts.Admin;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Food.Services.Controllers
{
    [Route("api/users")]
    public class UsersController : ContextableApiController
    {
        public UsersController(IFoodContext context, Accessor accessor)
        {
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [ActivatorUtilitiesConstructor]
        public UsersController()
        {}

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("")]
        public IActionResult GetUsers()
        {
            var users = Accessor.Instance.GetFullListOfUsers().Select(u => u.ToDto()).ToList();
            return Ok(users);
        }

        [HttpGet, Route("adminUsers")]
        public IActionResult GetAdminUsers()
        {
            var users = Accessor.Instance.GetFullListOfUsers().Select(u => u.ToAdminDto()).ToList();
            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("without/curators")]
        public IActionResult GetUsersWithoutCurators()
        {
            var users = Accessor.Instance.GetUsersWithoutCurators().Select(u => u.ToDto()).ToList();
            return Ok(users);
        }

        /*
       [Authorize(Roles = "Admin")]
       [HttpGet]
       [ResponseType(typeof(UserAdminModel))]
       [Route("{id:long}")]
       public async Task<IHttpActionResult> GetUser(int id)
       {
           Models.User user = await UserManager.FindByIdAsync(id);
           if (user == null)
           {
               return NotFound();
           }

           return Ok(user.ToDTO());
       }

       [Authorize(Roles = "Admin")]
       [HttpPost]
       [ResponseType(typeof(void))]
       [Route("{id:long}")]
       public async Task<IHttpActionResult> Update(int id, UserAdminModel model)
       {
           if (!ModelState.IsValid)
           {
               return BadRequest(ModelState);
           }

           if (id != model.Id)
           {
               return BadRequest();
           }

           var user = await UserManager.FindByIdAsync(id);

           user = user.FromDTO(model);
           user.LastUpdateDate = DateTime.UtcNow;

           if (User.Identity.IsAuthenticated)
           {
               //var creator = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
               user.LastUpdateBy = User.Identity.GetUserId<int>();
           }

           var result = await UserManager.UpdateAsync(user);

           IHttpActionResult errorResult = GetErrorResult(result);
           return errorResult ?? StatusCode(HttpStatusCode.NoContent);
       }

       [Authorize(Roles = "Admin")]
       [HttpPost]
       [ResponseType(typeof(UserAdminModel))]
       public async Task<IHttpActionResult> Create(UserAdminModel model, string returnUrl)
       {
           if (!ModelState.IsValid)
           {
               return BadRequest(ModelState);
           }

           var user = model.FromDTO();

           IdentityResult result = await CreateUserAsync(user.ToModelsUser());
           if (result.Succeeded && !string.IsNullOrWhiteSpace(returnUrl))
           {
               await user.SendAuthorizationLinkAsync(UserManager, Url, returnUrl);
           }

           IHttpActionResult errorResult = GetErrorResult(result);
           return errorResult ?? Ok(user);
       }

       [Authorize(Roles = "Admin")]
       [HttpPost]
       public async Task<IHttpActionResult> CreateMultiple(List<UserAdminModel> model, string returnUrl)
       {
           List<UserAdminModel> notValidUsers = new List<UserAdminModel>();

           for (int i = 0; i < model.Count; i++)
           {
               var user = model[i].FromDTO();
               IdentityResult result = await CreateUserAsync(user.ToModelsUser());
               if (result.Succeeded && !string.IsNullOrWhiteSpace(returnUrl))
               {
                   await user.SendAuthorizationLinkAsync(UserManager, Url, returnUrl);
               }
               if (result.Succeeded) continue;
               foreach (var error in result.Errors)
               {
                   ModelState.AddModelError("[" + i + "]", error);
               }
           }

           if (!ModelState.IsValid)
               return BadRequest(ModelState);

           return Ok();
       }

       [Authorize(Roles = "Admin")]
       [HttpPost]
       [ResponseType(typeof(void))]
       public async Task<IHttpActionResult> Delete(int id)
       {
           Models.User user = await UserManager.FindByIdAsync(id);
           if (user == null)
           {
               return NotFound();
           }

           var result = await UserManager.UpdateAsync(user);

           IHttpActionResult errorResult = GetErrorResult(result);
           if (errorResult != null)
               return errorResult;

           return Ok();
       }

       [Authorize(Roles = "Admin")]
       private async Task<IdentityResult> CreateUserAsync(Models.User user)
       {
           user.CreateDate = DateTime.UtcNow;

           if (User.Identity.IsAuthenticated)
           {
               //var creator = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
               user.CreatedBy = User.Identity.GetUserId<int>();
           }

           var result = await UserManager.CreateAsync(user);
           if (result.Succeeded)
           {
               if (!await RoleManager.RoleExistsAsync(EnumUserRole.User))
                   result = await RoleManager.CreateAsync(new Models.Role()
                   {
                       Name = "User",
                       Description = "Пoльзователь"
                   });

               if (result.Succeeded)
                   result = await UserManager.AddToRoleAsync(user.Id, EnumUserRole.User);
           }

           return result;
       }
       */
        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("adduserreferrallink/{parentId:long}/{refferalId:long}")]
        public IActionResult AddUserReferralLink(long parentId, long referralId)
        {
            return Ok(Accessor.Instance.AddUserReferralLink(parentId, referralId));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("addusertorole/{roleId:long}/{userId:long}")]
        public IActionResult AddUserToRole(long roleId, long userId)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (currentUser == null) return Ok(false);
                var userInRole = new UserInRole()
                {
                    RoleId = roleId,
                    UserId = userId
                };

                return Ok(UserServiceHelper.IsNewUserRoleLinkAvailable(userInRole) && Accessor.Instance.AddUserToRole(userInRole));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("edituser")]
        public IActionResult EditUser([FromBody]UserWithLoginModel user)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (currentUser != null)
                {
                    var userM = user.GetEntity();
                    userM.LastUpdateByUserId = currentUser.Id;
                    userM.LastUpdDate = DateTime.Now;

                    return Ok(Accessor.Instance.EditUser(userM));
                }

                return Ok(false);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("checkUniqueEmail")]
        public IActionResult CheckUniqueEmail(string email, long userId = -1)
        {
            return Ok(Accessor.Instance.CheckUniqueEmail(email, userId));
        }

        [HttpGet]
        [Route("edituserpointsbylogin")]
        public IActionResult EditUserPointsByLogin(string login, long typePoints, double points)
        {
            return Ok(Accessor.Instance.EditUserPointsByLogin(login, typePoints, points));
        }

        [HttpGet]
        [Route("edituserpointsbyloginandprice")]
        public IActionResult EditUserPointsByLoginAndTotalPrice(string login, double totalPrice)
        {
            return Ok(Accessor.Instance.EditUserPointsByLoginAndTotalPrice(login, totalPrice));
        }

        [HttpPost]
        [Route("edituserrole")]
        public IActionResult EditUserRole([FromBody]UserRoleModel userRoleLink)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (currentUser != null)
                {
                    var userInRole = userRoleLink.GetEntity();

                    return Ok(UserServiceHelper.IsNewUserRoleLinkAvailable(userInRole) && Accessor.Instance.EditUserRole(userInRole));
                }

                return Ok(false);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("fulllistofusers")]
        public IActionResult GetFullListOfUsers()
        {
            try
            {
                var listOfUsers = new List<UserWithLoginModel>();
                var userFromBase =
                    Accessor.Instance.GetFullListOfUsers();

                foreach (var itemListOfUsers in userFromBase)
                {
                    listOfUsers.Add(itemListOfUsers.GetContractUserLogin());
                }

                return Ok(listOfUsers);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("listroletouser/{userId:long}")]
        public IActionResult GetListRoleToUser(long userId)
        {
            try
            {
                var listFromBase = Accessor.Instance.GetListRoleToUser(userId);

                return Ok(listFromBase.Select(item => item.GetContract()).ToList());
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("listroleuser")]
        public IActionResult GetListUserRole()
        {
            try
            {
                var listOfUserRole = new List<UserRoleModel>();

                var itemFromBase = Accessor.Instance.GetListUserRole();

                foreach (var item in itemFromBase)
                {
                    listOfUserRole.Add(item.GetContract());
                }

                return Ok(listOfUserRole);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("roles")]
        public IActionResult GetRoles()
        {
            try
            {
                var listOfRole = new List<RoleModel>();

                var itemFromBase = Accessor.Instance.GetRoles();

                foreach (var item in itemFromBase)
                {
                    listOfRole.Add(item.GetContract());
                }

                return Ok(listOfRole);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("userbylogin")]
        public IActionResult GetUserByLogin(string login)
        {
            return Ok(Accessor.Instance.GetUserByLogin(login)?.GetContract());
        }

        [HttpGet]
        [Route("userbyemail")]
        public IActionResult GetUserByEmail(string email)
        {
            var user = Accessor.Instance.GetUserByEmail(email) ??
                       Accessor.Instance.GetUserByLogin(email);

            return Ok(user?.GetContract());
        }

        [HttpGet]
        [Route("userbyreflink")]
        public IActionResult GetUserByReferralLink(string referralLink)
        {
            var user = Accessor.Instance.GetUserByReferralLink(referralLink);
            return Ok(user?.GetContract());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("userbyroleid/{roleId:long}")]
        public IActionResult GetUserByRoleId(long roleId)
        {
            try
            {
                var listOfUsers = new List<UserWithLoginModel>();

                var userFromBase = Accessor.Instance.GetUserByRoleId(roleId);

                foreach (var itemListOfUsers in userFromBase)
                    listOfUsers.Add(itemListOfUsers.GetContractUserLogin());

                return Ok(listOfUsers);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }
        [HttpGet]
        [Route("userpointsbylogin")]
        public IActionResult GetUserPointsByLogin(string login)
        {
            return Ok(Accessor.Instance.GetUserPointsByLogin(login));
        }

        [HttpPost]
        [Route("getuserreferrals")]
        public IActionResult GetUserReferrals(long userId, int[] level)
        {
            var referrals = new List<UserReferralModel>();

            foreach (var referral in Accessor.Instance.GetUserReferrals(userId, level))
                referrals.Add(referral.GetContract());

            return Ok(referrals);
        }

        //[Authorize(Roles = "Admin, Consolidator")]
        [HttpGet]
        [Route("usersbycafeorcompany")]
        public IActionResult GetUsersByCafeOrCompany(long id, long longOrganizationType)
        {
            try
            {
                var organizationType = (OrganizationTypeEnum)longOrganizationType;
                var listOfUsers = new List<UserWithLoginModel>();
                List<User> userFromBase;

                if (organizationType == OrganizationTypeEnum.Cafe)
                {
                    userFromBase = Accessor.Instance.GetUsersByCafe(id);
                }
                else if (organizationType == OrganizationTypeEnum.Company)
                {
                    userFromBase = Accessor.Instance.GetListOfUserByCompanyId(id);
                }
                else
                {
                    throw new Exception("Unknown type in enum");
                }

                foreach (var itemListOfUsers in userFromBase)
                {
                    listOfUsers.Add(itemListOfUsers.GetContractUserLogin());
                }

                return Ok(listOfUsers);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("deleteuserrole")]
        public IActionResult RemoveUserRole([FromBody]UserRoleModel model)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (currentUser != null)
                {
                    var userRoleLink = model.GetEntity();

                    return Ok(Accessor.Instance.RemoveUserRole(userRoleLink));
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("feedback")]
        public async Task SendFeedbackAsync([FromBody] FeedbackModel feedback, [FromServices] IConfigureSettings configureSettings)
        {
            if (feedback != null)
            {
                NotificationBase notification =
                    new EmailNotification(configureSettings);

                NotificationBodyBase notificationBody =
                    new FeedbackFromSiteNotificationBody(
                        feedback,
                        configureSettings
                    );

                notification.FormNotification(notificationBody);

                await notification.SendNotificationAsync();
            }
        }

        /// <summary>
        /// Включает/отключает СМС-оповещения для пользователя
        /// </summary>
        /// <param name="UserLogin">Логин пользователя, для которого надо включить-отключить оповещения</param>
        /// <param name="EnableSms">true - включаем оповещения, false - отключаем</param>
        /// <returns>Возвращает Ok, если всё сохранилось успешно. Возвращает NotFound, если пользователь не найден. Возвращает InternalServerError в случае ошибки.</returns>
        [HttpGet]
        [Route("SetUserSmsNotifications")]
        public async Task<IActionResult> SetUserSmsNotifications(string UserLogin, bool EnableSms)
        {
            try
            {
                if (await Accessor.Instance.UserSetSmsNotifications(UserLogin, EnableSms))
                    return Ok();
                else
                    return NotFound();
            }
            catch //(Exception ex)
            {
                return new InternalServerError();
            }
        }
    }
}
