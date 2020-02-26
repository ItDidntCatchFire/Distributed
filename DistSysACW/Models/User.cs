using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DistSysACW.Models
{
    public class User : IModel
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

        public User() { }
        #endregion
    }
}
