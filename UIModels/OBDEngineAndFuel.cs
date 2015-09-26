using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels
{
    public class OBDEngineAndFuel : CommonPageBase
    {
        public OBDEngineAndFuel(IHostController hc)
            :base(hc, typeof(OBDEngineAndFuel).Name)
        {
        }
    }
}
