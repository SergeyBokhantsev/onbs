﻿using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationController
{
    internal interface IAutomationToolAdapter
    {
        void Key(params AutomationKeys[] key);
    }
}
