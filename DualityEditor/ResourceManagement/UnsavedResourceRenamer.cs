using System.Collections.Generic;
using System.Linq;

namespace Duality.Editor.ResourceManagement
{
	public class UnsavedResourceRenamer
	{
		private List<string> _unsavedResources = new List<string>();

		public UnsavedResourceRenamer()
		{
			DualityEditorApp.ObjectPropertyChanged += DualityEditorAppOnObjectPropertyChanged;
			FileEventManager.ResourceRenamed += OnResourceRenamed;
			Resource.ResourceSaved += OnResourceSaved;
		}

		private void OnResourceRenamed(object sender, ResourceRenamedEventArgs e)
		{
			foreach (var contentRef in _unsavedResources.Select(ContentProvider.RequestContent))
			{
				ReflectionHelper.VisitObjectsDeep<IContentRef>(contentRef.Res, r =>
				{
					if (r.IsDefaultContent) return r;
					if (r.IsExplicitNull) return r;
					if (string.IsNullOrEmpty(r.Path)) return r;

					if (e.IsResource && r.Path == e.OldPath)
					{
						r.Path = e.Path;
					}
					else if (e.IsDirectory && PathHelper.IsPathLocatedIn(r.Path, e.OldPath))
					{
						r.Path = r.Path.Replace(e.OldPath, e.Path);
					}
					return r;
				});
			}
		}

		private void OnResourceSaved(object sender, ResourceSaveEventArgs resourceSaveEventArgs)
		{
			if (_unsavedResources.Contains(resourceSaveEventArgs.Path))
				_unsavedResources.Remove(resourceSaveEventArgs.Path);
		}

		private void DualityEditorAppOnObjectPropertyChanged(object sender, ObjectPropertyChangedEventArgs eventArgs)
		{
			if (eventArgs.Objects.ResourceCount <= 0) 
				return;

			foreach (var resource in eventArgs.Objects.Resources)
			{
				if (_unsavedResources.Contains(resource.Path))
					continue;

				_unsavedResources.Add(resource.Path);
			}
		}
	}
}