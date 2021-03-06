﻿using System;
using System.Threading.Tasks;
using Core.Configs;
using Core.DataBase;
using Core.DataBaseEntities;
using Core.Enums.DataBaseEnums;
using Core.Enums.RequestStatuses;
using Core.Mail;
using Core.Mail.Models;
using Core.Users.Registration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core.Users
{
    public interface IUserManager
    {
        Task<RegistrationStatus> CreateEmailConfirmRequest(EmailRegisterInfo email);
        Task<bool> CreatePasswordRestoreRequest(string email);
        Task<bool> ChangePassword(User user, string newPassword);
        Task<RegistrationStatus> RegisterByVk(string name, string vkId);
    }

    public class UserManager : IUserManager
    {
        private readonly ILogger<UserManager> logger;
        private readonly IEmailManager emailManager;
        private readonly IAuthenticationAccountFactory authenticationAccountFactory;
        private readonly IInviteEmailFactory inviteEmailFactory;
        private readonly IAsyncRepository<Invite> invitesRepo;
        private readonly IAsyncRepository<User> usersRepo;
        private readonly IAsyncRepository<AuthenticationAccount> authenticationAccountRepo;
        private readonly IOptions<ConfigOptions> options;

        public UserManager(
            ILogger<UserManager> logger,
            IEmailManager emailManager,
            IAuthenticationAccountFactory authenticationAccountFactory,
            IInviteEmailFactory inviteEmailFactory,
            IAsyncRepository<Invite> invitesRepo,
            IAsyncRepository<User> usersRepo,
            IAsyncRepository<AuthenticationAccount> authenticationAccountRepo,
            IOptions<ConfigOptions> options)
        {
            this.logger = logger;
            this.emailManager = emailManager;
            this.authenticationAccountFactory = authenticationAccountFactory;
            this.inviteEmailFactory = inviteEmailFactory;
            this.invitesRepo = invitesRepo;
            this.usersRepo = usersRepo;
            this.authenticationAccountRepo = authenticationAccountRepo;
            this.options = options;
        }

        public async Task<RegistrationStatus> CreateEmailConfirmRequest(EmailRegisterInfo emailInfo)
        {
            if (await IsServiceIdAlreadyUsed(emailInfo.Email))
                return RegistrationStatus.EmailAlreadyUsed;

            var user = Create(GetName(emailInfo), UserRole.Participant);
            await usersRepo.AddAsync(user);

            var account = authenticationAccountFactory.CreatePasswordAuthenticationAccount(
                user,
                emailInfo.Email,
                emailInfo.Password);
            await authenticationAccountRepo.AddAsync(account);

            var request = inviteEmailFactory.CreateInvite(account, ConfirmationType.Registration);
            if (!TrySendEmail(request))
                return RegistrationStatus.Error;

            await invitesRepo.AddAsync(request);

            return RegistrationStatus.RequestCreated;
        }

        public async Task<bool> CreatePasswordRestoreRequest(string email)
        {
            var account = await authenticationAccountRepo.FirstOrDefaultAsync(a => a.ServiceId == email);
            if (account == null)
                return false;

            var request = inviteEmailFactory.CreateRestorePassword(account, ConfirmationType.Registration);
            if (!TrySendEmail(request))
                return false;

            await invitesRepo.AddAsync(request);

            return true;
        }

        public async Task<bool> ChangePassword(User user, string newPassword)
        {
            var account = await authenticationAccountRepo.FirstOrDefaultAsync(a => a.UserId == user.Id);
            if (account == null)
                return false;

            account = authenticationAccountFactory.ChangePassword(account, newPassword);
            await authenticationAccountRepo.UpdateAsync(account);

            return true;
        }

        public async Task<RegistrationStatus> RegisterByVk(string name, string vkId)
        {
            if (await IsServiceIdAlreadyUsed(vkId))
                return RegistrationStatus.VkIdAlreadyUsed;

            var user = await usersRepo.AddAsync(Create(name, UserRole.Participant));

            var account = authenticationAccountFactory.CreateVkAuthenticationAccount(user, vkId);
            await authenticationAccountRepo.AddAsync(account);

            return RegistrationStatus.Success;
        }

        private async Task<bool> IsServiceIdAlreadyUsed(string serviceId)
            => await authenticationAccountRepo.AnyAsync(r => r.ServiceId == serviceId);

        private bool TrySendEmail(Invite request)
        {
            var mail = request.PasswordRestore
                ? (EmailBase) new PasswordRestoreEmail(request.Email, request.ConfirmationCode, options)
                : new RegistrationConfirmEmail(request.Email, request.ConfirmationCode, options);

            try
            {
                emailManager.Send(mail);
                return true;
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    $"Не удалось отправить письмо {(request.PasswordRestore ? "восстановления пароля" : "подтверждения регистрации")} на адрес {request.Email}");
                return false;
            }
        }

        private static User Create(string userName, UserRole role)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = userName,
                Role = role,
            };
        }

        private static string GetName(EmailRegisterInfo emailInfo)
        {
            var name = $"{emailInfo.Surname} {emailInfo.Name}";
            if (!string.IsNullOrEmpty(emailInfo.Patronymic))
                name += $" {emailInfo.Patronymic}";
            return name;
        }
    }
}
