using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DotnetApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        public PostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("Posts")]
        public IEnumerable<Post> GetPosts()
        {
            string sql = @"SELECT * FROM TutorialAppSchema.Posts";
            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("PostSingle/{postId}")]
        public IEnumerable<Post> GetPostSingle(int postId)
        {
            string sql = @"SELECT * FROM TutorialAppSchema.Posts WHERE PostId = " + postId.ToString();

            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("PostByUser/{userId}")]
        public IEnumerable<Post> PostByUser(int userId)
        {
            string sql = @"SELECT * FROM TutorialAppSchema.Posts WHERE UserId = " + userId.ToString();

            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> MyPosts()
        {
            string sql = @"SELECT * FROM TutorialAppSchema.Posts WHERE UserId = " + this.User.FindFirst("userId")?.Value;

            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("PostsBySearch/{searchParam}")]
        public IEnumerable<Post> PostsBySearch(string searchParam)
        {
            string sql = $@"SELECT * FROM TutorialAppSchema.Posts
                            WHERE PostTitle LIKE '%{searchParam}%' 
                               OR PostContent LIKE '%{searchParam}%'"; 

            return _dapper.LoadData<Post>(sql);
        }

        [HttpPost("Post")]
        public IActionResult AddPost(PostToAddDto postToAdd)
        {
            string sqlPost = $@"INSERT INTO TutorialAppSchema.Posts(
                                [UserId],
                                [PostTitle],
                                [PostContent],
                                [PostCreated],
                                [PostUpdated]
                            ) VALUES (
                                 {this.User.FindFirst("userId")?.Value}, 
                                '{postToAdd.PostTitle}', 
                                '{postToAdd.PostContent}', 
                                GETDATE(), 
                                GETDATE()
                            )";

            if (_dapper.ExecuteSql(sqlPost))
            {
                return Ok();
            }

            return BadRequest("Failed to create new post!");
        }
                                   
        [HttpPut("Post")]
        public IActionResult EditPost(PostToEditDto postToEdit)
        {

            string sqlPost = @$"UPDATE TutorialAppSchema.Posts 
                                SET PostContent = '{postToEdit.PostContent}', 
                                    PostTitle = '{postToEdit.PostTitle}',
                                    PostUpdated = GETDATE()
                                WHERE PostId = {postToEdit.PostId} 
                                    AND UserId = {this.User.FindFirst("userId")?.Value}";

            if (_dapper.ExecuteSql(sqlPost))
            {
                return Ok();
            }

            return BadRequest("Failed to edit new post!");
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @$"DELETE FROM TutorialAppSchema.Posts WHERE PostId = {postId} AND UserId = {this.User.FindFirst("userId")?.Value}";

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            return BadRequest("Failed to delete new post!");
        }
    }
}
