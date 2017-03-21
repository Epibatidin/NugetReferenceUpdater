using System.Collections.Generic;

namespace NugetReferenceUpdater
{
    public class DependencyNode
    {
        public DependencyNode()
        {
            References = new Dictionary<string, string>();
        }

        public string JsonPath { get; set; }

        public IDictionary<string, string> References { get; }
    }
}
