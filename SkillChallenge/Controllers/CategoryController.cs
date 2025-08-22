using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillChallenge.DTOs.Category;
using SkillChallenge.DTOs.SubCategory;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;
using SkillChallenge.Services;

namespace SkillChallenge.Controllers
{
    [ApiController]
    [Route("categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IImageService _imageService;

        public CategoryController(ICategoryRepository categoryRepo, IImageService imageService)
        {
            _categoryRepo = categoryRepo;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories(CancellationToken ct)
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync(ct);
            var categoryDTOs = categories
                .Select(c => new CategoryDTO
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ImagePath = c.ImagePath,
                    SubCategories = c
                        .SubCategories.Select(uc => new SubCategoryDTO
                        {
                            SubCategoryId = uc.SubCategoryId,
                            SubCategoryName = uc.SubCategoryName,
                            CategoryId = uc.CategoryId,
                            CategoryName = c.CategoryName,
                        })
                        .ToList(),
                })
                .ToList();

            return Ok(categoryDTOs);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategoryById([FromRoute] int id, CancellationToken ct)
        {
            var category = await _categoryRepo.GetCategoryByIdAsync(id, ct);
            if (category == null)
            {
                return NotFound($"Category with id {id} was not found in the database");
            }

            var categoryDTO = new CategoryDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                SubCategories = category
                    .SubCategories.Select(uc => new SubCategoryDTO
                    {
                        SubCategoryId = uc.SubCategoryId,
                        SubCategoryName = uc.SubCategoryName,
                        CategoryId = uc.CategoryId,
                        CategoryName = category.CategoryName,
                    })
                    .ToList(),
            };

            return Ok(categoryDTO);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromForm] CreateCategoryDTO dto, CancellationToken ct)
        {
            string imagePath = "";
            if (dto.Image != null)
            {
                imagePath = await _imageService.SaveImageAsync(dto.Image, "category-images");
            }
            else
            {   // Den här finns inte längre?
                imagePath = "images/categories/default.png";
            }

            var category = new Category { CategoryName = dto.CategoryName, ImagePath = imagePath };

            var created = await _categoryRepo.CreateCategoryAsync(category, ct);

            var categoryDTO = new CategoryDTO
            {
                CategoryId = created.CategoryId,
                CategoryName = created.CategoryName,
                ImagePath = _imageService.GetImageUrl(created.ImagePath),
                SubCategories = new List<SubCategoryDTO>(),
            };

            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = created.CategoryId },
                categoryDTO
            );
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromForm] UpdateCategoryDTO updateCategoryDTO, CancellationToken ct)
        {
            var existingCategory = await _categoryRepo.GetCategoryByIdAsync(id, ct);
            if (existingCategory == null)
                return NotFound($"Category with id {id} was not found in the database");

            if (updateCategoryDTO.Image != null)
            {
                if (!string.IsNullOrEmpty(existingCategory.ImagePath) && !existingCategory.ImagePath.Contains("default"))
                {
                    await _imageService.DeleteImageAsync(existingCategory.ImagePath);
                }
                existingCategory.ImagePath = await _imageService.SaveImageAsync(updateCategoryDTO.Image, "category-images");
            }

            existingCategory.CategoryName = updateCategoryDTO.CategoryName;

            await _categoryRepo.UpdateCategoryAsync(id, existingCategory, ct);

            var categoryDTO = new CategoryDTO
            {
                CategoryId = existingCategory.CategoryId,
                CategoryName = existingCategory.CategoryName,
                ImagePath = _imageService.GetImageUrl(existingCategory.ImagePath),
                SubCategories = new List<SubCategoryDTO>(),
            };

            return Ok(categoryDTO);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id, CancellationToken ct)
        {
            var category = await _categoryRepo.GetCategoryByIdAsync(id, ct);
            if (category == null)
                return NotFound($"Category with id {id} was not found in the database");

            if (!string.IsNullOrEmpty(category.ImagePath) && !category.ImagePath.Contains("default"))
            {
                await _imageService.DeleteImageAsync(category.ImagePath);
            }

            await _categoryRepo.DeleteCategoryAsync(id, ct);
            return NoContent();
        }
    }
}
