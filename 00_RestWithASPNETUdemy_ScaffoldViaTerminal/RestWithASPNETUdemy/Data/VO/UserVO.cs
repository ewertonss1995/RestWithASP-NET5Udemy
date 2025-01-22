using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RestWithASPNETUdemy.Model.Base;

namespace RestWithASPNETUdemy.Data.VO
{
    public class UserVO : BaseEntity
    {
        //public long Id { get; set; }
        public string UserName { get; set; }
        //public string FullName { get; set; }
        public string Password { get; set; }
        //public string RefreshToken { get; set; }
        //public DateTime RefreshTokenExpiryTime { get; set; }
    }
}