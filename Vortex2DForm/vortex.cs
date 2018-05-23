using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vortex2DForm
{
    class vortex
    {
        public double x;
        public double y;
        public double vort;

        public vortex() { }

        public vortex(double x, double y, double vort)
        {
            this.x = x;
            this.y = y;
            this.vort = vort;
        }
    }
}
