namespace FootyTipping.Server.Authorization
{
    public interface IHashingUtilities
    {
        bool Verify(string password, string passwordHash);
        string HashPassword(string password);
    }

    public class HashingUtilities : IHashingUtilities
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
