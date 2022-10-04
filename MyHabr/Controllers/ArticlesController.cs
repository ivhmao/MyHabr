using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyHabr.Entities;
using MyHabr.Enums;
using MyHabr.Helpers;
using MyHabr.Interfaces;
using MyHabr.Models;
using MyHabr.Services;

namespace MyHabr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IServiceWrapper _serviceWrapper;

        public ArticlesController(IServiceWrapper serviceWrapper)
        {
            _serviceWrapper = serviceWrapper ?? throw new ArgumentNullException(nameof(serviceWrapper));
        }

        // GET: api/Articles
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Article>>> GetAllArticles()
        {
            try
            {
                var articles = await _serviceWrapper.Article.GetAllArticlesAsync();

                var articlesResult = articles.Select(a=> new ArticleDTO(a));
                return Ok(articlesResult);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Articles/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Article>> GetArticle(int id)
        {
            try
            {
                var article = await _serviceWrapper.Article.GetArticleByIdAsync(id);
                if(article == null)
                {
                    return NotFound();
                }
                else
                {
                    var articleResult = new ArticleDTO(article);
                    return Ok(articleResult);
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Articles/5/Approve
        [HttpPut("{id}/Approve")]
        [Authorize(Roles = "Moderator, Admin")]
        public async Task<IActionResult> ApproveArticle(int id)
        {
            try
            {
                var article = await _serviceWrapper.Article.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                if(article.ArticleState != ArticleState.Published)
                {
                    BadRequest("Only published articles can be approved");
                }

                article.ArticleState = ArticleState.Approved;

                _serviceWrapper.Article.UpdateArticle(article);
                await _serviceWrapper.SaveAsync();

                return NoContent();
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Articles/5/Publish
        [HttpPut("{id}/Publish")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> PublishArticle(int id)
        {
            try
            {
                var article = await _serviceWrapper.Article.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                if (article.ArticleState != ArticleState.Draft)
                {
                    BadRequest("Only drafts can be published");
                }
                
                var currentUser = GetCurrentUser();
                if (currentUser==null)
                {
                    return BadRequest("Need authentication");
                }

                if (!article.Authors.Any(u => u.Id == currentUser.Id))
                {
                    return BadRequest("Only authors can publish their articles");
                }

                article.ArticleState = ArticleState.Published;

                _serviceWrapper.Article.UpdateArticle(article);
                await _serviceWrapper.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Articles/5/Unpublish
        [HttpPut("{id}/Unpublish")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> UnpublishArticle(int id)
        {
            try
            {
                var article = await _serviceWrapper.Article.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                if (article.ArticleState != ArticleState.Published)
                {
                    BadRequest("Only published articles can be unpublished");
                }

                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return BadRequest("Need authentication");
                }

                if (!article.Authors.Any(u => u.Id == currentUser.Id))
                {
                    return BadRequest("Only authors can unpublish their articles");
                }

                article.ArticleState = ArticleState.Draft;

                _serviceWrapper.Article.UpdateArticle(article);
                await _serviceWrapper.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Articles/5/Decline
        [HttpPut("{id}/Decline")]
        [Authorize(Roles = "Moderator, Admin")]
        public async Task<IActionResult> DeclineArticle(int id)
        {
            try
            {
                var article = await _serviceWrapper.Article.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                if (article.ArticleState != ArticleState.Published)
                {
                    BadRequest("Only published articles can be declined");
                }

                article.ArticleState = ArticleState.Declined;

                _serviceWrapper.Article.UpdateArticle(article);
                await _serviceWrapper.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Articles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] ArticleForUpdateDTO article)
        {
            try
            {
                if(article == null)
                {
                    return BadRequest("Article object is null");
                }

                if (article.Id != id)
                {
                    return BadRequest("Article object isn't correct (id)");
                }

                // TODO model validation

                var articleEntity = await _serviceWrapper.Article.GetArticleByIdAsync(id);
                if(articleEntity == null)
                {
                    return NotFound();
                }

                if (articleEntity.ArticleState != ArticleState.Draft)
                {
                    return BadRequest("Article can only be edited in draft state");
                }

                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return BadRequest("Need authentication");
                }

                if (!articleEntity.Authors.Contains(currentUser))
                {
                    return BadRequest("Only authors can update articles");
                }

                articleEntity.Title = article.Title;
                articleEntity.Content = article.Content;
                articleEntity.Authors = _serviceWrapper.User.GetById(article.Authors);

                _serviceWrapper.Article.UpdateArticle(articleEntity);
                await _serviceWrapper.SaveAsync();

                return NoContent();
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Articles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<ActionResult<Article>> CreateArticle([FromBody] ArticleForCreateDTO article)
        {
            try
            {
                if (article == null)
                {
                    return BadRequest("Article object is null");
                }

                //TODO model validation



                var articleEntity = new Article()
                {
                    Title = article.Title,
                    CreatedDate = DateTime.UtcNow,
                    Content = article.Content,
                    ArticleState = ArticleState.Draft,
                    Authors = _serviceWrapper.User.GetById(article.Authors).ToList()
                };

                _serviceWrapper.Article.CreateArticle(articleEntity);
                await _serviceWrapper.SaveAsync();

                var createdArticle = _serviceWrapper.Article.GetArticleByIdAsync(articleEntity.Id); //TODO Use AutoMapper

                return CreatedAtAction("GetArticle", new { id = createdArticle.Id }, createdArticle);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Articles/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Writer, Admin")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            try
            {
                var article = await _serviceWrapper.Article.GetArticleByIdAsync(id);
                if(article == null)
                {
                    return NotFound();
                }

                //TODO delete/check referenced objects

                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return BadRequest("Need authentication");
                }

                if (!article.Authors.Contains(currentUser) && !currentUser.Roles.Any(r=>r.Name.Equals("Admin")))
                {
                    return BadRequest("Only authors or admins can delete articles");
                }

                _serviceWrapper.Article.DeleteArticle(article);
                await _serviceWrapper.SaveAsync();

                return NoContent();
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private User? GetCurrentUser()
        {
            var _context = ControllerContext.HttpContext;
            var token = _context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
            {
                return null;
            }

            var currentUser = _serviceWrapper.User.GetCurrentUser(token);
            return currentUser;
        }
    }
}
