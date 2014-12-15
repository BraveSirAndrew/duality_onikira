using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Duality;
using Duality.Editor;
using Duality.Editor.ResourceManagement;
using Moq;
using NUnit.Framework;

namespace DualityEditor.Tests
{
	[TestFixture]
	public class ResourceDatabaseTests
	{
		private Mock<IResourceEventManagerWrapper> _fileEventManager;
		
		public ResourceDatabase Db { get; set; }

		[SetUp]
		public void Setup()
		{
			_fileEventManager = new Mock<IResourceEventManagerWrapper>();
			Db = CreateResourceDatabase();
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			CleanupFiles();
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
		public void When_a_resource_is_deleted_that_doesnt_exist_in_the_database_Then_does_not_throw()
		{
			var resource = GetTestResource();

			Assert.DoesNotThrow(() => _fileEventManager.Raise(m => m.ResourceDeleted += null, new ResourceEventArgs(resource.Path)));
		}

		[Test]
		public void When_a_resource_is_renamed_Then_all_references_are_updated()
		{
			var resource = GetTestResource();
			var another = GetAnotherTestResource();
			
			resource.AnotherTestResource = another;
			resource.Save();

			RaiseResourceCreatedEvent(resource.Path);
			RaiseResourceCreatedEvent(another.Path);

			var newPath = @"test\another.res";
			_fileEventManager.Raise(m => m.ResourceRenamed += null, new ResourceRenamedEventArgs(newPath, another.Path));

			Assert.AreEqual(newPath, Db.GetResourceReferences(resource.Path).References.First());
		}

		[Test]
		public void When_an_unsaved_resource_is_changed_Then_do_not_save()
		{
			var contentPathModifierMock = CreateMockPathModifier();
			Db = new ResourceDatabase(_fileEventManager.Object, contentPathModifierMock.Object);
			var resource = GetTestResource();
			var another = GetAnotherTestResource();

			resource.AnotherTestResource = another;
			resource.Save();

			RaiseResourceCreatedEvent(resource.Path);
			RaiseResourceCreatedEvent(another.Path);

			DualityEditorApp.FlagResourceUnsaved(resource);

			contentPathModifierMock.Setup(x => x.FindReferencedResources(It.IsAny<string>())).Returns(()=> new List<string> { "yetAnotherResource.res" });
			var yetAnotherResource = new ContentRef<AnotherTestResource>(new AnotherTestResource());
			yetAnotherResource.Res.Save("yetAnotherResource.res");
			resource.AnotherTestResource = yetAnotherResource;

			RaiseResourceModifiedEvent(resource.Path);

			_fileEventManager.Raise(m => m.ResourceRenamed += null, new ResourceRenamedEventArgs("test\\yetAnotherResource.res", "yetAnotherResource.res"));

			contentPathModifierMock.Verify(x => x.UpdateContentPaths(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),Times.Never);
		}

		[Test]
		public void When_resource_is_directory_on_create_Then_do_not_add()
		{
			RaiseResourceCreatedEvent(Environment.CurrentDirectory);
			
			Assert.IsNull(Db.GetResourceReferences(Environment.CurrentDirectory));
		}

		[Test]
		public void When_resource_is_directory_on_modify_Then_do_not_add()
		{
			RaiseResourceModifiedEvent(Environment.CurrentDirectory);

			Assert.IsNull(Db.GetResourceReferences(Environment.CurrentDirectory));
		}

		private Mock<IResourceContentPathModifier> CreateMockPathModifier()
		{
			var contentPathModifier = new Mock<IResourceContentPathModifier>();
			contentPathModifier.Setup(x => x.FindReferencedResources(It.IsAny<string>())).Returns(new List<string>());
			return contentPathModifier;
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
			var db = new ResourceDatabase(_fileEventManager.Object, new XmlResourceContentPathModifier());
			
			Directory.CreateDirectory(DualityApp.DataDirectory);
			CleanupFiles();
			db.Initialize();
			return db;
		}

		private void CleanupFiles()
		{
			if (File.Exists(ResourceDatabase.DatabaseName))
				File.Delete(ResourceDatabase.DatabaseName);
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
