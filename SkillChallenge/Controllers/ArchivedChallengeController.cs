using Microsoft.AspNetCore.Mvc;
using SkillChallenge.Interfaces;

namespace SkillChallenge.Controllers
{
    [ApiController]
    [Route("/api/archived-challenges")]
    public class ArchivedChallengeController : Controller
    {
        private readonly IArchivedChallengeRepository _acRepo;

        public ArchivedChallengeController(IArchivedChallengeRepository acRepo)
        {
            _acRepo = acRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllArchivedChallenges(CancellationToken ct)
        {
            var archivedChallenges = await _acRepo.GetAllArchivedChallengesAsync(ct);
            return Ok(archivedChallenges);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetArchivedChallengesByUserId([FromRoute] string userId, CancellationToken ct)
        {
            var archivedChallenges = await _acRepo.GetArchivedChallengesByUserIdAsync(userId, ct);
            if (archivedChallenges == null || archivedChallenges.Count == 0)
            {
                return NotFound($"No archived challenges with user id {userId} was found in the database");
            }
            return Ok(archivedChallenges);
        }
    }
}
