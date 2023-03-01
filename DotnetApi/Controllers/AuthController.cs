using Dapper;
using DotnetApi.Data;
using DotnetApi.Dtos;
using DotnetApi.Helpers;
using DotnetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DotnetApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        private readonly AuthHelper _authHelper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordCofirm)
            {
                string sqlCheckUserExists =
                    "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + userForRegistration.Email + "'";

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if (existingUsers.Count() == 0)
                {
                    UserForLoginDto userForSetPassword = new UserForLoginDto()
                    {
                        Email = userForRegistration.Email,
                        Password= userForRegistration.Password,
                    };

                    if (_authHelper.SetPassword(userForSetPassword))
                    {
                        string sqlAddUser = $@"EXEC TutorialAppSchema.spUser_Upsert
                        @FirstName = '{userForRegistration.FirstName}',
                        @LastName = '{userForRegistration.LastName}',
                        @Email = '{userForRegistration.Email}',
                        @Gender = '{userForRegistration.Gender}',
                        @Active = 1,
                        @JobTitle = '{userForRegistration.JobTitle}',
                        @Department = '{userForRegistration.Department}',
                        @Salary = {userForRegistration.Salary}";


                        if (_dapper.ExecuteSql(sqlAddUser))
                        {
                            return Ok(userForRegistration.Email);
                        }
                        throw new Exception("Failed to register user.");
                    }
                    throw new Exception("Failed to register user.");

                }
                return Conflict("User with this email already exists");
            }
            return BadRequest("Passwords do not match!");
        }

        [HttpPut("ResetPassword")] // bug: I can reset password of any other user!!!! I have to rest password only for my email
        public IActionResult ResetPassword(UserForLoginDto userForSetPassword)
        {
            if (_authHelper.SetPassword(userForSetPassword))
            {
                return Ok();
            }
            return BadRequest("Failed to update password");
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = @"EXEC TutorialAppSchema.spLoginConfirmation_Get 
                @Email = @EmailParam";

            DynamicParameters sqlParameters = new DynamicParameters();

            // SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar);
            // emailParameter.Value = userForLogin.Email;
            // sqlParameters.Add(emailParameter);

            sqlParameters.Add("@EmailParam", userForLogin.Email, DbType.String);

            UserLoginConfirmationDto userForConfirmation = _dapper
                .LoadDataSingleWithParameters<UserLoginConfirmationDto>(sqlForHashAndSalt, sqlParameters);

            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            // if (passwordHash == userForConfirmation.PasswordHash) // Won't work

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                {
                    return StatusCode(401, "Incorrect password!");
                }
            }

            string userIdSql = @"
                SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '" +
                userForLogin.Email + "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(userId)}
            });
        }


        [HttpGet("RefreshToken")]
        public string refreshToken()
        {
            string userIdSql = @"
                SELECT [UserId] FROM  TutorialAppSchema.Users WHERE UserId = '" +
                 User.FindFirst("userId")?.Value + "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return _authHelper.CreateToken(userId);
        }
              
    }
}
