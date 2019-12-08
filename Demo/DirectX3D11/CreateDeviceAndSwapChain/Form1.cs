using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CreateDeviceAndSwapChain
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void ShowObjectWithFields(String name, Object obj)
        {
            String info = name + " {\r\n";
            foreach (var field in obj.GetType().GetFields())
            {
                info += String.Format("  {0}={1}\r\n", field.Name, field.GetValue(obj));
            }
            info += "}\r\n";
            mainText.Text += info;
        }
    }
}
