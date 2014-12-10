using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Duality.Editor.Plugins.CamView.Properties;
using OpenTK;

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
	internal class RectSelectObjectAction : IObjectAction
	{
		public void Update(CamViewState state, Point mouseLoc, Point beginLoc, Vector3 beginLocSpace, Vector3 lastLocSpace, Vector3 selectionCenter, float selectionRadius, CamViewState.LockedAxis lockedAxis)
		{
			if (DualityEditorApp.IsSelectionChanging) return; // Prevent Recursion in case SelectObjects triggers UpdateAction.

			bool shift = (Control.ModifierKeys & Keys.Shift) != Keys.None;
			bool ctrl = (Control.ModifierKeys & Keys.Control) != Keys.None;

			// Determine picked rect
			int pX = Math.Max(Math.Min(mouseLoc.X, beginLoc.X), 0);
			int pY = Math.Max(Math.Min(mouseLoc.Y, beginLoc.Y), 0);
			int pX2 = Math.Max(mouseLoc.X, beginLoc.X);
			int pY2 = Math.Max(mouseLoc.Y, beginLoc.Y);
			int pW = Math.Max(pX2 - pX, 1);
			int pH = Math.Max(pY2 - pY, 1);

			// Check which renderers are picked
			List<CamViewState.SelObj> picked = state.PickSelObjIn(pX, pY, pW, pH);

			// Store in internal rect selection
			ObjectSelection oldRectSel = state.ActiveRectSel;
			state.ActiveRectSel = new ObjectSelection(picked);

			// Apply internal selection to actual editor selection
			if (shift || ctrl)
			{
				if (state.ActiveRectSel.ObjectCount > 0)
				{
					ObjectSelection added = (state.ActiveRectSel - oldRectSel) + (oldRectSel - state.ActiveRectSel);
					state.SelectObjects(added.OfType<CamViewState.SelObj>(), shift ? SelectMode.Append : SelectMode.Toggle);
				}
			}
			else if (state.ActiveRectSel.ObjectCount > 0)
				state.SelectObjects(state.ActiveRectSel.OfType<CamViewState.SelObj>());
			else
				state.ClearSelection();

			state.Invalidate();
		}

		public string GetStatusText()
		{
			return CamViewRes.CamView_Action_Select_Active;
		}

		public Cursor GetCursor()
		{
			return CursorHelper.Arrow;
		}
	}
}