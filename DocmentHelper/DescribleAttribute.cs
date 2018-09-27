using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocmentHelper
{
    public class DescribleAttribute: Attribute
    {
        public string Name { get; set; }
        public string Describle { get; set; }

        public DescribleAttribute(string name)
        {
            Name = name;
        }

        public DescribleAttribute(string name,string describle)
        {
            Name = name;
            Describle = describle;
        }
    }
}
