using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication5.Models
{
    public class General
    {
        [Key]
        public int id { get; set; }
        public string PathForUser { get; set; }
        public string PathInDb { get; set; }
        public string Folder { get; set; }
    }
}
