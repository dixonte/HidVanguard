using System;
using System.Collections.Generic;
using System.Text;

namespace HidVanguard.Config.Extensions
{
    public static class ArrayExtender
    {
        public static T[] Append<T>(this T[] a1, T[] a2)
        {
            T[] dest = new T[a1.Length + a2.Length];
            a1.CopyTo(dest, 0);
            a2.CopyTo(dest, a1.Length);

            return dest;
        }
    }
}
