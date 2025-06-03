using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillChallenge.DTOs;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace SkillChallenge.Controllers
{
    [ApiController]
    [Route("categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepo;

        public CategoryController(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
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
                    UnderCategories = c
                        .UnderCategories.Select(uc => new UnderCategoryDTO
                        {
                            UnderCategoryId = uc.UnderCategoryId,
                            UnderCategoryName = uc.UnderCategoryName,
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
                UnderCategories = category
                    .UnderCategories.Select(uc => new UnderCategoryDTO
                    {
                        UnderCategoryId = uc.UnderCategoryId,
                        UnderCategoryName = uc.UnderCategoryName,
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
            [FromBody] CreateCategoryDTO createCategoryDTO,
            CancellationToken ct
        )
        {
            var category = new Category { CategoryName = createCategoryDTO.CategoryName };

            var created = await _categoryRepo.CreateCategoryAsync(category, ct);

            var categoryDTO = new CategoryDTO
            {
                CategoryId = created.CategoryId,
                CategoryName = created.CategoryName,
                UnderCategories = new List<UnderCategoryDTO>(),
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
                UnderCategories = new List<UnderCategoryDTO>(),
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
