using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillChallenge.DTOs;
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
                    SubCategories = c
                        .SubCategories.Select(uc => new SubCategoryDTO
                        {
                            SubCategoryId = uc.SubCategoryId,
                            SubCategoryName = uc.SubCategoryName, // Corrected duplicate initialization
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
        public async Task<IActionResult> CreateCategory(
            [FromForm] CreateCategoryDTO dto,
            CancellationToken ct
        )
        {
            string imagePath = "";
            if (dto.Image != null)
            {
                imagePath = await _imageService.SaveImageAsync(dto.Image, "categories");
            }
            else
            {
                imagePath = "images/categories/default.png";
            }

            var category = new Category { CategoryName = dto.CategoryName, ImagePath = imagePath };

            var created = await _categoryRepo.CreateCategoryAsync(category, ct);

            var categoryDTO = new CategoryDTO
            {
                CategoryId = created.CategoryId,
                CategoryName = created.CategoryName,
                ImageUrl = _imageService.GetImageUrl(created.ImagePath),
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
        public async Task<IActionResult> UpdateCategory(
            [FromRoute] int id,
            [FromBody] UpdateCategoryDTO updateCategoryDTO,
            CancellationToken ct
        )
        {
            var categoryToUpdate = new Category { CategoryName = updateCategoryDTO.CategoryName };

            var category = await _categoryRepo.UpdateCategoryAsync(id, categoryToUpdate, ct);
            if (category == null)
            {
                return NotFound($"Category with id {id} was not found in the database");
            }

            var categoryDTO = new CategoryDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                SubCategories = new List<SubCategoryDTO>(),
            };

            return Ok(categoryDTO);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id, CancellationToken ct)
        {
            var category = await _categoryRepo.DeleteCategoryAsync(id, ct);
            if (category == null)
            {
                return NotFound($"Category with id {id} was not found in the database");
            }
            return NoContent();
        }
    }
}
