using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Duality;
using Duality.Editor;
using Moq;
using NUnit.Framework;

namespace DualityEditor.Tests
{
	[TestFixture]
	public class ResourceDatabaseTests
	{
		private Mock<IFileEventManagerWrapper> _fileEventManager;

		public ResourceDatabase Db { get; set; }

		[SetUp]
		public void Setup()
		{
			_fileEventManager = new Mock<IFileEventManagerWrapper>();
			Db = CreateResourceDatabase();
		}

		[Test]
		public void When_resource_doesnt_exist_Then_GetResourceReferences_returns_null()
		{
			Assert.IsNull(Db.GetResourceReferences(""));
		}

		[Test]
		public void When_resource_created_Then_adds_resource_to_database()
		{
			var resource = GetTestResource();
			_fileEventManager.Raise(m => m.ResourceCreated += null, new ResourceEventArgs(resource.Path));

			Assert.AreEqual(resource.Path, Db.GetResourceReferences(resource.Path).Path);
		}

		[Test]
		public void When_a_resource_is_created_that_contains_references_Then_create_references_in_database()
		{
			var anotherTestResource = GetAnotherTestResource();
			var resource = GetTestResource();

			resource.AnotherTestResource = anotherTestResource;
			resource.Save();

			_fileEventManager.Raise(m => m.ResourceCreated += null, new ResourceEventArgs(resource.Path));
			_fileEventManager.Raise(m => m.ResourceCreated += null, new ResourceEventArgs(anotherTestResource.Path));

			var resourceReferences = Db.GetResourceReferences(resource.Path);

			Assert.IsTrue(resourceReferences.References.Contains(anotherTestResource.Path));
		}

		[Test]
		public void When_a_resource_with_references_is_saved_Then_resources_referenced_in_database()
		{
			var anotherTestResource = GetAnotherTestResource();
			var resource = GetTestResource();

			RaiseResourceCreatedEvent(resource.Path);
			RaiseResourceCreatedEvent(anotherTestResource.Path);

			resource.AnotherTestResource = anotherTestResource;
			resource.Save();
			
			RaiseResourceModifiedEvent(resource.Path);
			var resourceReferences = Db.GetResourceReferences(resource.Path);

			Assert.IsTrue(resourceReferences.References.Contains(anotherTestResource.Path));
		}

		[Test]
		public void When_a_reference_is_removed_and_the_resource_saved_Then_removes_the_reference_from_the_database()
		{
			var anotherTestResource = GetAnotherTestResource();
			var resource = GetTestResource();

			RaiseResourceCreatedEvent(resource.Path);
			RaiseResourceCreatedEvent(anotherTestResource.Path);

			resource.AnotherTestResource = anotherTestResource;
			resource.Save();

			RaiseResourceModifiedEvent(resource.Path);

			resource.AnotherTestResource = null;
			resource.Save();

			RaiseResourceModifiedEvent(resource.Path);
			var resourceReferences = Db.GetResourceReferences(resource.Path);

			Assert.IsFalse(resourceReferences.References.Contains(anotherTestResource.Path));
		}

		[Test]
		public void When_a_resource_is_deleted_Then_it_is_deleted_from_the_database()
		{
			var resource = GetTestResource();

			RaiseResourceCreatedEvent(resource.Path);
			_fileEventManager.Raise(m => m.ResourceDeleted += null, new ResourceEventArgs(resource.Path));

			var resourceReferences = Db.GetResourceReferences(resource.Path);
			Assert.IsNull(resourceReferences);
		}

		[Test]
		public void When_a_resource_is_deleted_that_doesnt_exist_in_the_database_Then_throw()
		{
			var resource = GetTestResource();

			Assert.Throws<InvalidOperationException>(() => _fileEventManager.Raise(m => m.ResourceDeleted += null, new ResourceEventArgs(resource.Path)));
		}

		[Test]
		public void When_a_resource_is_moved_Then_all_references_are_updated()
		{
			var resource = GetTestResource();
			var another = GetAnotherTestResource();
			
			resource.AnotherTestResource = another;
			resource.Save();

			RaiseResourceCreatedEvent(resource.Path);
			RaiseResourceCreatedEvent(another.Path);

			var newPath = @"test\another.res";
			_fileEventManager.Raise(m => m.ResourceRenamed += null, new ResourceRenamedEventArgs(newPath, another.Path));

//			another.Save(pathNew);
//			RaiseResourceCreatedEvent(pathNew);

			Assert.AreEqual(newPath, Db.GetResourceReferences(resource.Path).References.First());
		}

		[Test]
		public void When_a_resource_is_renamed_Then_all_references_are_updated()
		{
			Assert.Fail("This isn't over!");
		}

		[Test]
		public void When_resource_renamed_and_resource_not_in_database_Then_throw_exception()
		{
			Assert.Fail("Finish me");
		}

		[Test]
		public void When_resource_exists_on_create_Then_throws()
		{
			Assert.Fail("lasldasd");
		}

		private void RaiseResourceModifiedEvent(string path)
		{
			_fileEventManager.Raise(m => m.ResourceModified += null, new ResourceEventArgs(path));
		}

		private void RaiseResourceCreatedEvent(string path)
		{
			_fileEventManager.Raise(m => m.ResourceCreated += null, new ResourceEventArgs(path));
		}

		private static TestResource GetTestResource()
		{
			var resource = ResourceTestHelper.GetResource();
			resource.Save("resource.res");
			return resource;
		}

		private static AnotherTestResource GetAnotherTestResource()
		{
			var anotherTestResource = new AnotherTestResource();
			anotherTestResource.Save("another.res");
			return anotherTestResource;
		}

		private ResourceDatabase CreateResourceDatabase()
		{
			return new ResourceDatabase(_fileEventManager.Object);
		}

	}


	public class ResourceTestHelper
	{
		public static TestResource GetResource()
		{
			var tr = new TestResource();
			return tr;
		}

		
	}

	[Serializable]
	public class TestResource : Resource
	{
		public ContentRef<AnotherTestResource> AnotherTestResource { get; set; }
	}

	[Serializable]
	public class AnotherTestResource : Resource
	{
				
	}
}
