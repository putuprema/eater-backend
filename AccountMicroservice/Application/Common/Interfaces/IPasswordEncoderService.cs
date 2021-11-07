namespace Application.Common.Interfaces
{
    public interface IPasswordEncoderService
    {
        public string Encode(string rawPassword);
        public bool Matches(string rawPassword, string encodedPassword);
    }
}
