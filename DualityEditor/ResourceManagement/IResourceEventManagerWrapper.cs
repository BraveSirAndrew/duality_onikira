using System;

namespace Duality.Editor.ResourceManagement
{
	public interface IResourceEventManagerWrapper
	{
		event EventHandler<ResourceEventArgs> ResourceCreated;
		event EventHandler<ResourceEventArgs> ResourceModified;
		event EventHandler<ResourceEventArgs> ResourceDeleted;
		event EventHandler<ResourceRenamedEventArgs> ResourceRenamed;

		event EventHandler<ResourceSaveEventArgs> ResourceSaved;
	}
}