using Microsoft.AspNetCore.Identity;

namespace EcommApp.Repository
{
    public interface ITokenRepository
    {
        string CreateJwtToken(IdentityUser user, List<string> roles);
    }
}
