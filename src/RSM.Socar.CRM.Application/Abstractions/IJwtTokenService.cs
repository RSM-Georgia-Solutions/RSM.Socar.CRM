using RSM.Socar.CRM.Domain.Identity;

namespace RSM.Socar.CRM.Application.Abstractions
{
    public interface IJwtTokenService
    {
        string CreateAccessToken(User user);
    }
}
