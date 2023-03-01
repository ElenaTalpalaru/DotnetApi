using Dapper;
using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Data;
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

        [HttpGet("{postId}/{userId}/{searchParam}")]
        public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string? searchParam = null)
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get";
            string stringParameters = "";

            //DynamicParameters sqlParameters = new DynamicParameters();
            if (postId != 0)
            {
                stringParameters += ", @PostId=" + postId.ToString();
                //sqlParameters.Add("@PostIdParameter", postId, DbType.Int32);
            }
            if (userId != 0)
            {
                stringParameters += ", @UserId=" + userId.ToString();
                //sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
            }
            if (searchParam != null)
            {
                stringParameters += $", @SearchValue='{searchParam}'";
               // sqlParameters.Add("@SearchValueParameter", searchParam, DbType.String);
            }

            if (stringParameters.Length > 0)
            {
                sql += stringParameters.Substring(1);
            }

            return _dapper.LoadData<Post>(sql);
        }             

        [HttpGet("MyPosts")]
        public IEnumerable<Post> MyPosts()
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get @UserId = " + 
                this.User.FindFirst("userId")?.Value;

            return _dapper.LoadData<Post>(sql);
        }

        [HttpPut("Add_Or_UpdatePost")]
        public IActionResult UpsertPost(Post postToUpsert)
        {
            string sqlPost = $@"EXEC TutorialAppSchema.spPosts_Upsert
                                 @UserId = {this.User.FindFirst("userId")?.Value},
                                 @PostTitle = '{postToUpsert.PostTitle}',
                                 @PostContent = '{postToUpsert.PostContent}'";
            //@PostId INT = NULL

            if (postToUpsert.PostId > 0)
            {
                sqlPost += $", @PostId = {postToUpsert.PostId}";
            }
                             
            if (_dapper.ExecuteSql(sqlPost))
            {
                return Ok();
            }

            return BadRequest("Failed to upsert new post!");
        }                                
       

        [HttpDelete("{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @$"EXEC TutorialAppSchema.spPost_Delete 
                            @PostId = {postId},
                            @UserId = {this.User.FindFirst("userId")?.Value}";

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            return BadRequest("Failed to delete post!");
        }
    }
}
