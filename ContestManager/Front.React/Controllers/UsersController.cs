using System;
using System.Threading.Tasks;
using Core.DataBase;
using Core.DataBaseEntities;
using Core.Users;
using Core.Users.Login;
using Core.Users.Registration;
using Core.Users.Sessions;
using Microsoft.AspNetCore.Mvc;

namespace Front.React.Controllers
{
    public class UsersController : ControllerBase
    {
        private readonly IUserCookieManager userCookieManager;
        private readonly IAuthenticationManager authenticationManager;
        private readonly IUserManager userManager;
        private readonly IAsyncRepository<User> usersRepo;

        public UsersController(
            IUserCookieManager userCookieManager,
            IAuthenticationManager authenticationManager,
            IUserManager userManager,
            IAsyncRepository<User> usersRepo)
        {
            this.userCookieManager = userCookieManager;
            this.authenticationManager = authenticationManager;
            this.userManager = userManager;
            this.usersRepo = usersRepo;
        }

        [HttpPost("login/email")]
        public async Task<ActionResult> Login([FromBody] EmailLoginInfo emailLoginInfo)
        {
            try
            {
                var user = await authenticationManager.Authenticate(emailLoginInfo);
                userCookieManager.SetLoginCookie(Response, user);

                return Json(user);
            }
            catch (AuthenticationFailedException)
            {
                return Unauthorized();
            }
        }

        [HttpPost("login/vk")]
        public async Task<ActionResult> Login([FromBody] VkLoginInfo vkLoginInfo)
        {
            try
            {
                var user = await authenticationManager.Authenticate(vkLoginInfo);
                userCookieManager.SetLoginCookie(Response, user);

                var actionResult = Json(user);
                return actionResult;
            }
            catch (AuthenticationFailedException)
            {
                return Unauthorized();
            }
        }

        [HttpPost("register/email")]
        public async Task<JsonResult> CreateEmailRegistrationRequest([FromBody] EmailRegisterInfo emailInfo)
        {
            var emailRegistrationRequest = await userManager.CreateEmailConfirmRequest(emailInfo);

            return Json(emailRegistrationRequest);
        }

        [HttpPost("register/vk")]
        public async Task<JsonResult> RegisterByVk([FromBody] VKRegisterInfo vkRegisterInfo)
        {
            var registerByVk = await userManager.RegisterByVk(vkRegisterInfo.Name, vkRegisterInfo.VkId);

            return Json(registerByVk);
        }

        [HttpGet("check")]
        public async Task<ActionResult> Check()
        {
            try
            {
                var user = await userCookieManager.GetUser(Request);

                return Json(user);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403);
            }
        }

        [HttpPost("restore")]
        public async Task<ActionResult> RestorePassword([FromBody] string email)
        {
            var registrationRequestStatus = await userManager.CreatePasswordRestoreRequest(email);

            return Json(registrationRequestStatus);
        }

        [HttpPatch]
        public async Task<ActionResult> Patch([FromBody] User user)
        {
            try
            {
                var userFromDb = await userCookieManager.GetUser(Request);

                if (!RoleChangeValidator.Validate(userFromDb.Role, user.Role))
                    return StatusCode(
                        403,
                        new { error = "Нельзя менять роль", from = userFromDb.Role, to = user.Role });

                userFromDb.Class = user.Class;
                userFromDb.School = user.School;
                userFromDb.Name = user.Name;
                userFromDb.Role = user.Role;
                userFromDb.City = user.City;
                userFromDb.Coach = user.Coach;
                await usersRepo.UpdateAsync(userFromDb);
                return Json(user);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403);
            }
        }

        [HttpPost("changePass")]
        public async Task<ActionResult> ChangePass(string password)
        {
            var user = await userCookieManager.GetUser(Request);

            var isChanged = await userManager.ChangePassword(user, password);
            if (isChanged)
                return Redirect("/");

            return StatusCode(400);
        }


        [HttpPost("logout")]
        public void LogOut() => userCookieManager.Clear(Response);
    }
}