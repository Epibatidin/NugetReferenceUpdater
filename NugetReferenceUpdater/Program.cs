using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NugetReferenceUpdater
{
    class Program
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

        static void Main(string[] args)
        {
            // search _globalNuget.json
            string rootPath = @"D:\Develop\KleinZeug\NugetReferenceUpdater\NugetReferenceUpdater";
            rootPath = @"D:\Develop\FLUX";
            if (args.Length > 0)
                rootPath = args[0];
            
            System.Console.WriteLine("Path : " +  rootPath);
            var f = new FileInfo(rootPath + "\\globalNuget.json");
            if(!f.Exists)
                throw new FileNotFoundException("No globalNuget.json " + f.FullName);
            
            var json = JObject.Parse(f.OpenText().ReadToEnd());

            var dependencies = LoadDependencyNodesFromGlobal(json);

            var rootDir = new DirectoryInfo(rootPath);

            var projectJSons = rootDir.GetFiles("project.json", SearchOption.AllDirectories);
            
            foreach (var projectJSon in projectJSons)
            {
                try
                {
                    ExecuteOneProjectFile(projectJSon, dependencies);
                } 
                catch (Exception e)
                {
                    throw new Exception("Error in file " + projectJSon.FullName, e);
                }
            }
        }

        private static void ExecuteOneProjectFile(FileInfo projectJSon, IList<DependencyNode> dependencies)
        {
            string jsonContent = "";

            using (var reader = projectJSon.OpenText())
            {
                jsonContent = reader.ReadToEnd();
                reader.Close();
            }
            var jo = JObject.Parse(jsonContent);

            foreach (var dependency in dependencies)
            {
                var actualDependencyNode = jo.SelectToken(dependency.JsonPath);
                if (actualDependencyNode == null) continue;

                foreach (var reference in dependency.References)
                {
                    var actualDependencyValue = actualDependencyNode[reference.Key];

                    if (actualDependencyValue == null) continue;

                    var asValue = actualDependencyValue as JValue;
                    if (asValue == null) continue;

                    asValue.Value = reference.Value;
                }
            }

            var newContent = jo.ToString();

            using (var fs = new FileStream(projectJSon.FullName, FileMode.Create))
            using (var writer = new StreamWriter(fs))
            {
                writer.Write(newContent);

                writer.Flush();
                writer.Close();
            }
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
