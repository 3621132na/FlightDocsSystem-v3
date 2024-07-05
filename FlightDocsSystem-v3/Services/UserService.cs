using FlightDocsSystem_v3.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;
using FlightDocsSystem_v3.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FlightDocsSystem_v3.Services
{
    public class UserService:IUserService
    {
        private readonly FlightDocsSystemContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(FlightDocsSystemContext dbContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User> RegisterUser(User user)
        {
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
                throw new Exception("Username is already taken.");
            if (!IsEmailInVietjetDomain(user.Email))
                throw new Exception("Email must belong to @vietjetair.com domain.");
            ValidatePassword(user.Password);
            var password=user.Password;
            user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            ValidatePhoneNumber(user.PhoneNumber);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            var subject = "Registration Successful";
            var body = $"Dear {user.Username},\n\nYour registration was successful. Here are your details:\n" +
                       $"Email: {user.Email}\n" +
                       $"Username: {user.Username}\n" +
                       $"Phone Number: {user.PhoneNumber}\n" +
                       $"Password: {password}\n\n" +
                       "Please keep this information safe.\n\n" +
                       "Best regards,\nYour Team";
            SendEmail(user.Email, subject, body);
            return user;
        }
        public async Task<string> Login(LoginViewModel model)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                throw new Exception("Invalid email or password");
            if (!IsEmailInVietjetDomain(model.Email))
                throw new Exception("Email must belong to @vietjetair.com domain.");
            if (user.Role == null)
                throw new Exception("User role is not assigned.");
            var token = GenerateJwtToken(user);
            return token;
        }
        public async Task<User> GetUserById(int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
                throw new Exception("User not found.");
            return user;
        }
        public async Task<IEnumerable<User>> GetUsersByRole(string role)
        {
            var users = await _dbContext.Users
                .Where(u => u.Role == role)
                .ToListAsync();
            return users;
        }
        public async Task<User> UpdateUser(int id,UserViewModel user)
        {
            var existingUser = await _dbContext.Users.FindAsync(id);
            if (existingUser == null)
                throw new Exception("User not found");
            var currentUserRole = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole == "Admin")
            {
                existingUser.Email = user.Email;
                existingUser.Username = user.Username;
                existingUser.PhoneNumber = user.PhoneNumber;
            }
            else if (currentUserRole == "GO")
            {
                if (existingUser.Role == "Admin" || existingUser.Role == "GO")
                    throw new Exception("You are not authorized to update this user.");
                existingUser.Email = user.Email;
                existingUser.Username = user.Username;
                existingUser.PhoneNumber = user.PhoneNumber;
            }
            else if (currentUserRole == "Pilot" || currentUserRole == "Crew")
                throw new Exception("You are not authorized to update any users.");
            else
                throw new Exception("Unauthorized access.");
            _dbContext.Users.Update(existingUser);
            await _dbContext.SaveChangesAsync();
            return existingUser;
        }

        public async Task<bool> DeleteUser(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
                return false;
            var currentUserRole = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole == "Admin")
                _dbContext.Users.Remove(user);
            else if (currentUserRole == "GO")
            {
                if (user.Role == "Admin" || user.Role == "GO")
                    throw new Exception("You are not authorized to delete this user.");
                _dbContext.Users.Remove(user);
            }
            else if (currentUserRole == "Pilot" || currentUserRole == "Crew")
                throw new Exception("You are not authorized to delete any users.");
            else
                throw new Exception("Unauthorized access.");
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _dbContext.Users.ToListAsync();
        }
        public async Task<bool> ForgotPassword(string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new Exception("Email not found.");
            var newPassword = GenerateRandomPassword();
            ValidatePassword(newPassword);
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _dbContext.Entry(user).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            SendEmail(user.Email, "Password Reset", $"Your new password is: {newPassword}");
            return true;
        }

        public async Task<bool> ChangeOwnerAccountAsync(int newOwnerId)
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new Exception("Current user not found.");
            var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == int.Parse(currentUserId));
            if (currentUser == null)
                throw new Exception("Current user not found.");
            var newOwner = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == newOwnerId);
            if (newOwner == null)
                throw new Exception("New owner user not found.");
            var currentUserRole = currentUser.Role;
            currentUser.Role = newOwner.Role;
            newOwner.Role = currentUserRole;
            _dbContext.Entry(currentUser).State = EntityState.Modified;
            _dbContext.Entry(newOwner).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private bool IsEmailInVietjetDomain(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Trim().ToLower().EndsWith("@vietjetair.com");
        }
        private void ValidatePassword(string password)
        {
            if (password.Length < 8)
                throw new Exception("Password must be at least 8 characters long");
            if (!password.Any(char.IsUpper))
                throw new Exception("Password must contain at least one uppercase letter");
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?"":{}|<>]"))
                throw new Exception("Password must contain at least one special character");
        }
        private void ValidatePhoneNumber(string phoneNumber)
        {
            var regex = new Regex(@"^0\d{9}$");
            if (!regex.IsMatch(phoneNumber))
                throw new ValidationException("Phone number must start with 0 and be exactly 10 digits.");
        }
        private void SendEmail(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
            {
                Port = int.Parse(_configuration["Smtp:Port"]),
                Credentials = new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]),
                EnableSsl = true,
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Smtp:FromEmail"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);
            smtpClient.Send(mailMessage);
        }
        private string GenerateRandomPassword()
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*(),.?\":{}|<>";
            const string allChars = upper + lower + digits + special;
            var random = new Random();
            var passwordChars = new char[12];
            passwordChars[0] = upper[random.Next(upper.Length)];
            passwordChars[1] = lower[random.Next(lower.Length)];
            passwordChars[2] = digits[random.Next(digits.Length)];
            passwordChars[3] = special[random.Next(special.Length)];
            for (int i = 4; i < passwordChars.Length; i++)
                passwordChars[i] = allChars[random.Next(allChars.Length)];
            return new string(passwordChars.OrderBy(c => random.Next()).ToArray());
        }
    }
}
