using FlightDocsSystem_v3.Data;
using FlightDocsSystem_v3.Models;

namespace FlightDocsSystem_v3.Services
{
    public interface IUserService
    {
        Task<User> RegisterUser(User user);
        Task<string> Login(LoginViewModel model);
        Task<User> GetUserById(int id);
        Task<IEnumerable<User>> GetUsersByRole(string role);
        Task<User> UpdateUser(int id, UserViewModel user);
        Task<bool> DeleteUser(int id);
        Task<IEnumerable<User>> GetAllUsers();
        Task<bool> ForgotPassword(string email);
        Task<bool> ChangeOwnerAccountAsync(int newOwnerId);
    }
}
