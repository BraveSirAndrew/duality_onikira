using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Duality.Serialization;

namespace Duality.Editor
{
	[Serializable]
	public class ResourceDatabase
	{
		private const string DatabaseName = "ResourceDatabase.db";

		private List<KeyValuePair<string, string>> _references = new List<KeyValuePair<string, string>>();

		public ResourceDatabase(IFileEventManagerWrapper wrapper)
		{
			AttachEvents(wrapper);
		}

		private void AttachEvents(IFileEventManagerWrapper wrapper)
		{
			wrapper.ResourceCreated += OnResourceCreated;
			wrapper.ResourceModified += OnResourceModified;
			wrapper.ResourceDeleted += OnResourceDeleted;
			wrapper.ResourceRenamed += OnResourceRenamed;
			wrapper.ResourceSaved += OnResourceSaved;
		}

		public void Initialize()
		{
			if (File.Exists(DatabaseName) == false)
			{
				var resources = Resource.GetResourceFiles();
				foreach (var resource in resources)
				{
					OnResourceCreated(this, new ResourceEventArgs(resource));
				}

				Formatter.WriteObject(_references, DatabaseName, FormattingMethod.Binary);
			}
			else
			{
				_references = Formatter.ReadObject<List<KeyValuePair<string, string>>>(DatabaseName, FormattingMethod.Binary);
			}
		}

		public ResourceReferences GetResourceReferences(string resourcePath)
		{
			if(!_references.Any(x=> x.Key == resourcePath))
				return null;
			var resources = _references.Where(x=> x.Key == resourcePath).Select(x=> x.Value).ToList();
			return new ResourceReferences {Path = resourcePath, References = resources};
		}

		private void OnResourceCreated(object sender, ResourceEventArgs e)
		{
			var resources = FindReferencedResourcesInXml(e.Path);
			foreach (var resource in resources)
			{
				_references.Add(new KeyValuePair<string, string>(e.Path, resource));
			}
		}

		private void OnResourceModified(object sender, ResourceEventArgs e)
		{
			var referencesFound = FindReferencedResourcesInXml(e.Path);
			_references.RemoveAll(x => x.Key == e.Path);
			_references.AddRange(referencesFound.Select(r=> new KeyValuePair<string, string>(e.Path, r)));
		}

		private void OnResourceSaved(object sender, ResourceSaveEventArgs e)
		{
			var referencesFound = FindReferencedResourcesInXml(e.Path);
			_references.RemoveAll(x => x.Key == e.Path);
			_references.AddRange(referencesFound.Select(r => new KeyValuePair<string, string>(e.Path, r)));
		}

		private void OnResourceDeleted(object sender, ResourceEventArgs e)
		{
			_references.RemoveAll(x => x.Key == e.Path);
		}

		private void OnResourceRenamed(object sender, ResourceRenamedEventArgs e)
		{
			var referencesKeys = _references.Where(x => x.Value == e.OldPath).Select(x => x.Key);
			
			foreach (var resourcePath in referencesKeys)
			{
				if (DualityEditorApp.IsResourceUnsaved(resourcePath))
					continue;

				var xml = XDocument.Load(resourcePath);
				var contentPathElements = xml.Descendants("contentPath").Where(x => x.Value == e.OldPath);
				foreach (var element in contentPathElements)
				{
					element.Value = e.Path;
				}
				xml.Save(resourcePath);
			}

			var references = _references.Where(x => x.Value == e.OldPath).ToArray();
			foreach (var reference in references)
			{
				_references[_references.IndexOf(reference)] = new KeyValuePair<string, string>(reference.Key, e.Path);
			}
		}

		private List<string> FindReferencedResourcesInXml(string path)
		{
			var xml = XDocument.Load(path);

			return xml.Descendants("contentPath").Select(x => x.Value).ToList();
		}
	}
}
