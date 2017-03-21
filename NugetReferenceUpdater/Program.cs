using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NugetReferenceUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            // search _globalNuget.json
            string rootPath = @"D:\Develop\KleinZeug\NugetReferenceUpdater\NugetReferenceUpdater\Testing";
            //rootPath = @"D:\Develop\FLUX";
            if (args.Length > 0)
                rootPath = args[0];
            
            System.Console.WriteLine("Path : " +  rootPath);
            var f = new FileInfo(rootPath + "\\globalNuget.json");
            if(!f.Exists)
                throw new FileNotFoundException("No globalNuget.json " + f.FullName);
            
            var json = JObject.Parse(f.OpenText().ReadToEnd());

            var dependencies = LoadDependencyNodesFromGlobal(json);

            var rootDir = new DirectoryInfo(rootPath);

            //new ProjectJsonSupport().ExecuteForDir(rootDir, dependencies);

            new CsProjSupport().ExecuteForDir(rootDir, dependencies);
        }

        

        private static IList<DependencyNode> LoadDependencyNodesFromGlobal(JObject json)
        {
            var tokens = json.SelectTokens("$..dependencies");

            var dependencies = new List<DependencyNode>();           
            foreach (var token in tokens)
            {
                var dependency = new DependencyNode();
                dependency.JsonPath = token.Path;
                 
                foreach (var references in token.Children())
                {
                    var asProperty = references as JProperty;

                    var value = asProperty.Value;
                    if (value.Type == JTokenType.Object)
                    {
                        var versionProp = value.SelectToken("$..version");
                        dependency.References.Add(asProperty.Name + ".version", versionProp.Value<string>());
                    }
                    else if (value.Type == JTokenType.String)
                    {
                        dependency.References.Add(asProperty.Name, value.Value<string>());
                    }
                    else
                        throw new NotSupportedException("Unexpected Token Type");
                                            
                    
                }
                dependencies.Add(dependency);
            }
            return dependencies;
        } 

    }
}
