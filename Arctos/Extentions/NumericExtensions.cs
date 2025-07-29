using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ursa.Extensions
{
    public static class NumericExtensions
    {
        public static double ToRadians(this double degrees)
        {
            return (Math.PI / 180) * degrees;
        }

        public static double ToDegrees(this double radians)
        {
            return (180 / Math.PI) * radians;
        }

        public static float ToRadians(this float degrees)
        {
            return (float)(Math.PI / 180.0) * degrees;
        }

        public static float ToDegrees(this float radians)
        {
            return (float)(180 / Math.PI) * radians;
        }

    }
}
