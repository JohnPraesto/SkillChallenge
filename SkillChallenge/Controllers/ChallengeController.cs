using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillChallenge.DTOs.Challenge;
using SkillChallenge.DTOs.SubCategory;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;
using SkillChallenge.Services;
using System.Security.Claims;

namespace SkillChallenge.Controllers
{
    [ApiController]
    [Route("challenges")]
    public class ChallengeController : ControllerBase
    {
        private readonly IChallengeRepository _challengeRepo;
        private readonly EloRatingService _eloRatingService;

        public ChallengeController(IChallengeRepository challengeRepo, EloRatingService eloRatingService)
        {
            _challengeRepo = challengeRepo;
            _eloRatingService = eloRatingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChallenges(CancellationToken ct)
        {
            var challenges = await _challengeRepo.GetAllChallengesAsync(ct);
            var challengeDTOs = challenges
                .Select(c => new ChallengeDTO
                {
                    ChallengeId = c.ChallengeId,
                    ChallengeName = c.ChallengeName,
                    EndDate = c.EndDate,
                    VotePeriodEnd = c.VotePeriodEnd,
                    IsTakenDown = c.IsTakenDown,
                    Description = c.Description,
                    NumberOfParticipants = c.NumberOfParticipants,
                    SubCategory =
                        c.SubCategory == null
                            ? null
                            : new SubCategoryDTO
                            {
                                SubCategoryId = c.SubCategory.SubCategoryId,
                                SubCategoryName = c.SubCategory.SubCategoryName,
                                ImagePath = c.SubCategory.ImagePath,
                            },
                    JoinedUsers = c.Participants.Select(u => u.UserName ?? "Unknown").ToList(),
                    CreatedBy = c.CreatedBy,
                    CreatorUserName = c.Creator?.UserName ?? "Unknown",
                })
                .ToList();

            return Ok(challengeDTOs);
        }

        [HttpGet("my-challenges")]
        [Authorize]
        public async Task<IActionResult> GetMyChallenges(CancellationToken ct)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var challenges = await _challengeRepo.GetChallengesByCreatorAsync(currentUserId, ct);
            var challengeDTOs = challenges
                .Select(c => new ChallengeDTO
                {
                    ChallengeId = c.ChallengeId,
                    ChallengeName = c.ChallengeName,
                    EndDate = c.EndDate,
                    VotePeriodEnd = c.VotePeriodEnd,
                    IsTakenDown = c.IsTakenDown,
                    Description = c.Description,
                    NumberOfParticipants = c.NumberOfParticipants,
                    SubCategory =
                        c.SubCategory == null
                            ? null
                            : new SubCategoryDTO
                            {
                                SubCategoryId = c.SubCategory.SubCategoryId,
                                SubCategoryName = c.SubCategory.SubCategoryName,
                                ImagePath = c.SubCategory.ImagePath,
                            },
                    JoinedUsers = c.Participants.Select(u => u.UserName ?? "Unknown").ToList(),
                    CreatedBy = c.CreatedBy,
                    CreatorUserName = c.Creator?.UserName ?? "Unknown",
                })
                .ToList();

            return Ok(challengeDTOs);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetChallengeById([FromRoute] int id, CancellationToken ct)
        {
            var challenge = await _challengeRepo.GetChallengeByIdAsync(id, ct);
            if (challenge == null)
            {
                return NotFound($"Challenge with id {id} was not found in the database");
            }

            var challengeDTO = new ChallengeDTO
            {
                ChallengeId = challenge.ChallengeId,
                ChallengeName = challenge.ChallengeName,
                EndDate = challenge.EndDate,
                VotePeriodEnd = challenge.VotePeriodEnd,
                IsTakenDown = challenge.IsTakenDown,
                Description = challenge.Description,
                NumberOfParticipants = challenge.NumberOfParticipants,
                SubCategory =
                    challenge.SubCategory == null
                        ? null
                        : new SubCategoryDTO
                        {
                            SubCategoryId = challenge.SubCategory.SubCategoryId,
                            SubCategoryName = challenge.SubCategory.SubCategoryName,
                            ImagePath = challenge.SubCategory.ImagePath,
                        },
                JoinedUsers = challenge.Participants.Select(u => u.UserName ?? "Unknown").ToList(),
                UploadedResults = challenge.UploadedResults.Select(ur => new UploadedResultDTO
                {
                    UploadedResultId = ur.UploadedResultId,
                    ChallengeId = ur.ChallengeId,
                    Url = ur.Url,
                    UserId = ur.UserId,
                    SubmissionDate = ur.SubmissionDate,
                    UserName = ur.User?.UserName ?? "Unknown",
                    Votes = ur.Votes.Select(vote => new VoteEntity
                    {
                        Id = vote.Id,
                        ChallengeId = vote.ChallengeId,
                        UploadedResultId = vote.UploadedResultId,
                        UserId = vote.UserId,
                    }).ToList()
                }).ToList(),
                CreatedBy = challenge.CreatedBy,
                CreatorUserName = challenge.Creator?.UserName ?? "Unknown",
            };

            return Ok(challengeDTO);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> CreateChallenge([FromBody] CreateChallengeDTO createChallengeDTO, CancellationToken ct)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var endDate = createChallengeDTO.EndDate;
            var votePeriodEnd = endDate.AddDays(7);
            var isTakenDown = votePeriodEnd.AddDays(7);

            var challenge = new Challenge
            {
                ChallengeName = createChallengeDTO.ChallengeName,
                EndDate = endDate,
                VotePeriodEnd = votePeriodEnd,
                IsTakenDown = isTakenDown,
                Description = createChallengeDTO.Description,
                NumberOfParticipants = createChallengeDTO.NumberOfParticipants,
                SubCategoryId = createChallengeDTO.SubCategoryId,
                CreatedBy = currentUserId,
            };

            var created = await _challengeRepo.CreateChallengeAsync(challenge, ct);
            return CreatedAtAction(nameof(GetChallengeById), new { id = created.ChallengeId }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateChallenge([FromRoute] int id, [FromBody] UpdateChallengeDTO updateChallengeDTO, CancellationToken ct)
        {
            var challenge = await _challengeRepo.GetChallengeByIdAsync(id, ct);
            if (challenge == null)
            {
                return NotFound($"Challenge with id {id} was not found in the database");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Admin" && challenge.CreatedBy != currentUserId)
            {
                return Forbid("You can only update your own challenges");
            }

            var endDate = updateChallengeDTO.EndDate;
            var votePeriodEnd = endDate.AddDays(7);
            var isTakenDown = votePeriodEnd.AddDays(7);

            var updatedChallenge = new Challenge
            {
                ChallengeName = updateChallengeDTO.ChallengeName,
                EndDate = endDate,
                VotePeriodEnd = votePeriodEnd,
                IsTakenDown = isTakenDown,
                Description = updateChallengeDTO.Description,
                SubCategoryId = updateChallengeDTO.SubCategoryId,
            };

            var result = await _challengeRepo.UpdateChallengeAsync(id, updatedChallenge, ct);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteChallenge([FromRoute] int id, CancellationToken ct)
        {
            var challenge = await _challengeRepo.GetChallengeByIdAsync(id, ct);
            if (challenge == null)
            {
                return NotFound($"Challenge with id {id} was not found in the database");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Admin" && challenge.CreatedBy != currentUserId)
            {
                return Forbid("You can only delete your own challenges");
            }

            await _challengeRepo.DeleteChallengeAsync(id, ct);
            return NoContent();
        }

        [HttpPost("{challengeId:int}/join")]
        [Authorize]
        public async Task<IActionResult> JoinChallenge([FromRoute] int challengeId, CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _challengeRepo.AddUserToChallengeAsync(challengeId, userId, ct);
            return result switch
            {
                ChallengeJoinStatus.Success => Ok("Joined challenge successfully."),
                ChallengeJoinStatus.ChallengeFull => Conflict("Challenge is already full."),
                ChallengeJoinStatus.AlreadyJoined => Conflict("You have already joined this challenge."),
                ChallengeJoinStatus.ChallengeNotFound or ChallengeJoinStatus.UserNotFound => NotFound("Challenge or user not found."),
                _ => StatusCode(500, "Unknown error.")
            };
        }

        [HttpPost("{challengeId:int}/leave")]
        [Authorize]
        public async Task<IActionResult> LeaveChallenge([FromRoute] int challengeId, CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _challengeRepo.RemoveUserFromChallengeAsync(challengeId, userId, ct);
            if (!success)
                return NotFound($"Challenge or user not found.");

            return Ok("Left challenge successfully.");
        }

        [HttpPost("{challengeId:int}/upload-result")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UploadResult([FromRoute] int challengeId, [FromBody] string uploadedResultURL, CancellationToken ct)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var newUploadedResult = new UploadedResult
            {
                Url = uploadedResultURL,
                ChallengeId = challengeId,
                UserId = currentUserId,
                SubmissionDate = DateTime.UtcNow,
            };

            var result = await _challengeRepo.AddUploadedResultToChallengeAsync(challengeId, newUploadedResult, ct);

            return result switch
            {
                UploadResultStatus.Success => CreatedAtAction(nameof(GetChallengeById), new { id = challengeId }, newUploadedResult.Url),
                UploadResultStatus.NotJoined => StatusCode(403, "You must join the challenge before uploading a result."),
                UploadResultStatus.AlreadyUploaded => Conflict("You have already uploaded a result for this challenge."),
                UploadResultStatus.ChallengeNotFound => NotFound("Challenge not found."),
                UploadResultStatus.ChallengeEnded => Conflict("You cannot upload results to a challenge that has ended."),
                _ => StatusCode(500, "Unknown error.")
            };
        }

        [HttpDelete("{challengeId:int}/uploaded-result")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteUploadedResult([FromRoute] int challengeId, CancellationToken ct)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var success = await _challengeRepo.DeleteUploadedResultAsync(challengeId, currentUserId, ct);
            if (!success)
                return NotFound("Uploaded result not found or you do not have permission to delete it.");

            return NoContent();
        }

        [HttpPost("{challengeId:int}/uploaded-result/vote/{uploadedResultId:int}")]
        [Authorize]
        public async Task<IActionResult> AddVote([FromRoute] int challengeId, [FromRoute] int uploadedResultId, CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _challengeRepo.AddOrMoveVoteAsync(challengeId, uploadedResultId, userId, ct);
            if (!success)
                return NotFound($"Uploaded result not found.");

            return Ok("Voted successfully.");
        }

        [HttpPost("{challengeId:int}/submit-result")]
        [Authorize]
        public async Task<IActionResult> SubmitResult([FromRoute] int challengeId, CancellationToken ct)
        {
            var challenge = await _challengeRepo.GetChallengeByIdAsync(challengeId, ct);
            if (challenge == null)
                return NotFound($"Challenge with id {challengeId} was not found.");

            if (!challenge.SubCategoryId.HasValue)
                return BadRequest("Challenge does not have a subcategory.");

            var categoryId = challenge.SubCategory.CategoryId;

            // Ensure all users have a rating for this subcategory
            await _eloRatingService.EnsureRatingsExistForParticipantsAsync(challenge.Participants, categoryId, challenge.SubCategoryId.Value, challenge, ct);
            await _eloRatingService.UpdateEloRatingsAsync(challenge.Participants, categoryId, challenge.SubCategoryId.Value, challenge, ct);
            return Ok("Ratings ensured for all participants.");
        }
    }
}
