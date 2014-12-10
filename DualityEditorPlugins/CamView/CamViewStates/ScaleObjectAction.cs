using System.Drawing;
using System.Windows.Forms;
using Duality.Editor.Plugins.CamView.Properties;
using OpenTK;

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
	public class ScaleObjectAction : IObjectAction
	{
		public void Update(CamViewState state, Point mouseLoc, Point beginLoc, Vector3 beginLocSpace, Vector3 lastLocSpace, Vector3 selectionCenter, float selectionRadius, CamViewState.LockedAxis lockedAxis)
		{
			state.ValidateSelectionStats();
			if (selectionRadius == 0.0f) return;

			Vector3 spaceCoord = state.GetSpaceCoord(new Vector3(mouseLoc.X, mouseLoc.Y, selectionCenter.Z));
			float lastRadius = selectionRadius;
			float curRadius = (selectionCenter - spaceCoord).Length;
			float scale = MathF.Clamp(curRadius / lastRadius, 0.0001f, 10000.0f);

			state.ScaleSelectionBy(scale);

			state.ActionLastLocSpace = spaceCoord;
			state.Invalidate();
		}

		public string GetStatusText()
		{
			return CamViewRes.CamView_Action_Scale;
		}

		public Cursor GetCursor()
		{
			return CursorHelper.ArrowActionScale;
		}
	}
}