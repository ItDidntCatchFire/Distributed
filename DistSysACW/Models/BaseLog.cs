using System;
using System.ComponentModel.DataAnnotations;

namespace DistSysACW.Models
{
    public abstract class BaseLog : IModel
    {
        [Key]
        public string LogId { get; set; }
        public string LogString { get; set; }
        public DateTime LogDateTime { get; set; }
    }
}