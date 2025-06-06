using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillChallenge.DTOs;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;
using SkillChallenge.Services;

namespace SkillChallenge.Controllers
{
    [ApiController]
    [Route("undercategories")]
    public class UnderCategoryController : ControllerBase
    {
        private readonly IUnderCategoryRepository _underCategoryRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IImageService _imageService;

        public UnderCategoryController(
            IUnderCategoryRepository underCategoryRepo,
            ICategoryRepository categoryRepo,
            IImageService imageService
        )
        {
            _underCategoryRepo = underCategoryRepo;
            _categoryRepo = categoryRepo;
            _imageService = imageService;
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
            [FromForm] CreateUnderCategoryDTO dto,
            CancellationToken ct
        )
        {
            // Kontrollera att kategorin finns
            var category = await _categoryRepo.GetCategoryByIdAsync(dto.CategoryId, ct);
            if (category == null)
                return BadRequest($"Category with id {dto.CategoryId} does not exist");

            string imagePath = null;
            if (dto.Image != null)
            {
                imagePath = await _imageService.SaveImageAsync(dto.Image, "undercategories");
            }
            else
            {
                // Om ingen bild valts, använd categoryns bild
                imagePath = category.ImagePath;
            }

            var underCategory = new UnderCategory
            {
                UnderCategoryName = dto.UnderCategoryName,
                CategoryId = dto.CategoryId,
                ImagePath = imagePath,
            };

            var created = await _underCategoryRepo.CreateUnderCategoryAsync(underCategory, ct);

            var underCategoryDTO = new UnderCategoryDTO
            {
                UnderCategoryId = created.UnderCategoryId,
                UnderCategoryName = created.UnderCategoryName,
                CategoryId = created.CategoryId,
                CategoryName = category.CategoryName,
                ImageUrl = _imageService.GetImageUrl(created.ImagePath),
            };

            return CreatedAtAction(
                nameof(GetUnderCategoryById),
                new { id = created.UnderCategoryId },
                underCategoryDTO
            );
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateUnderCategory(
            [FromRoute] int id,
            [FromForm] UpdateUnderCategoryDTO updateUnderCategoryDTO,
            CancellationToken ct
        )
        {
            var existing = await _underCategoryRepo.GetUnderCategoryByIdAsync(id, ct);
            if (existing == null)
                return NotFound($"UnderCategory with id {id} was not found in the database");

            if (updateUnderCategoryDTO.Image != null)
            {
                if (
                    !string.IsNullOrEmpty(existing.ImagePath)
                    && existing.ImagePath != existing.Category.ImagePath
                )
                {
                    var fullPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        existing.ImagePath
                    );
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
                existing.ImagePath = await _imageService.SaveImageAsync(
                    updateUnderCategoryDTO.Image,
                    "undercategories"
                );
            }

            existing.UnderCategoryName = updateUnderCategoryDTO.UnderCategoryName;
            existing.CategoryId = updateUnderCategoryDTO.CategoryId;

            await _underCategoryRepo.UpdateUnderCategoryAsync(id, existing, ct);

            var underCategoryDTO = new UnderCategoryDTO
            {
                UnderCategoryId = existing.UnderCategoryId,
                UnderCategoryName = existing.UnderCategoryName,
                CategoryId = existing.CategoryId,
                CategoryName = existing.Category?.CategoryName ?? "",
                ImageUrl = _imageService.GetImageUrl(existing.ImagePath),
            };

            return Ok(underCategoryDTO);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUnderCategory(
            [FromRoute] int id,
            CancellationToken ct
        )
        {
            var underCategory = await _underCategoryRepo.GetUnderCategoryByIdAsync(id, ct);
            if (underCategory == null)
                return NotFound($"UnderCategory with id {id} was not found in the database");

            if (
                !string.IsNullOrEmpty(underCategory.ImagePath)
                && underCategory.ImagePath != underCategory.Category.ImagePath
            )
            {
                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    underCategory.ImagePath
                );
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            await _underCategoryRepo.DeleteUnderCategoryAsync(id, ct);
            return NoContent();
        }
    }
}
