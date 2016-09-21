using SRTFormat.DBAccess;
using SRTFormat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTFormat.Business
{
    public class RuleBusiness
    {
        RuleDataAccess dataAccess = new RuleDataAccess();

        public void AddRule(RuleModel model)
        {
            dataAccess.AddRule(model);
        }

        public List<RuleModel> GetRules()
        {
            return dataAccess.GetRules();
        }
    }
}
