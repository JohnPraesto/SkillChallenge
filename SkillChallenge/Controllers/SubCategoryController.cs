using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillChallenge.DTOs;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;
using SkillChallenge.Services;

namespace SkillChallenge.Controllers
{
    [ApiController]
    [Route("subcategories")]
    public class SubCategoryController : ControllerBase
    {
        private readonly ISubCategoryRepository _subCategoryRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IImageService _imageService;

        public SubCategoryController(
            ISubCategoryRepository subCategoryRepo,
            ICategoryRepository categoryRepo,
            IImageService imageService
        )
        {
            _subCategoryRepo = subCategoryRepo;
            _categoryRepo = categoryRepo;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubCategories(CancellationToken ct)
        {
            var subCategories = await _subCategoryRepo.GetAllSubCategoriesAsync(ct);
            var subCategoryDTOs = subCategories
                .Select(uc => new SubCategoryDTO
                {
                    SubCategoryId = uc.SubCategoryId,
                    SubCategoryName = uc.SubCategoryName,
                    CategoryId = uc.CategoryId,
                    CategoryName = uc.Category?.CategoryName ?? string.Empty,
                })
                .ToList();

            return Ok(subCategoryDTOs);
        }

        [HttpGet("category/{categoryId:int}")]
        public async Task<IActionResult> GetSubCategoriesByCategoryId(
            [FromRoute] int categoryId,
            CancellationToken ct
        )
        {
            var subCategories = await _subCategoryRepo.GetSubCategoriesByCategoryIdAsync(
                categoryId,
                ct
            );
            var subCategoryDTOs = subCategories
                .Select(uc => new SubCategoryDTO
                {
                    SubCategoryId = uc.SubCategoryId,
                    SubCategoryName = uc.SubCategoryName,
                    CategoryId = uc.CategoryId,
                    CategoryName = uc.Category?.CategoryName ?? string.Empty,
                })
                .ToList();

            return Ok(subCategoryDTOs);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSubCategoryById(
            [FromRoute] int id,
            CancellationToken ct
        )
        {
            var subCategory = await _subCategoryRepo.GetSubCategoryByIdAsync(id, ct);
            if (subCategory == null)
            {
                return NotFound($"SubCategory with id {id} was not found in the database");
            }

            var subCategoryDTO = new SubCategoryDTO
            {
                SubCategoryId = subCategory.SubCategoryId,
                SubCategoryName = subCategory.SubCategoryName,
                CategoryId = subCategory.CategoryId,
                CategoryName = subCategory.Category?.CategoryName ?? string.Empty,
            };

            return Ok(subCategoryDTO);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> CreateSubCategory(
            [FromForm] CreateSubCategoryDTO dto,
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
                imagePath = await _imageService.SaveImageAsync(dto.Image, "subcategories");
            }
            else
            {
                // Om ingen bild valts, använd categoryns bild
                imagePath = category.ImagePath;
            }

            var subCategory = new SubCategory
            {
                SubCategoryName = dto.SubCategoryName,
                CategoryId = dto.CategoryId,
                ImagePath = imagePath,
            };

            var created = await _subCategoryRepo.CreateSubCategoryAsync(subCategory, ct);

            var subCategoryDTO = new SubCategoryDTO
            {
                SubCategoryId = created.SubCategoryId,
                SubCategoryName = created.SubCategoryName,
                CategoryId = created.CategoryId,
                CategoryName = category.CategoryName,
                ImagePath = _imageService.GetImageUrl(created.ImagePath),
            };

            return CreatedAtAction(
                nameof(GetSubCategoryById),
                new { id = created.SubCategoryId },
                subCategoryDTO
            );
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateSubCategory(
            [FromRoute] int id,
            [FromBody] UpdateSubCategoryDTO updateSubCategoryDTO,
            CancellationToken ct
        )
        {
            if (!await _categoryRepo.CategoryExistsAsync(updateSubCategoryDTO.CategoryId, ct))
            {
                return BadRequest(
                    $"Category with id {updateSubCategoryDTO.CategoryId} does not exist"
                );
            }

            var subCategoryToUpdate = new SubCategory
            {
                SubCategoryName = updateSubCategoryDTO.SubCategoryName,
                CategoryId = updateSubCategoryDTO.CategoryId,
            };

            var subCategory = await _subCategoryRepo.UpdateSubCategoryAsync(
                id,
                subCategoryToUpdate,
                ct
            );
            if (subCategory == null)
            {
                return NotFound($"SubCategory with id {id} was not found in the database");
            }

            return Ok(subCategory);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSubCategory([FromRoute] int id, CancellationToken ct)
        {
            var subCategory = await _subCategoryRepo.DeleteSubCategoryAsync(id, ct);
            if (subCategory == null)
            {
                return NotFound($"SubCategory with id {id} was not found in the database");
            }
            return NoContent();
        }
    }
}
