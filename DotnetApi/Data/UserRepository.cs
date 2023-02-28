using AutoMapper;
using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DotnetApi.Data
{
    public class UserRepository : IUserRepository
    {
       
        DataContextEF _entityFramework;

        public UserRepository(IConfiguration config)
        {
            _entityFramework = new DataContextEF(config);
        }

        public bool SaveChanges()
        {
            return _entityFramework.SaveChanges() > 0;
        }

        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd is not null)
            {
                _entityFramework.Add(entityToAdd);
            }            
        }

        public void RemoveEntity<T>(T entityToAdd)
        {
            if (entityToAdd is not null)
            {
                _entityFramework.Remove(entityToAdd);
            }
        }

        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _entityFramework.Users.ToList<User>();
            return users;
        }

        public User GetSingleUser(int userId)
        {
            User? user = _entityFramework.Users.Where(u => u.UserId == userId)
                .FirstOrDefault<User>();
            if (user != null)
            {
                return user;
            }
            throw new Exception("Failed to get user");
        }

        public IEnumerable<UserJobInfo> GetUserJobInfo()
        {
            IEnumerable<UserJobInfo> userJobInfo = _entityFramework.UserJobInfo.ToList<UserJobInfo>();
            return userJobInfo;
        }
       
        public UserJobInfo GetSingleUsersJobInfo(int userId)
        {
            UserJobInfo? userJob = _entityFramework.UserJobInfo.Where(u => u.UserId == userId)
                .FirstOrDefault<UserJobInfo>();
            if (userJob != null)
            {
                return userJob;            }

            throw new Exception("Failed to get job info");
        }


    }
}
