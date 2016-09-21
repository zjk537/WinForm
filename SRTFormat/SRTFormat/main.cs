using SRTFormat.Business;
using SRTFormat.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SRTFormat
{
    public partial class main : Form
    {
        public main()
        {
            InitializeComponent();
        }

        private void main_Load(object sender, EventArgs e)
        {
        }

        private void init()
        {
            RuleBusiness ruleBusiness = new RuleBusiness();


        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            RuleModel model = new RuleModel();
            model.Name = this.txtName.Text.Trim();
            model.RegValue = this.txtRegValue.Text.Trim();
            model.ReplaceValue = this.txtReplaceValue.Text.Trim();
            model.Desc = this.txtDesc.Text.Trim();

            if (string.IsNullOrEmpty(model.Name))
            {

            }
        }

        
    }
}
