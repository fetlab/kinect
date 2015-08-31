using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawTest
{
    class _3dWorld
    {
        double x_world;
        double y_world;
        double z_world;

        public _3dWorld(double x, double y, double z)
        {
            this.x_world = x;
            this.y_world = y;
            this.z_world = z;
        }

        public double getX()
        {
            return this.x_world;
        }

        public double getY()
        {
            return this.y_world;
        }

        public double getZ()
        {
            return this.z_world;
        }
    }
}
