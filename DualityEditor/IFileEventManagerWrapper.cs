using System;

namespace Duality.Editor
{
	public interface IFileEventManagerWrapper
	{
		event EventHandler<ResourceEventArgs> ResourceCreated;
		event EventHandler<ResourceEventArgs> ResourceModified;
		event EventHandler<ResourceEventArgs> ResourceDeleted;
		event EventHandler<ResourceRenamedEventArgs> ResourceRenamed;
	}
}