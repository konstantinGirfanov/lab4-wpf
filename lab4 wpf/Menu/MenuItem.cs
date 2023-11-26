using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab4_wpf
{
    public class MenuItem
    {
        public string Caption;
        public string ClassFullName;

        public MenuItem(string caption, string tag)
        {
            Caption = caption;
            ClassFullName = tag;
        }
    }
}