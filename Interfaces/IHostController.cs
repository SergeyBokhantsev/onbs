﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IHostController
    {
        ILogger Logger { get; }
        T GetController<T>() where T : IController;
    }
}
