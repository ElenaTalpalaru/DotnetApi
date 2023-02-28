using DotnetApi.Data;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System;
using AutoMapper;
using System.Linq;

namespace DotnetApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserJobInfoEF : ControllerBase
    {

        IUserRepository _userRepository;
        IMapper _mapper;

        public UserJobInfoEF(IConfiguration config, IUserRepository userRepository)
        {
            _userRepository = userRepository;

            _mapper = new Mapper(new MapperConfiguration(cfg =>{
                cfg.CreateMap<UserJobInfo, UserJobInfo>();
            }));
        }

        // Not working, need to be solved:
        //[HttpGet("UserJobInfo")]          
        //public IEnumerable<UserJobInfo> UserJob()
        //{
        //    return _userRepository.GetUserJobInfo();
        //}

        [HttpGet("GetUserJobInfo/{userId}")]
        public UserJobInfo GetSingleUsersJobInfo(int userId)
        {
            return _userRepository.GetSingleUsersJobInfo(userId);        
           
        }

        [HttpPut("EditUserJobInfo")]
        public IActionResult EditUser(UserJobInfo userJobInfo)
        {
            UserJobInfo? userjobInfoDb = _userRepository.GetSingleUsersJobInfo(userJobInfo.UserId);

            if (userjobInfoDb is not null)
            {
                userjobInfoDb.JobTitle = userJobInfo.JobTitle;
                userjobInfoDb.Department = userJobInfo.Department;
               

                if (_userRepository.SaveChanges())
                {
                    return Ok();   
                }
            }
            throw new Exception("Failed to update User");
        }

        [HttpPost("CreateUserJob")]
        public IActionResult AddUser(UserJobInfo userJobInfoDto)
        {
            UserJobInfo userJobInfo = _mapper.Map<UserJobInfo>(userJobInfoDto);
            try
            {
                _userRepository.AddEntity<UserJobInfo>(userJobInfo);
                _userRepository.SaveChanges();
                return Created($"GetUserJobInfo/{userJobInfo.UserId}", userJobInfoDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteJobInfo/{userId}")]
        public IActionResult DeleteJobUser(int userId)
        {
            UserJobInfo? userjobInfoDb = _userRepository.GetSingleUsersJobInfo(userId);

            if (userjobInfoDb != null)
            {
                _userRepository.RemoveEntity<UserJobInfo>(userjobInfoDb);

                if (_userRepository.SaveChanges())
                {
                    return Ok();
                }
            }
            throw new Exception("Failed to delete job info");
        }

    }
} 