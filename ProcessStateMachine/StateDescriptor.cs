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
        private static Predicate<string>[] zeroPrediacates = new Predicate<string>[0];

        private readonly Predicate<string>[] predicates;

        public static IEnumerable<Predicate<string>> CreateRegexPredicates(params string[] regexPatterns)
        {
            return regexPatterns.Select(pattern => new Predicate<string>(new Regex(pattern).IsMatch));
        }

        public static IEnumerable<Predicate<string>> CreateSubstringPredicates(params string[] substrings)
        {
            return substrings.Select(str => new Predicate<string>(inputString => inputString.Contains(str)));
        }

        public static StateDescriptor WithRegexPattern(string name, params string[] regexPatterns)
        {
            return new StateDescriptor(name, CreateRegexPredicates(regexPatterns));
        }

        public static StateDescriptor WithSubstringMatch(string name, params string[] substrings)
        {
            return new StateDescriptor(name, CreateSubstringPredicates(substrings));
        }

        public string Name { get; private set; }

        protected List<StateDescriptor> Children { get; private set; }

        public object Tag { get; set; }

        public StateDescriptor()
            :this("Root", null)
        {
        }

        public StateDescriptor(string name, IEnumerable<Predicate<string>> predicates)
        {
            this.predicates = null != predicates ? predicates.ToArray() : zeroPrediacates;

            Name = name;
            Children = new List<StateDescriptor>(3);
        }

        public void Add(StateDescriptor stateDescr)
        {
            if (!Children.Contains(stateDescr))
                Children.Add(stateDescr);
        }

        public StateDescriptor GetNextState(string line)
        {
            return Children.FirstOrDefault(c => c.predicates.Any(p => p(line)));
        }
    }
}
