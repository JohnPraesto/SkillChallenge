using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillChallenge.DTOs;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace SkillChallenge.Controllers
{
    [ApiController]
    [Route("undercategories")]
    public class UnderCategoryController : ControllerBase
    {
        private readonly IUnderCategoryRepository _underCategoryRepo;
        private readonly ICategoryRepository _categoryRepo;

        public UnderCategoryController(
            IUnderCategoryRepository underCategoryRepo,
            ICategoryRepository categoryRepo
        )
        {
            _underCategoryRepo = underCategoryRepo;
            _categoryRepo = categoryRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUnderCategories(CancellationToken ct)
        {
            var underCategories = await _underCategoryRepo.GetAllUnderCategoriesAsync(ct);
            var underCategoryDTOs = underCategories
                .Select(uc => new UnderCategoryDTO
                {
                    UnderCategoryId = uc.UnderCategoryId,
                    UnderCategoryName = uc.UnderCategoryName,
                    CategoryId = uc.CategoryId,
                    CategoryName = uc.Category?.CategoryName ?? string.Empty,
                })
                .ToList();

            return Ok(underCategoryDTOs);
        }

        [HttpGet("category/{categoryId:int}")]
        public async Task<IActionResult> GetUnderCategoriesByCategoryId(
            [FromRoute] int categoryId,
            CancellationToken ct
        )
        {
            var underCategories = await _underCategoryRepo.GetUnderCategoriesByCategoryIdAsync(
                categoryId,
                ct
            );
            var underCategoryDTOs = underCategories
                .Select(uc => new UnderCategoryDTO
                {
                    UnderCategoryId = uc.UnderCategoryId,
                    UnderCategoryName = uc.UnderCategoryName,
                    CategoryId = uc.CategoryId,
                    CategoryName = uc.Category?.CategoryName ?? string.Empty,
                })
                .ToList();

            return Ok(underCategoryDTOs);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUnderCategoryById(
            [FromRoute] int id,
            CancellationToken ct
        )
        {
            var underCategory = await _underCategoryRepo.GetUnderCategoryByIdAsync(id, ct);
            if (underCategory == null)
            {
                return NotFound($"UnderCategory with id {id} was not found in the database");
            }

            var underCategoryDTO = new UnderCategoryDTO
            {
                UnderCategoryId = underCategory.UnderCategoryId,
                UnderCategoryName = underCategory.UnderCategoryName,
                CategoryId = underCategory.CategoryId,
                CategoryName = underCategory.Category?.CategoryName ?? string.Empty,
            };

            return Ok(underCategoryDTO);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> CreateUnderCategory(
            [FromBody] CreateUnderCategoryDTO createUnderCategoryDTO,
            CancellationToken ct
        )
        {
            // Kontrollera att kategorin finns
            if (!await _categoryRepo.CategoryExistsAsync(createUnderCategoryDTO.CategoryId, ct))
            {
                return BadRequest(
                    $"Category with id {createUnderCategoryDTO.CategoryId} does not exist"
                );
            }

            var underCategory = new UnderCategory
            {
                UnderCategoryName = createUnderCategoryDTO.UnderCategoryName,
                CategoryId = createUnderCategoryDTO.CategoryId,
            };

            var created = await _underCategoryRepo.CreateUnderCategoryAsync(underCategory, ct);

            return CreatedAtAction(
                nameof(GetUnderCategoryById),
                new { id = created.UnderCategoryId },
                created
            );
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateUnderCategory(
            [FromRoute] int id,
            [FromBody] UpdateUnderCategoryDTO updateUnderCategoryDTO,
            CancellationToken ct
        )
        {
            // Kontrollera att kategorin finns
            if (!await _categoryRepo.CategoryExistsAsync(updateUnderCategoryDTO.CategoryId, ct))
            {
                return BadRequest(
                    $"Category with id {updateUnderCategoryDTO.CategoryId} does not exist"
                );
            }

            var underCategoryToUpdate = new UnderCategory
            {
                UnderCategoryName = updateUnderCategoryDTO.UnderCategoryName,
                CategoryId = updateUnderCategoryDTO.CategoryId,
            };

            var underCategory = await _underCategoryRepo.UpdateUnderCategoryAsync(
                id,
                underCategoryToUpdate,
                ct
            );
            if (underCategory == null)
            {
                return NotFound($"UnderCategory with id {id} was not found in the database");
            }

            return Ok(underCategory);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUnderCategory(
            [FromRoute] int id,
            CancellationToken ct
        )
        {
            var underCategory = await _underCategoryRepo.DeleteUnderCategoryAsync(id, ct);
            if (underCategory == null)
            {
                return NotFound($"UnderCategory with id {id} was not found in the database");
            }
            return NoContent();
        }
    }
}
