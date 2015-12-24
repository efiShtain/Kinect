using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace PhysicsSimulator
{

    public class Moveable
    {
        enum Axis { X,Y,Z};

        DateTime _t0;
        float _x0, _y0, _z0;
        float _v0x, _v0y, _v0z;
        float _ax, _ay, _az;
        public Moveable(float x0, float y0, float z0,
                        float v0x, float v0y, float v0z, 
                        float ax, float ay, float az)
        {
            _x0 = x0;
            _y0 = y0;
            _z0 = z0;
            _v0x = v0x;
            _v0y = v0y;
            _v0z = v0z;
            _ax = ax;
            _ay = ay;
            _az = az;
        }

        public void Init()
        {
            _t0 = DateTime.Now;
        }

        public Point3D GetNextPosition()
        {
            Point3D point = new Point3D();
            double dt = (DateTime.Now-_t0).TotalSeconds;
            point.X = calculate(dt, Axis.X);
            point.Y = calculate(dt, Axis.Y);
            point.Z = calculate(dt, Axis.Z);
            return point;
        }

        float calculate(double dt, Axis axe)
        {
            double res = 0;
            switch (axe)
            {
                case Axis.X:
                    res = _x0 + _v0x * dt + 0.5 * _ax * dt * dt;
                    break;
                case Axis.Y:
                    res = _y0 + _v0y * dt + 0.5 * _ay * dt * dt;
                    break;
                case Axis.Z:
                    res = _z0 + _v0z * dt + 0.5 * _az * dt * dt;
                    break;
            }
            
            return (float)res;
        }
    }
}
