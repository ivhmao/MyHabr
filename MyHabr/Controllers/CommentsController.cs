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

namespace MyHabr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly IServiceWrapper _serviceWrapper;

        public CommentsController(IServiceWrapper serviceWrapper)
        {
            _serviceWrapper = serviceWrapper ?? throw new ArgumentNullException(nameof(serviceWrapper));
        }

        // GET: api/Comments
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Comment>>> GetAllComments()
        {
            try
            {
                var comments = await _serviceWrapper.Comment.GetAllCommentsAsync();

                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            try
            {
                var comm = await _serviceWrapper.Comment.GetCommentByIdAsync(id);
                if(comm == null)
                {
                    return NotFound();
                }

                return Ok(comm);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
                     
            }
        }

        // PUT: api/Comments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutComment(int id, CommentForUpdateDTO comment)
        {
            try
            {
                if(comment == null)
                {
                    return BadRequest("Comment object is null");
                }

                if(comment.Id != id)
                {
                    return BadRequest("Comment object isn't correct (Id)");
                }

                var commentEntity = await _serviceWrapper.Comment.GetCommentByIdAsync(id);
                if(commentEntity == null)
                {
                    return NotFound("Comment not found");
                }

                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return BadRequest("Need authentication");
                }

                if (commentEntity.UserId != currentUser.Id && currentUser.Roles.Any(r=>r.Name.Equals("Admin")|| r.Name.Equals("Moderator")))
                {
                    return BadRequest("Only authors/administrators/moderators can update comments");
                }

                commentEntity.Content = comment.Content;
                commentEntity.Updated = DateTime.Now;
                commentEntity.UpdatedByUserId = currentUser.Id;

                _serviceWrapper.Comment.UpdateComment(commentEntity);
                await _serviceWrapper.SaveAsync();

                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Comments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Comment>> PostComment(CommentForCreateDTO comment)
        {
            try
            {
                if (comment == null)
                {
                    return BadRequest("Comment object is null");
                }

                if (string.IsNullOrEmpty(comment.Content))
                {
                    return BadRequest("Comment is empty");
                }

                var article = await _serviceWrapper.Article.GetArticleByIdAsync(comment.ArticleId);
                if(article == null)
                {
                    return BadRequest("Article doesn't exists");
                }

                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return BadRequest("Need authentication");
                }

                var commentEntity = new Comment()
                {
                    ArticleId = comment.ArticleId,
                    ParentId = comment.ParentId,
                    Content = comment.Content,
                    Created = DateTime.Now,
                    CommentState = CommentState.Approved,
                    CommentType = CommentType.Ordinary,
                    UserId = currentUser.Id
                };

                _serviceWrapper.Comment.CreateComment(commentEntity);
                await _serviceWrapper.SaveAsync();

                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var comm = await _serviceWrapper.Comment.GetCommentByIdAsync(id);
                if(comm == null)
                {
                    return NotFound("Comment not found");
                }

                if (comm.Children.Count > 0)
                {
                    return BadRequest("You cannot delete comments with child comments");
                }

                var currentUser = GetCurrentUser();
                if(currentUser == null)
                {
                    return BadRequest("Need authentication");
                }

                if (comm.UserId != currentUser.Id && currentUser.Roles.Any(r => r.Name.Equals("Admin") || r.Name.Equals("Moderator")))
                {
                    return BadRequest("Only authors/administrator/moderators can delete comments");
                }

                _serviceWrapper.Comment.DeleteComment(comm);
                await _serviceWrapper.SaveAsync();

                return Ok();
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
