using RestWithASPNETUdemy.Data.VO;
using RestWithASPNETUdemy.Model;
using RestWithASPNETUdemy.Model.Base;

namespace RestWithASPNETUdemy.Repository
{

    public interface IUserRepository
    {
        User ValidateCredentials(UserVO user);

        User RefreshTokenInfo(User user);
    }
}