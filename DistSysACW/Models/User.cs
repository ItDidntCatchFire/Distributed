using System.ComponentModel.DataAnnotations;

namespace DistSysACW.Models
{
    public class User
    {
        #region Task2
        public enum Roles
        {
            User,
            Admin
        }

        [Key]
        public string ApiKey { get; set; }
        public string UserName { get; set; }
        public Roles Role { get; set; }

        public User()
        {

        }

        #endregion

    }
}
