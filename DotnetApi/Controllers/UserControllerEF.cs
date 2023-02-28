using DotnetApi.Data;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System;
using AutoMapper;

namespace DotnetApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserControllerEF : ControllerBase
    {
        IUserRepository _userRepository;
        IMapper _mapper;

        public UserControllerEF(IConfiguration config, IUserRepository userRepository)
        {
            _userRepository = userRepository;

            _mapper = new Mapper(new MapperConfiguration(cfg =>{
                cfg.CreateMap<UserToAddDto, User>();
            }));
        }             
        
        [HttpGet("GetUsers")]          
        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _userRepository.GetUsers();
            return users;
        }

        [HttpGet("GetUsers/{userId}")]
        //public IEnumerable<User> GetUsers()
        public User GetSingleUser(int userId)
        {
            return _userRepository.GetSingleUser(userId);
        }

        [HttpPut("EditUser")]
        public IActionResult EditUser(User user)
        {
            User? userDb = _userRepository.GetSingleUser(user.UserId);

            if (userDb != null)
            {
                userDb.Active = user.Active;
                userDb.FirstName = user.FirstName;
                userDb.LastName = user.LastName;
                userDb.Gender = user.Gender;
                userDb.Email = user.Email;

                if (_userRepository.SaveChanges())
                {
                    return Ok();   
                }
            }
            throw new Exception("Failed to update User");
        }

        [HttpPost]
        public IActionResult AddUser(UserToAddDto user)
        {
            User userDb = _mapper.Map<User>(user);           

            _userRepository.AddEntity<User>(userDb);

            if (_userRepository.SaveChanges())
            {
                return Ok();
            }            
            throw new Exception("Failed to add User");
        }


        [HttpDelete("DeleteUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            User? userDb = _userRepository.GetSingleUser(userId);

            if (userDb != null)
            {
                _userRepository.RemoveEntity<User>(userDb);                    

                if (_userRepository.SaveChanges())
                {
                    return Ok();
                }
            }
            throw new Exception("Failed to delete User");
        }

    }
} 