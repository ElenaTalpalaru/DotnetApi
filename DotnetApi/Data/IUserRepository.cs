using DotnetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Data
{
    public interface IUserRepository
    {
        public bool SaveChanges();

        public void AddEntity<T>(T entityToAdd);

        public void RemoveEntity<T>(T entityToAdd);

        public IEnumerable<User> GetUsers();

        public User GetSingleUser(int userId);

        public IEnumerable<UserJobInfo> GetUserJobInfo();

        public UserJobInfo GetSingleUsersJobInfo(int userId);


    }
}
