using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProcessStateMachine
{
    public class StateDescriptor
    {
        private readonly Regex[] conditions;

        public List<StateDescriptor> Children { get; private set; }

        public string Name { get; private set; }

        public static IEnumerable<Regex> CreateConditions(params string[] regexPatterns)
        {
            return regexPatterns.Select(pattern => new Regex(pattern));
        }

        public StateDescriptor()
            :this("Root", Enumerable.Empty<Regex>())
        {
        }

        public StateDescriptor(string name, string regexPattern)
            :this(name, CreateConditions(regexPattern))
        {
        }

        public StateDescriptor(string name, IEnumerable<Regex> conditions)
        {
            Name = name;

            this.conditions = null != conditions ? conditions.ToArray() : new Regex[0];

            Children = new List<StateDescriptor>(5);
        }

        public StateDescriptor GetNextState(string line)
        {
            return Children.FirstOrDefault(chld => chld.conditions.Any(condition => condition.IsMatch(line)));
        }
    }
}
