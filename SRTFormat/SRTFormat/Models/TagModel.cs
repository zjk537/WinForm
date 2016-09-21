using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTFormat.Models
{
    public class TagModel
    {
        public int Id { get; set; }

        public string TagName { get; set; }

        public bool TagStatus { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
