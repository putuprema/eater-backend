﻿namespace Infrastructure.Services
{
    public class PasswordEncoderService : IPasswordEncoderService
    {
        public string Encode(string rawPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(rawPassword);
        }

        public bool Matches(string rawPassword, string encodedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(rawPassword, encodedPassword);
        }
    }
}
