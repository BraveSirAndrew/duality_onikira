using System;
using System.Linq.Expressions;
using System.Reflection;
using Duality.Components;
using Duality.Components.Renderers;
using Duality.Resources;
using NUnit.Framework;

namespace Duality.Tests.Resources
{
	[TestFixture]
	public class PrefabTests
	{
		[Test]
		public void PrefabChangesWithTheSamePropertyNameAreSaved()
		{
			var gameObj = this.CreateSimpleGameObject();
			var prefab = new Prefab(gameObj);
			gameObj.LinkToPrefab(prefab);

			var transform = gameObj.GetComponent<Transform>();
			var spriteRenderer = gameObj.GetComponent<SpriteRenderer>();
			gameObj.PrefabLink.PushChange(transform, PropertyOf(() => transform.ActiveSingle), false);
			gameObj.PrefabLink.PushChange(spriteRenderer, PropertyOf(() => spriteRenderer.ActiveSingle), false);

			gameObj.PrefabLink.ApplyChanges();

			Assert.IsFalse(gameObj.GetComponent<SpriteRenderer>().ActiveSingle);
			Assert.IsFalse(gameObj.GetComponent<Transform>().ActiveSingle);
		}

		private GameObject CreateSimpleGameObject(GameObject parent = null)
		{
			var obj = new GameObject("SimpleObject", parent);
			obj.AddComponent<Transform>();
			obj.AddComponent<SpriteRenderer>();
			return obj;
		}

		private static PropertyInfo PropertyOf<T>(Expression<Func<T>> expression)
		{
			var body = (MemberExpression)expression.Body;
			return (PropertyInfo)body.Member;
		}
	}
}