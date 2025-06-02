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
            return Ok(challenges);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetChallengeById([FromRoute] int id, CancellationToken ct)
        {
            var challenge = await _challengeRepo.GetChallengeByIdAsync(id, ct);
            if (challenge == null)
            {
                return NotFound($"Challenge with id {id} was not found in the database");
            }
            return Ok(challenge);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> CreateChallenge(
            [FromBody] CreateChallengeDTO createChallengeDTO,
            CancellationToken ct
        )
        {
            var challenge = new Challenge
            {
                ChallengeName = createChallengeDTO.ChallengeName,
                EndDate = createChallengeDTO.EndDate,
                TimePeriod = createChallengeDTO.TimePeriod,
                Description = createChallengeDTO.Description,
                IsPublic = createChallengeDTO.IsPublic,
                UnderCategory = createChallengeDTO.UnderCategoryId.HasValue
                    ? new UnderCategory
                    {
                        UnderCategoryId = createChallengeDTO.UnderCategoryId.Value,
                    }
                    : null,
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

            // Kontrollera behörigheter - användare kan bara uppdatera sina egna challenges
            // (Du behöver lägga till CreatedBy property i Challenge model för detta)
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
            {
                // Här skulle du kontrollera om användaren äger challenge:et
                // if (challenge.CreatedBy != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                // {
                //     return Forbid("You can only update your own challenges");
                // }
            }

            var updatedChallenge = new Challenge
            {
                ChallengeName = updateChallengeDTO.ChallengeName,
                EndDate = updateChallengeDTO.EndDate,
                TimePeriod = updateChallengeDTO.TimePeriod,
                Description = updateChallengeDTO.Description,
                IsPublic = updateChallengeDTO.IsPublic,
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

            // Kontrollera behörigheter - användare kan bara ta bort sina egna challenges
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
            {
                // Här skulle du kontrollera om användaren äger challenge:et
                // if (challenge.CreatedBy != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                // {
                //     return Forbid("You can only delete your own challenges");
                // }
            }

            await _challengeRepo.DeleteChallengeAsync(id, ct);
            return NoContent();
        }
    }
}
