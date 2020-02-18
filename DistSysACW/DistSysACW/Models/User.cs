using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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



    #region Task13?
    // TODO: You may find it useful to add code here for Logging
    #endregion

    public static class UserDatabaseAccess
    {
        #region Task3 
        // TODO: Make methods which allow us to read from/write to the database 
        #endregion
    }


}