using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication5.Models
{
   public class operationsViewModel
    {
        
        public int operation { get; set; }
        public List<fileViewModel> files { get; set; }
        public Dictionary<string ,string> pathname { get; set; }
    }
}
