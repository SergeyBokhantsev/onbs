﻿using Interfaces;
using Interfaces.UI;
using System;

namespace UIModels
{
    public class UnexpectedErrorModel : MultilineModel
    {
        public UnexpectedErrorModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            SetProperty(ModelNames.PageTitle, "Unexpected error occured");
        }
    }
}
