using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Duality.Editor.ResourceManagement
{
	public class XmlResourceContentPathModifier : IResourceContentPathModifier
	{
		public void UpdateContentPaths(string newPath, string oldPath, string resourcePath)
		{
			var xml = XDocument.Load(resourcePath);
			var contentPathElements = xml.Descendants("contentPath").Where(x => x.Value == oldPath);
			foreach (var element in contentPathElements)
			{
				element.Value = newPath;
			}
			xml.Save(resourcePath);
		}

		public List<string> FindReferencedResources(string path)
		{
			var xml = XDocument.Load(path);

			return xml.Descendants("contentPath").Select(x => x.Value).ToList();
		}
	}
}