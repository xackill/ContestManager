using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DataBase;
using Core.DataBaseEntities;
using Core.Enums.DataBaseEnums;
using Core.Users.Sessions;
using Front.React.Models;
using Microsoft.AspNetCore.Mvc;

namespace Front.React.Controllers
{
    public class QualificationController : ControllerBase
    {
        private readonly IUserCookieManager cookieManager;
        private readonly IAsyncRepository<QualificationTask> tasksRepo;
        private readonly IAsyncRepository<Participant> participantRepo;
        private readonly IAsyncRepository<Contest> contestRepo;
        private readonly IAsyncRepository<QualificationParticipation> participationRepo;
        private static readonly TimeSpan RoundTime = TimeSpan.FromHours(3);

        public QualificationController(
            IUserCookieManager cookieManager,
            IAsyncRepository<QualificationTask> tasksRepo,
            IAsyncRepository<Participant> participantRepo,
            IAsyncRepository<Contest> contestRepo,
            IAsyncRepository<QualificationParticipation> participationRepo)
        {
            this.cookieManager = cookieManager;
            this.tasksRepo = tasksRepo;
            this.participantRepo = participantRepo;
            this.contestRepo = contestRepo;
            this.participationRepo = participationRepo;
        }

        [HttpGet]
        public async Task<ActionResult> GetQualificationState(Guid contestId)
        {
            var participant = await GetParticipant(contestId);
            if (participant == null)
                return StatusCode(403, "No participant for contest");

            var participation = await participationRepo.FirstOrDefaultAsync(p => p.ParticipantId == participant.Id);
            if (participation == null)
                return StatusCode(400, "not started");

            var contest = await contestRepo.GetByIdAsync(participant.ContestId);
            var tasks = await GetTasks(participant);
            return Json(
                new QualificationState
                {
                    Title = contest.Title,
                    Answers = participation.Answers,
                    Tasks = tasks.Select(t => t.Text).ToArray(),
                    TimeLeft = (int) (participation.EndTime - DateTimeOffset.UtcNow).TotalSeconds,
                });
        }

        [HttpGet("state")]
        public async Task<ActionResult> IsStarted(Guid contestId)
        {
            var participant = await GetParticipant(contestId);
            if (participant == null)
                return StatusCode(400, "No participant for contest");

            var participation = await participationRepo.FirstOrDefaultAsync(p => p.ParticipantId == participant.Id);
            if (participation == null)
                return Json(QualificationSolveState.NotStarted);

            if (DateTimeOffset.UtcNow + RoundTime > participation.EndTime)
                return Json(QualificationSolveState.InProgress);

            return Json(QualificationSolveState.Finished);
        }

        [HttpPost("start")]
        public async Task<ActionResult> Start(Guid contestId)
        {
            var participant = await GetParticipant(contestId);
            if (participant == null)
                return StatusCode(400, "No participant for contest");

            var tasks = await GetTasks(participant);

            var participation = new QualificationParticipation
            {
                Id = Guid.NewGuid(),
                ContestId = contestId,
                ParticipantId = participant.Id,
                EndTime = DateTimeOffset.UtcNow + RoundTime,
                Answers = tasks.OrderBy(t => t.Number).Select(_ => "").ToArray(),
            };

            var qualificationParticipation = await participationRepo.AddAsync(participation);

            return Json(qualificationParticipation);
        }

        [HttpPost("save")]
        public async Task<ActionResult> Start(Guid contestId, [FromBody] string[] answers)
        {
            var participant = await GetParticipant(contestId);
            if (participant == null)
                return StatusCode(403, "No participant for contest");

            var participation = await participationRepo.FirstOrDefaultAsync(p => p.ParticipantId == participant.Id);
            participation.Answers = answers;
            await participationRepo.UpdateAsync(participation);

            return Ok();
        }

        private async Task<IReadOnlyList<QualificationTask>> GetTasks(Participant participant)
        {
            return await tasksRepo.WhereAsync(
                t => t.ContestId == participant.ContestId && t.Classes.Contains(participant.UserSnapshot.Class.Value));
        }

        private async Task<Participant> GetParticipant(Guid contestId)
        {
            var user = await cookieManager.GetUser(Request);
            if (user.Role != UserRole.Participant)
                return null;

            return await participantRepo.FirstOrDefaultAsync(p => p.UserId == user.Id && p.ContestId == contestId);
        }
    }
}
