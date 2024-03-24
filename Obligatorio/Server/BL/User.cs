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
        private string _password;

        public string Name { get; set; }
        public List<int> Trip;

        public User()
        {
            this._id = 0; //generar un numero acorde a algo
        }
    }
}
