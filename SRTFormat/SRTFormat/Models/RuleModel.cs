using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTFormat.Models
{
    public class RuleModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string RegValue { get; set; }

        public String ReplaceValue { get; set; }


        public string Desc { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
