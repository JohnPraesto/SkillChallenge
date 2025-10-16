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
    [Route("/api/challenges")]
    public class ChallengeController : ControllerBase
    {
        private readonly IChallengeRepository _challengeRepo;
        private readonly EloRatingService _eloRatingService;
        private readonly IMediaService _mediaService;
        private static readonly string[] AllowedExtensions = { ".mp4", ".webm", ".mov", ".pdf", ".jpg", ".jpeg", ".png" };


        public ChallengeController(IChallengeRepository challengeRepo, EloRatingService eloRatingService, IMediaService mediaService)
        {
            _challengeRepo = challengeRepo;
            _eloRatingService = eloRatingService;
            _mediaService = mediaService;
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
                    CreatedDate = c.CreatedDate,
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
                                CategoryId = c.SubCategory.CategoryId,
                                CategoryName = c.SubCategory.Category?.CategoryName ?? string.Empty,
                                ImagePath = c.SubCategory.ImagePath,
                            },
                    JoinedUsers = c.Participants.Select(u => u.UserName ?? "Unknown").ToList(),
                    CreatedBy = c.CreatedBy,
                    CreatorUserName = c.Creator?.UserName ?? "Unknown",
                    ResultsSubmitted = c.ResultsSubmitted,
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
                                CategoryId = c.SubCategory.CategoryId,
                                CategoryName = c.SubCategory.Category?.CategoryName ?? string.Empty,
                                ImagePath = c.SubCategory.ImagePath,
                            },
                    JoinedUsers = c.Participants.Select(u => u.UserName ?? "Unknown").ToList(),
                    CreatedBy = c.CreatedBy,
                    CreatorUserName = c.Creator?.UserName ?? "Unknown",
                    ResultsSubmitted = c.ResultsSubmitted,
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
                            CategoryId = challenge.SubCategory.CategoryId,
                            CategoryName = challenge.SubCategory.Category?.CategoryName ?? string.Empty,
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
                    FreeText = ur.FreeText,
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
                ResultsSubmitted = challenge.ResultsSubmitted,
            };

            return Ok(challengeDTO);
        }

        [HttpGet("{challengeName}")]
        public async Task<IActionResult> GetChallengesByChallengeName([FromRoute] string challengeName, CancellationToken ct)
        {
            var challenges = await _challengeRepo.GetChallengesByChallengeNameAsync(challengeName, ct);
            bool isEmpty = !challenges.Any();
            if (isEmpty)
            {
                return NotFound($"No challenge named {challengeName} was found in the database");
            }

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
                                CategoryId = c.SubCategory.CategoryId,
                                CategoryName = c.SubCategory.Category?.CategoryName ?? string.Empty,
                                ImagePath = c.SubCategory.ImagePath,
                            },
                    JoinedUsers = c.Participants.Select(u => u.UserName ?? "Unknown").ToList(),
                    CreatedBy = c.CreatedBy,
                    CreatorUserName = c.Creator?.UserName ?? "Unknown",
                    ResultsSubmitted = c.ResultsSubmitted,
                })
                .ToList();

            return Ok(challengeDTOs);
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
                CreatedDate = DateTime.UtcNow,
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

            foreach (var uploadedResult in challenge.UploadedResults)
            {
                if (IsUploadedFileUrl(uploadedResult.Url))
                {
                    await _mediaService.DeleteMediaAsync(uploadedResult.Url);
                }
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

            var challenge = await _challengeRepo.GetChallengeByIdAsync(challengeId, ct);
            var uploadedResult = challenge?.UploadedResults.FirstOrDefault(ur => ur.UserId == userId);

            if (uploadedResult != null)
            {
                if (IsUploadedFileUrl(uploadedResult.Url))
                {
                    await _mediaService.DeleteMediaAsync(uploadedResult.Url);
                }
                await _challengeRepo.DeleteUploadedResultAsync(challengeId, userId, ct);
            }

            var success = await _challengeRepo.RemoveUserFromChallengeAsync(challengeId, userId, ct);
            if (!success)
                return NotFound($"Challenge or user not found.");

            return Ok("Left challenge successfully.");
        }

        [RequestSizeLimit(200_000_000)]
        [HttpPost("{challengeId:int}/upload-result")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UploadResult([FromRoute] int challengeId, [FromForm] UploadedResultRequest uploadedResultRequest, CancellationToken ct)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            // Validate input: Only one of YoutubeUrl or File should be provided
            bool hasYoutubeUrl = !string.IsNullOrWhiteSpace(uploadedResultRequest.YoutubeUrl);
            bool hasFile = uploadedResultRequest.File != null;
            bool hasFreeText = uploadedResultRequest.FreeText != null;

            int providedCount = (hasYoutubeUrl ? 1 : 0) + (hasFile ? 1 : 0) + (hasFreeText ? 1 : 0);

            if (providedCount != 1)
                return BadRequest("You must provide either a YouTube link or a file or free text.");

            string? resultUrl = null;

            if (hasYoutubeUrl)
            {
                // Optionally: Validate YouTube URL format here
                resultUrl = uploadedResultRequest.YoutubeUrl;
            }
            else if (hasFile)
            {
                var allowedExtensions = new[] { ".mp4", ".webm", ".mov", ".pdf", ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp" };
                var ext = Path.GetExtension(uploadedResultRequest.File.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                    return BadRequest("Invalid file type. Allowed: mp4, webm, mov, pdf, jpg, jpeg, png.");

                long fileSize = uploadedResultRequest.File.Length;
                long MaxFileSizeBytes = 200 * 1024 * 1024; // 200 MB per file

                if (uploadedResultRequest.File.Length > MaxFileSizeBytes)
                {
                    return BadRequest("File is too large. Maximum allowed size is 200 MB.");
                }

                var totalBytes = await _challengeRepo.GetTotalUploadedFileSizeAsync(ct);

                const long GlobalStorageLimitBytes = 200L * 1024 * 1024 * 1024; // 200 GB storage threshold

                if (hasFile && (totalBytes + uploadedResultRequest.File.Length > GlobalStorageLimitBytes))
                {
                    return BadRequest("Storage limit reached. No more uploads are allowed at this time.");
                }

                // Save to Azure Blob Storage
                resultUrl = await _mediaService.SaveMediaAsync(uploadedResultRequest.File, "challenge-media");
            }
            else if (hasFreeText)
            {
                resultUrl = uploadedResultRequest.FreeText;
            }

            var newUploadedResult = new UploadedResult
            {
                ChallengeId = challengeId,
                UserId = currentUserId,
                SubmissionDate = DateTime.UtcNow,
                FileSize = hasFile ? uploadedResultRequest.File.Length : null,
            };

            if (hasFreeText)
            {
                newUploadedResult.FreeText = uploadedResultRequest.FreeText;
            }
            else
            {
                newUploadedResult.Url = resultUrl;
            }

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

            var challenge = await _challengeRepo.GetChallengeByIdAsync(challengeId, ct);
            var uploadedResult = challenge?.UploadedResults.FirstOrDefault(ur => ur.UserId == currentUserId);

            if (uploadedResult == null)
                return NotFound("Uploaded result not found or you do not have permission to delete it.");

            if (IsUploadedFileUrl(uploadedResult.Url))
            {
                await _mediaService.DeleteMediaAsync(uploadedResult.Url);
            }

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
            await _eloRatingService.EnsureRatingsExistForParticipantsAsync(challenge, challenge.Participants, categoryId, challenge.SubCategoryId.Value, ct);
            await _eloRatingService.UpdateEloRatingsAsync(challenge.Participants, categoryId, challenge.SubCategoryId.Value, challenge, ct);
            return Ok("Ratings ensured for all participants.");
        }

        private bool IsUploadedFileUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // Check for GUID in the URL
            var guidMatch = System.Text.RegularExpressions.Regex.Match(url, @"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}");
            if (!guidMatch.Success)
                return false;

            // Check for allowed extension
            var hasAllowedExtension = AllowedExtensions.Any(ext => url.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
            return hasAllowedExtension;

            // if false is returned then url is probably a youtube link
            // and not link to a file in a storage
        }
    }
}
