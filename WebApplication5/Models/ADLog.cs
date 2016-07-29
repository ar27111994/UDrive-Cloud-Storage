using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication5.Models
{
    public class ADLog
    {
       
        [Key]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
       
    }
}
