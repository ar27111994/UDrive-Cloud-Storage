using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication5.Models
{
   public class FilesHash
    {
        [Required]
        [Key]
        public int id { get; set; }
        public string path { get; set; }
        public string HashCode { get; set; }
        public string  FolderName { get; set; }
        public ICollection<UserFiles> User { get; set; }
    }
}
