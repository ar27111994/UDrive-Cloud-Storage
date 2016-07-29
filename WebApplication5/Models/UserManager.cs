using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication5.Models
{
   public  class UserManager
    { 
        [Required]
        [Key]
        public string Email { get; set; }
        public double CurrentStorage { get; set; }
        public double MaxStorage { get; set; }
        public bool Sent { get; set; }
        public int LastLoginYear { get; set; }
        public int LastLoginMonth { get; set; }
        public bool LoginWarning { get; set; }
        public ICollection<UserFiles> Files { get; set; }

    }
}
