using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.BL
{
    public class User
    {
        private int _id;

        public string Name { get; set; }
        private string _password;
        public List<int> Trip;
    }
}
