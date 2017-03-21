using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace NugetReferenceUpdater
{
    public class ProjectJsonSupport
    {
        public void ExecuteForDir(DirectoryInfo rootFolder, IList<DependencyNode> dependencies)
        {
            var projectJSons = rootFolder.GetFiles("project.json", SearchOption.AllDirectories);

            for (int i = 0; i < projectJSons.Length; i++)
            {
                var projectJSon = projectJSons[i];
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

        private void ExecuteOneProjectFile(FileInfo projectJSon, IList<DependencyNode> dependencies)
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
    }
}

