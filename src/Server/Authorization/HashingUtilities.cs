namespace FootyTipping.Server.Authorization
{
    public interface IHashingUtilities
    {
        bool Verify(string password, string passwordHash);
    }

    public class HashingUtilities : IHashingUtilities
    {
        public bool Verify(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
