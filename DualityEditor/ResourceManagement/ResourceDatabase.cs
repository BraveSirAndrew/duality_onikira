using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Duality.Serialization;

namespace Duality.Editor.ResourceManagement
{
	[Serializable]
	public class ResourceDatabase
	{
		private readonly IResourceContentPathModifier _contentPathModifier;
		public const string DatabaseName = "ResourceDatabase.db";

		private List<KeyValuePair<string, string>> _references = new List<KeyValuePair<string, string>>();

		public ResourceDatabase(IResourceEventManagerWrapper wrapper, IResourceContentPathModifier contentPathModifier)
		{
			_contentPathModifier = contentPathModifier;
			AttachEvents(wrapper);
		}

		private void AttachEvents(IResourceEventManagerWrapper wrapper)
		{
			wrapper.ResourceCreated += OnResourceCreated;
			wrapper.ResourceModified += OnResourceModified;
			wrapper.ResourceDeleted += OnResourceDeleted;
			wrapper.ResourceRenamed += OnResourceRenamed;
			wrapper.ResourceSaved += OnResourceSaved;
		}

		public void Initialize()
		{
			if (!Directory.Exists(DualityApp.DataDirectory))
				return;
			if (File.Exists(DatabaseName) == false)
			{
				var resources = Resource.GetResourceFiles();
				foreach (var resource in resources)
				{
					OnResourceCreated(this, new ResourceEventArgs(resource));
				}

				Formatter.WriteObject(_references, DatabaseName, FormattingMethod.Xml);
			}
			else
			{
				_references = Formatter.ReadObject<List<KeyValuePair<string, string>>>(DatabaseName, FormattingMethod.Xml);
			}
		}

		public ResourceReferences GetResourceReferences(string resourcePath)
		{
			if (_references.All(x => x.Key != resourcePath))
				return null;
			var resources = _references.Where(x => x.Key == resourcePath).Select(x => x.Value).ToList();
			return new ResourceReferences { Path = resourcePath, References = resources };
		}

		private void OnResourceCreated(object sender, ResourceEventArgs e)
		{
			if (e.IsDirectory)
				return;
			var resources = _contentPathModifier.FindReferencedResources(e.Path);
			foreach (var resource in resources)
			{
				var res = new KeyValuePair<string, string>(e.Path, resource);
				if (!_references.Any(x => x.Key == res.Key && x.Value == res.Value))
					_references.Add(res);
			}
		}

		private void OnResourceModified(object sender, ResourceEventArgs e)
		{
			if (!e.IsDirectory)
				UpdateReferences(e.Path);
		}

		private void OnResourceSaved(object sender, ResourceSaveEventArgs e)
		{
			if (!e.IsDirectory)
				UpdateReferences(e.Path);
		}

		private void UpdateReferences(string path)
		{
			var referencesFound = _contentPathModifier.FindReferencedResources(path).Distinct();
			_references.RemoveAll(x => x.Key == path);
			_references.AddRange(referencesFound.Select(r => new KeyValuePair<string, string>(path, r)));
		}

		private void OnResourceDeleted(object sender, ResourceEventArgs e)
		{
			_references.RemoveAll(x => x.Key == e.Path);
			_references.RemoveAll(x => x.Value == e.Path);
		}

		private void OnResourceRenamed(object sender, ResourceRenamedEventArgs e)
		{
			var referencesKeys = _references.Where(x => x.Value == e.OldPath).Select(x => x.Key).Distinct();

			foreach (var resourcePath in referencesKeys)
			{
				if (DualityEditorApp.IsResourceUnsaved(resourcePath))
					continue;

				_contentPathModifier.UpdateContentPaths(e.Path, e.OldPath, resourcePath);
			}

			var references = _references.Where(x => x.Value == e.OldPath).ToArray();
			foreach (var reference in references)
			{
				_references[_references.IndexOf(reference)] = new KeyValuePair<string, string>(reference.Key, e.Path);
			}

			var referencesValues = _references.Where(x => x.Key == e.OldPath)
					.Select(x => x.Value).ToArray();

			foreach (var value in referencesValues)
			{
				_references.Add(new KeyValuePair<string, string>(e.Path, value));
			}
			_references.RemoveAll(x => x.Key == e.OldPath);
		}
	}
}
