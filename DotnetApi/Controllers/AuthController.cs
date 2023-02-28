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
                    byte[] passwordSalt = new byte[128 / 8];

                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

                    string sqlAddAuth = @"INSERT INTO TutorialAppSchema.Auth (
                                                Email,
                                                PasswordHash,
                                                PasswordSalt
                                            ) 
                                          VALUES ('" + userForRegistration.Email +
                                          "', @PasswordHash, @PasswordSalt)";

                    Console.WriteLine(sqlAddAuth);
                    List<SqlParameter> sqlParameter = new List<SqlParameter>();

                    SqlParameter passwordSaltParamater = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                    passwordSaltParamater.Value = passwordSalt;

                    SqlParameter passwordHashParamater = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                    passwordHashParamater.Value = passwordHash;

                    sqlParameter.Add(passwordSaltParamater);
                    sqlParameter.Add(passwordHashParamater);

                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameter)) { }
                    {
                        string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users
                            ([FirstName],
                            [LastName],
                            [Email],
                            [Gender],
                            [Active]) 
                        VALUES ("
                                +
                             "'" + userForRegistration.FirstName +
                             "', '" + userForRegistration.LastName +
                             "', '" + userForRegistration.Email +
                             "', '" + userForRegistration.Gender +
                             "', 1)";
                        if (_dapper.ExecuteSql(sqlAddUser))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to register user.");
                    }
                    throw new Exception("Failed to register user.");

                }
                return Conflict("User with this email already exists");
            }
            return BadRequest("Passwords do not match!");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto UserForLogin)
        {
            string sqlForHashAndSalt = @"SELECT
                                  [PasswordHash]
                                  ,[PasswordSalt]
                              FROM [DotNetCourseDatabase].[TutorialAppSchema].[Auth] 
                              WHERE Email = '" +
                              UserForLogin.Email + "'";

            UserLoginConfirmationDto userforConfirmation;

            try
            {
                userforConfirmation = _dapper
                .LoadDataSingle<UserLoginConfirmationDto>(sqlForHashAndSalt)!;
            }
            catch (Exception)
            {
                return Unauthorized("user not found");
            }
             

            byte[] passwordHash = _authHelper.GetPasswordHash(UserForLogin.Password, userforConfirmation.PasswordSalt);

            for (int i = 0; i < passwordHash.Length; i++)
            {
                if (passwordHash[i] != userforConfirmation.PasswordHash[i])
                {
                    return StatusCode(401, "Incorrect password");
                }
            }

            string userIdSql = @"
                SELECT [UserId] FROM  TutorialAppSchema.Users WHERE Email = '" +
                UserForLogin.Email + "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string>
            {
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
