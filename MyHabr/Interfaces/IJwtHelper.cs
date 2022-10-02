using MyHabr.Entities;

namespace MyHabr.Interfaces
{
    public interface IJwtHelper
    {
        string GeterateJwtToken(User user);
        int? VerifyJwtToken(string token);
    }
}