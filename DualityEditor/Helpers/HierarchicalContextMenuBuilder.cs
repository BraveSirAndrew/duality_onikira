using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Duality.Editor.Helpers
{
	public static class HierarchicalContextMenuBuilder
	{
		public static void CreateHierarchicalContextMenuItemsFromTypes(ToolStripMenuItem parentMenuItem, 
			IEnumerable<Type> types, 
			Func<Type, Image> getNodeImageFunc, 
			ToolStripItemClickedEventHandler clickEventHandler,
			IEditorCategoryProvider editorCategoryProvider,
			Func<Type, string> getNameFunc = null)
		{
			ResetMenuToOriginalState(parentMenuItem);

			List<ToolStripItem> newItems = new List<ToolStripItem>();
			foreach (Type type in types)
			{
				// Generate category item
				string[] category = editorCategoryProvider.GetCategory(type);
				ToolStripMenuItem categoryItem = parentMenuItem;
				for (int i = 0; i < category.Length; i++)
				{
					ToolStripMenuItem subCatItem;
					if (categoryItem == parentMenuItem)
						subCatItem = newItems.FirstOrDefault(item => item.Name == category[i]) as ToolStripMenuItem;
					else
						subCatItem = categoryItem.DropDownItems.Find(category[i], false).FirstOrDefault() as ToolStripMenuItem;

					if (subCatItem == null)
					{
						subCatItem = new ToolStripMenuItem(category[i])
						{
							Name = category[i], 
							Tag = type.Assembly
						};
						subCatItem.DropDownItemClicked += clickEventHandler;
						if (categoryItem == parentMenuItem)
							InsertToolStripTypeItem(newItems, subCatItem);
						else
							InsertToolStripTypeItem(categoryItem.DropDownItems, subCatItem);
					}
					categoryItem = subCatItem;
				}

				var image = getNodeImageFunc != null ? getNodeImageFunc(type) : null;

				ToolStripMenuItem typeItem = new ToolStripMenuItem(getNameFunc != null ? getNameFunc(type) : type.Name, image);
				typeItem.Tag = type;
				if (categoryItem == parentMenuItem)
					InsertToolStripTypeItem(newItems, typeItem);
				else
					InsertToolStripTypeItem(categoryItem.DropDownItems, typeItem);
			}

			parentMenuItem.DropDownItems.AddRange(newItems.ToArray());
		}

		private static void ResetMenuToOriginalState(ToolStripMenuItem parentMenuItem)
		{
			List<ToolStripItem> oldItems = new List<ToolStripItem>(parentMenuItem.DropDownItems.OfType<ToolStripItem>());
			parentMenuItem.DropDownItems.Clear();
			foreach (ToolStripItem item in oldItems.Skip(2)) item.Dispose();
			parentMenuItem.DropDownItems.AddRange(oldItems.Take(2).ToArray());
		}

		private static void InsertToolStripTypeItem(System.Collections.IList items, ToolStripItem newItem)
		{
			ToolStripItem item2 = newItem;
			ToolStripMenuItem menuItem2 = item2 as ToolStripMenuItem;
			for (int i = 0; i < items.Count; i++)
			{
				ToolStripItem item1 = items[i] as ToolStripItem;
				ToolStripMenuItem menuItem1 = item1 as ToolStripMenuItem;
				if (item1 == null)
					continue;

				bool item1IsType = item1.Tag is Type;
				bool item2IsType = item2.Tag is Type;
				System.Reflection.Assembly assembly1 = item1.Tag is Type ? (item1.Tag as Type).Assembly : item1.Tag as System.Reflection.Assembly;
				System.Reflection.Assembly assembly2 = item2.Tag is Type ? (item2.Tag as Type).Assembly : item2.Tag as System.Reflection.Assembly;
				int result = 
					(assembly2 == typeof(DualityApp).Assembly ? 1 : 0) - 
					(assembly1 == typeof(DualityApp).Assembly ? 1 : 0);
				if (result > 0)
				{
					items.Insert(i, newItem);
					return;
				}
				else if (result != 0) continue;

				result = 
					(item2IsType ? 1 : 0) - 
					(item1IsType ? 1 : 0);
				if (result > 0)
				{
					items.Insert(i, newItem);
					return;
				}
				else if (result != 0) continue;

				result = string.Compare(item1.Text, item2.Text);
				if (result > 0)
				{
					items.Insert(i, newItem);
					return;
				}
				else if (result != 0) continue;
			}

			items.Add(newItem);
		}
	}
}