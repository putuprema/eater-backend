namespace Infrastructure.Config
{
    public class JwtConfig
    {
        public string ValidIssuer { get; set; }
        public string Secret { get; set; }
        public int Lifetime { get; set; }
    }
}
