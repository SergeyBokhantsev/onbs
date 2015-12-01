﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public static class Ensure
    {
        public static T ArgumentIsNotNull<T>(T o)
            where T : class
        {
            if (o == null)
                throw new ArgumentNullException(string.Format("Required argument typeof {0} is null", typeof(T)));

            return o;
        }
    }
}