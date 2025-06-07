using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillChallenge.DTOs;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace SkillChallenge.Controllers
{
    [ApiController]
    [Route("challenges")]
    public class ChallengeController : ControllerBase
    {
        private readonly IChallengeRepository _challengeRepo;

        public ChallengeController(IChallengeRepository challengeRepo)
        {
            _challengeRepo = challengeRepo;
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
                    TimePeriod = c.TimePeriod,
                    Description = c.Description,
                    IsPublic = c.IsPublic,
                    SubCategoryId = c.SubCategoryId,
                    UserIds = c.Users.Select(u => int.Parse(u.Id)).ToList(),
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
                    TimePeriod = c.TimePeriod,
                    Description = c.Description,
                    IsPublic = c.IsPublic,
                    SubCategoryId = c.SubCategoryId,
                    UserIds = c.Users.Select(u => int.Parse(u.Id)).ToList(),
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
                TimePeriod = challenge.TimePeriod,
                Description = challenge.Description,
                IsPublic = challenge.IsPublic,
                SubCategoryId = challenge.SubCategoryId,
                UserIds = challenge.Users.Select(u => int.Parse(u.Id)).ToList(),
                CreatedBy = challenge.CreatedBy,
                CreatorUserName = challenge.Creator?.UserName ?? "Unknown",
            };

            return Ok(challengeDTO);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> CreateChallenge(
            [FromBody] CreateChallengeDTO createChallengeDTO,
            CancellationToken ct
        )
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var challenge = new Challenge
            {
                ChallengeName = createChallengeDTO.ChallengeName,
                EndDate = createChallengeDTO.EndDate,
                TimePeriod = createChallengeDTO.TimePeriod,
                Description = createChallengeDTO.Description,
                IsPublic = createChallengeDTO.IsPublic,
                SubCategoryId = createChallengeDTO.SubCategoryId,
                CreatedBy = currentUserId,
            };

            var created = await _challengeRepo.CreateChallengeAsync(challenge, ct);
            return CreatedAtAction(
                nameof(GetChallengeById),
                new { id = created.ChallengeId },
                created
            );
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateChallenge(
            [FromRoute] int id,
            [FromBody] UpdateChallengeDTO updateChallengeDTO,
            CancellationToken ct
        )
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

            var updatedChallenge = new Challenge
            {
                ChallengeName = updateChallengeDTO.ChallengeName,
                EndDate = updateChallengeDTO.EndDate,
                TimePeriod = updateChallengeDTO.TimePeriod,
                Description = updateChallengeDTO.Description,
                IsPublic = updateChallengeDTO.IsPublic,
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
    }
}
