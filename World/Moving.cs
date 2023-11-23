using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Moving
    {
       public string moving {  get; set; }

        public Moving()
        { }
        public Moving(string movement) :base()
        {
            this.moving = movement;

        }
    }
}
