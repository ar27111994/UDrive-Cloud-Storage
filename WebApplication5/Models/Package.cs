using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication5.Models
{
    public class Package
    {
        [Key]
        public int PackageId { get; set; }
        [Required]
        public string PackageName { get; set; }
        [Required]
        public string OnlineStorage { get; set; }
        [Required]
        public string MoneyBack { get; set; }
        [Required]
        public float Price { get; set; }

    }
}
