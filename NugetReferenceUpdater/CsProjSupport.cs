using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace NugetReferenceUpdater
{
    public class CsProjSupport
    {
        public void ExecuteForDir(DirectoryInfo rootFolder, IList<DependencyNode> dependencies)
        {
            var projectJSons = rootFolder.GetFiles("*.csproj", SearchOption.AllDirectories);

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

        private void ExecuteOneProjectFile(FileInfo csproj, IList<DependencyNode> dependencies)
        {
            var fileStream = csproj.Open(FileMode.Open, FileAccess.Read);
            var memStream = new MemoryStream();
            fileStream.CopyTo(memStream);

            fileStream.Flush();
            fileStream.Close();
            memStream.Seek(0, SeekOrigin.Begin);
            

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(memStream);

            foreach (var dependency in dependencies.SelectMany(c => c.References))
            {
                var nodes = xmlDocument.SelectNodes($"//PackageReference[@Include='{dependency.Key}']");
                if (nodes.Count == 0) continue;
                var activeNode = nodes[0];
                if (nodes.Count > 1)
                {
                    for (int i = 1; i < nodes.Count; i++)
                    {
                        var parent = nodes[i].ParentNode;
                        parent.RemoveChild(nodes[i]);
                    }
                }
                activeNode.Attributes["Version"].Value = dependency.Value;
            }

            xmlDocument.Save(csproj.FullName);
        }
    }
}
