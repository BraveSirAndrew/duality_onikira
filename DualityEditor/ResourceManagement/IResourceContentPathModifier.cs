using System.Collections.Generic;

namespace Duality.Editor.ResourceManagement
{
	public interface IResourceContentPathModifier
	{
		void UpdateContentPaths(string newPath, string oldPath, string resourcePath);
		List<string> FindReferencedResources(string path);
	}
}