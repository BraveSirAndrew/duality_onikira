using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Duality.Editor
{
	public class ResourceDatabase
	{
		private List<KeyValuePair<string, string>> _references = new List<KeyValuePair<string, string>>();

		public ResourceDatabase(IFileEventManagerWrapper wrapper)
		{
			wrapper.ResourceCreated += OnResourceCreated;
			wrapper.ResourceModified += OnResourceModified;
			wrapper.ResourceDeleted += OnResourceDeleted;
			wrapper.ResourceRenamed += OnResourceRenamed;
		}

		private void OnResourceCreated(object sender, ResourceEventArgs e)
		{
			var resources = FindReferencedResourcesInXml(e.Path);
			foreach (var resource in resources)
			{
				_references.Add(new KeyValuePair<string, string>(e.Path, resource));
			}
			//UpdateResourcesReferencingKey(resources, e.Path);
		}

		private void OnResourceModified(object sender, ResourceEventArgs e)
		{
			if (!_references.Any(x=> x.Key == e.Path))
			{
				throw new InvalidOperationException(
					String.Format("Resource database does not contain an entry for modified resource {0}", e.Path));
			}

			var referencesFound = FindReferencedResourcesInXml(e.Path);
			_references.RemoveAll(x => x.Key == e.Path);
			_references.AddRange(referencesFound.Select(r=> new KeyValuePair<string, string>(e.Path, r)));

			//UpdateResourcesReferencingKey(referencesFound, e.Path);
		}

//		private void UpdateResourcesReferencingKey(List<string> resources, string resourcePath)
//		{
//			foreach (var resource in resources)
//			{
//				HashSet<string> referencedResources;
//				if (!_resourcesReferencingKey.TryGetValue(resource, out referencedResources))
//				{
//					_resourcesReferencingKey.Add(resource, new HashSet<string>());
//				}
//				_resourcesReferencingKey[resource].Add(resourcePath);
//			}
//		}

		private void OnResourceDeleted(object sender, ResourceEventArgs e)
		{
			if (!_references.Any(x => x.Key == e.Path))
				throw new InvalidOperationException();
			_references.RemoveAll(x => x.Key == e.Path);
		}

		private void OnResourceRenamed(object sender, ResourceRenamedEventArgs e)
		{
			var referencesKeys = _references.Where(x => x.Value == e.OldPath).Select(x => x.Key);

			foreach (var resourcePath in referencesKeys)
			{
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
//			foreach (var resourcePath in references)
//			{
//				var xml = XDocument.Load(resourcePath);
//				var contentPathElements = xml.Descendants("contentPath").Where(x => x.Value == e.OldPath);
//				foreach (var element in contentPathElements)
//				{
//					element.Value = e.Path;
//				}
//				xml.Save(resourcePath);
//			}

			
//			var alreadyProcessed = new HashSet<string>();
//			foreach (var pair in _resourcesReferencedByKey)
//			{
//				foreach (var resourcePath in pair.Value)
//				{
//					if (alreadyProcessed.Contains(resourcePath))
//						continue;
//
//					foreach (var pair2 in _resourcesReferencedByKey)
//					{
//						if (resourcePath == pair2.Key)
//							continue;
//
//						foreach (var resourcePath2 in pair2.Value)
//						{
//							if (resourcePath != resourcePath2)
//								continue;
//
//							var xml = XDocument.Load(resourcePath);
//							var contentPathElements = xml.Descendants("contentPath").Where(x => x.Value == e.OldPath);
//							foreach (var element in contentPathElements)
//							{
//								element.Value = e.Path;
//							}
//							xml.Save(resourcePath);
//						}
//					}
//				}
//			}
		}

		private List<string> FindReferencedResourcesInXml(string path)
		{
			var xml = XDocument.Load(path);

			return xml.Descendants("contentPath").Select(x => x.Value).ToList();
		}

		public ResourceReferences GetResourceReferences(string resourcePath)
		{
			if(!_references.Any(x=> x.Key == resourcePath))
				return null;
			var resources = _references.Where(x=> x.Key == resourcePath).Select(x=> x.Value).ToList();
			return new ResourceReferences {Path = resourcePath, References = resources};
		}
	}

	public class ResourceDatabase2
	{
		/*This file's references: _resourcesReferencedByKey

			res1   		res2;res3;res1 
			res2
			res3		
			res4   		res2;res1 
			
		 
			Files referencing this file: _resourcesReferencingKey;
			res1		res1;res4
			res2		res1;res4
			res3		res1
			res4			
		 
		 */

		private Dictionary<string, List<string>> _resourcesReferencedByKey;
		private Dictionary<string, HashSet<string>> _resourcesReferencingKey;
		
		public ResourceDatabase2(IFileEventManagerWrapper wrapper)
		{
			wrapper.ResourceCreated += OnResourceCreated;
			wrapper.ResourceModified += OnResourceModified;
			wrapper.ResourceDeleted += OnResourceDeleted;
			wrapper.ResourceRenamed += OnResourceRenamed;

			_resourcesReferencedByKey = new Dictionary<string, List<string>>();
			_resourcesReferencingKey = new Dictionary<string, HashSet<string>>();
		}

		private void OnResourceCreated(object sender, ResourceEventArgs e)
		{
			var resources = FindReferencedResources(e.Path);
			_resourcesReferencedByKey.Add(e.Path, resources);
			UpdateResourcesReferencingKey(resources, e.Path);
		}

		private void OnResourceModified(object sender, ResourceEventArgs e)
		{
			List<string> resources;
			if (!_resourcesReferencedByKey.TryGetValue(e.Path, out resources))
			{
				throw new InvalidOperationException(
					String.Format("Resource database does not contain an entry for modified resource {0}", e.Path));
			}

			var referencesFound = FindReferencedResources(e.Path);
			_resourcesReferencedByKey[e.Path] = referencesFound;

			UpdateResourcesReferencingKey(referencesFound, e.Path);
		}

		private void UpdateResourcesReferencingKey(List<string> resources, string resourcePath)
		{
			foreach (var resource in resources)
			{
				HashSet<string> referencedResources;
				if (!_resourcesReferencingKey.TryGetValue(resource, out referencedResources))
				{
					_resourcesReferencingKey.Add(resource, new HashSet<string>());
				}
				_resourcesReferencingKey[resource].Add(resourcePath);
			}
		}

		private void OnResourceDeleted(object sender, ResourceEventArgs e)
		{
			if (_resourcesReferencedByKey.ContainsKey(e.Path) == false)
				throw new InvalidOperationException(String.Format("Resource database does not contain an entry for deleted resource {0}", e.Path));

			_resourcesReferencedByKey.Remove(e.Path);
		}

		private void OnResourceRenamed(object sender, ResourceRenamedEventArgs e)
		{
			var references = _resourcesReferencingKey[e.OldPath];
			foreach (var resourcePath in references)
			{
				var xml = XDocument.Load(resourcePath);
				var contentPathElements = xml.Descendants("contentPath").Where(x => x.Value == e.OldPath);
				foreach (var element in contentPathElements)
				{
					element.Value = e.Path;
				}
				xml.Save(resourcePath);
			}

//			var alreadyProcessed = new HashSet<string>();
//			foreach (var pair in _resourcesReferencedByKey)
//			{
//				foreach (var resourcePath in pair.Value)
//				{
//					if (alreadyProcessed.Contains(resourcePath))
//						continue;
//
//					foreach (var pair2 in _resourcesReferencedByKey)
//					{
//						if (resourcePath == pair2.Key)
//							continue;
//
//						foreach (var resourcePath2 in pair2.Value)
//						{
//							if (resourcePath != resourcePath2)
//								continue;
//
//							var xml = XDocument.Load(resourcePath);
//							var contentPathElements = xml.Descendants("contentPath").Where(x => x.Value == e.OldPath);
//							foreach (var element in contentPathElements)
//							{
//								element.Value = e.Path;
//							}
//							xml.Save(resourcePath);
//						}
//					}
//				}
//			}
		}

		private List<string> FindReferencedResources(string path)
		{
			var xml = XDocument.Load(path);

			return xml.Descendants("contentPath").Select(x => x.Value).ToList();
		}

		public ResourceReferences GetResourceReferences(string resourcePath)
		{
			var references = new List<string>();
			if (_resourcesReferencedByKey.TryGetValue(resourcePath, out references))
				return new ResourceReferences
				{
					Path = resourcePath,
					References = references
				};
			return null;
		}
	}
}
