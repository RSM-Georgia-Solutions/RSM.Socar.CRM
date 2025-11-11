namespace RSM.Socar.CRM.Application.Auth
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public string SigningKey { get; set; } = default!; // long random string
        public int AccessTokenMinutes { get; set; } = 60;
    }
}
