using System;

namespace Duality.Editor
{
	public class FileEventManagerWrapper : IFileEventManagerWrapper
	{
		public event EventHandler<ResourceEventArgs> ResourceCreated;
		public event EventHandler<ResourceEventArgs> ResourceModified;
		public event EventHandler<ResourceEventArgs> ResourceDeleted;
		public event EventHandler<ResourceRenamedEventArgs> ResourceRenamed;
		public event EventHandler<ResourceSaveEventArgs> ResourceSaved;

		public FileEventManagerWrapper()
		{
			FileEventManager.ResourceCreated += OnResourceCreated;
			FileEventManager.ResourceModified += OnResourceModified;
			FileEventManager.ResourceRenamed += OnResourceRenamed;
			FileEventManager.ResourceDeleted += OnResourceDeleted;

			Resource.ResourceSaved += OnResourceSaved;
		}

		private void OnResourceSaved(object sender, ResourceSaveEventArgs resourceSaveEventArgs)
		{
			var handler = ResourceSaved;
			if (handler != null)
				handler(sender, resourceSaveEventArgs);	
		}

		private void OnResourceCreated(object sender, ResourceEventArgs resourceEventArgs)
		{
			var handler = ResourceCreated;
			if (handler != null)
				handler(sender, resourceEventArgs);	
		}

		private void OnResourceDeleted(object sender, ResourceEventArgs resourceEventArgs)
		{
			var handler = ResourceDeleted;
			if (handler != null)
				handler(sender, resourceEventArgs);
		}

		private void OnResourceModified(object sender, ResourceEventArgs resourceEventArgs)
		{
			var handler = ResourceModified;
			if (handler != null)
				handler(sender, resourceEventArgs);
		}

		private void OnResourceRenamed(object sender, ResourceRenamedEventArgs resourceRenamedEventArgs)
		{
			var handler = ResourceRenamed;
			if (handler != null) 
				handler(sender, resourceRenamedEventArgs);	
		}
	}
}