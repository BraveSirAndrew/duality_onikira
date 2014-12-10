using System.Drawing;
using System.Windows.Forms;
using Duality.Editor.Plugins.CamView.Properties;
using OpenTK;

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
	public class MoveObjectAction : IObjectAction
	{
		public void Update(CamViewState state, Point mouseLoc, Point beginLoc, Vector3 beginLocSpace, Vector3 lastLocSpace, Vector3 selectionCenter, float selectionRadius, CamViewState.LockedAxis lockedAxis)
		{
			state.ValidateSelectionStats();

			float zMovement = state.CameraObj.Transform.Pos.Z - lastLocSpace.Z;
			Vector3 target = state.GetSpaceCoord(new Vector3(mouseLoc.X, mouseLoc.Y, selectionCenter.Z + zMovement));
			Vector3 movLock = beginLocSpace - lastLocSpace;
			Vector3 mov = target - lastLocSpace;
			mov.Z = zMovement;
			target.Z = 0;

			if (lockedAxis == CamViewState.LockedAxis.X)
			{
				mov = new Vector3(mov.X, 0, 0);
			}
			else if (lockedAxis == CamViewState.LockedAxis.Y)
			{
				mov = new Vector3(0, mov.Y, 0);
			}
			else
			{
				mov = state.ApplyAxisLock(mov, movLock, target + (Vector3.UnitZ * state.CameraObj.Transform.Pos.Z) - beginLocSpace);
			}

			state.MoveSelectionBy(mov);

			state.ActionLastLocSpace += mov;
		}

		public string GetStatusText()
		{
			return CamViewRes.CamView_Action_Move;
		}

		public Cursor GetCursor()
		{
			return CursorHelper.ArrowActionMove;
		}
	}
}