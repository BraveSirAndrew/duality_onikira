using System.Drawing;
using System.Windows.Forms;
using Duality.Editor.Plugins.CamView.Properties;
using OpenTK;

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
	public class RotateObjectAction : IObjectAction
	{
		public string GetStatusText()
		{
			return CamViewRes.CamView_Action_Rotate;
		}

		public void Update(CamViewState state, Point mouseLoc, Point beginLoc, Vector3 beginLocSpace, Vector3 lastLocSpace, Vector3 selectionCenter, float selectionRadius, CamViewState.LockedAxis lockedAxis)
		{
			state.ValidateSelectionStats();

			Vector3 spaceCoord = state.GetSpaceCoord(new Vector3(mouseLoc.X, mouseLoc.Y, selectionCenter.Z));
			float lastAngle = MathF.Angle(selectionCenter.X, selectionCenter.Y, lastLocSpace.X, lastLocSpace.Y);
			float curAngle = MathF.Angle(selectionCenter.X, selectionCenter.Y, spaceCoord.X, spaceCoord.Y);
			float rotation = curAngle - lastAngle;

			state.RotateSelectionBy(rotation);

			state.ActionLastLocSpace = spaceCoord;
		}

		public Cursor GetCursor()
		{
			return CursorHelper.ArrowActionRotate;
		}
	}
}