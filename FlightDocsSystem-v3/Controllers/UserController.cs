using FlightDocsSystem_v3.Data;
using FlightDocsSystem_v3.Models;
using FlightDocsSystem_v3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlightDocsSystem_v3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(User user)
        {
            try
            {
                var registeredUser = await _userService.RegisterUser(user);
                return Ok(registeredUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                var user = await _userService.Login(model);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthenticationCookieName");
            return Ok(new { message = "Logout successful." });
        }

        [HttpGet("userinfo")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();
                var user = await _userService.GetUserById(int.Parse(userId));
                if (user == null)
                    return NotFound(new { message = "User not found." });
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("edit/{id}")]
        [Authorize(Roles = "Admin, GO")]
        public async Task<IActionResult> UpdateUser(int id, UserViewModel user)
        {
            try
            {
                var updatedUser = await _userService.UpdateUser(id,user);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin, GO")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUser(id);
                if (result)
                    return Ok(new { message = "User deleted successfully." });
                else
                    return NotFound(new { message = "User not found." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("all")]
        [Authorize(Roles = "Admin, GO")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("detail/{id}")]
        [Authorize(Roles = "Admin, GO")]
        public async Task<IActionResult> GetUserDetail(int id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null)
                    return NotFound(new { message = "User not found." });
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin, GO")]
        [HttpGet("users-by-role")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            try
            {
                var users = await _userService.GetUsersByRole(role);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                var success = await _userService.ForgotPassword(email);
                if (success)
                    return Ok(new { message = "Password reset link sent to email." });
                else
                    return BadRequest(new { message = "Error processing request." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("changeowner")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeOwnerAccount(int newOwnerId)
        {
            try
            {
                var success = await _userService.ChangeOwnerAccountAsync(newOwnerId);
                if (success)
                    return Ok(new { message = "Owner account changed successfully." });
                else
                    return BadRequest(new { message = "Error processing request." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
