using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Reflector
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private List<ClassModel> GetClasses(string dllPath)
        {
            if (string.IsNullOrEmpty(dllPath))
            {
                MessageBox.Show("请选择文件");
            }
            List<ClassModel> modelList = new List<ClassModel>();
            Assembly assembly = Assembly.LoadFile(this.txtPath.Text);
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.IsEnum)
                {
                    string typeDesc = "";
                    var typeAttrs = type.GetCustomAttributes(false);
                    foreach (Attribute attr in typeAttrs)
                    {
                        if (attr is DescriptionAttribute)
                        {
                            typeDesc = (attr as DescriptionAttribute).Description;
                        }

                    }

                    string[] enumNames = type.GetEnumNames();
                    foreach(string name in enumNames){
                        FieldInfo fi = type.GetField(name);
                        ClassModel model = new ClassModel();
                        model.ClassType = "enum";
                        model.ClassName = type.Name;
                        model.ClassDesc = typeDesc;
                        model.ClassProp = name;
                        model.PropValue = Convert.ToInt32(fi.GetValue(null));
                        var attrs = fi.GetCustomAttributes(false);
                        foreach (Attribute attr in attrs)
                        {
                            if (attr is DescriptionAttribute)
                            {
                                model.PropDesc = (attr as DescriptionAttribute).Description;
                            }
                            
                        }

                        modelList.Add(model);
                    }
                }
            }

            return modelList;
        }

        private void btnBrown_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //ofd.InitialDirectory = "C:\\";
            ofd.Filter = "dll(*.dll)|*.dll";
            //ofd.RestoreDirectory = true;
            ofd.FilterIndex = 1;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = ofd.FileName;
                List<ClassModel> modelList = GetClasses(ofd.FileName);
                this.dataGridView1.DataSource = modelList;
                this.dataGridView1.Refresh();
            }
        }
    }

    public class ClassModel
    {
        //public String NameSpace { get; set; }
        public String ClassType { get; set; }

        public String ClassName { get; set; }
        public string ClassDesc { get; set; }
        public String ClassProp { get; set; }

        public int PropValue { get; set; }
        public string PropDesc { get; set; }
    }
}
