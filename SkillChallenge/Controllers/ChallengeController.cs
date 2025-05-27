using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> CreateChallenge(
            [FromBody] Challenge newChallenge,
            CancellationToken ct
        )
        {
            var created = await _challengeRepo.CreateChallengeAsync(newChallenge, ct);
            return CreatedAtAction(
                nameof(GetChallengeById),
                new { id = created.ChallengeId },
                created
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateChallenge(
            [FromRoute] int id,
            [FromBody] Challenge updatedChallenge,
            CancellationToken ct
        )
        {
            var challenge = await _challengeRepo.UpdateChallengeAsync(id, updatedChallenge, ct);
            if (challenge == null)
            {
                return NotFound($"Challenge with id {id} was not found in the database");
            }
            return Ok(challenge);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteChallenge([FromRoute] int id, CancellationToken ct)
        {
            var challenge = await _challengeRepo.DeleteChallengeAsync(id, ct);
            if (challenge == null)
            {
                return NotFound($"Challenge with id {id} was not found in the database");
            }
            return NoContent();
        }
    }
}
