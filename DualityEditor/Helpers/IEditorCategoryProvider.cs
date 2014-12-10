using System;

namespace Duality.Editor.Helpers
{
	public interface IEditorCategoryProvider
	{
		string[] GetCategory(Type type);
	}

	public class DefaultEditorCategoryProvider : IEditorCategoryProvider
	{
		public string[] GetCategory(Type type)
		{
			return type.GetEditorCategory();
		}
	}
}