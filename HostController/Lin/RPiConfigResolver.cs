using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace HostController.Lin
{
    public class RPiConfigResolver : ConfigValuesResolver
    {
        private readonly IProcessRunnerFactory processRunnerFactory;
        private readonly Dictionary<string, object> cache = new Dictionary<string, object>();
        private readonly Dictionary<string, Func<string>> resolvers;

        public RPiConfigResolver(IProcessRunnerFactory processRunnerFactory)
        {
            this.processRunnerFactory = processRunnerFactory;

            resolvers = new Dictionary<string, Func<string>>
            {
                { ConfigNames.Placeholder_Elm327Port, GetElm327Port }
            };
        }

        protected override string GetValue(string placeholder)
        {
            if (resolvers.ContainsKey(placeholder))
                return resolvers[placeholder]();
            else
                return null;
        }

        private string GetElm327Port()
        {
            try
            {
                var pr = processRunnerFactory.Create("sudo", "dmesg", false);

                pr.Run();

                pr.WaitForExit(5000);

                var output = pr.GetFromStandardOutput();

                return output;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception in RPiConfigResolver.GetElm327Port resolver: {0}", ex.Message), ex);
            }
        }
    }
}
