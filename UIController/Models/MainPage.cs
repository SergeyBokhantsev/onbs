using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIController.Models
{
    public class MainPage : IPageModel
    {
        public event EventHandler Disposing;

        public string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }
        
        public void Dispose()
        {
            var handler = Disposing;
            if (handler != null)
                handler(null, null);       
        }
    }
}
