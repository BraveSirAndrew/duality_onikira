using System.Drawing;
using System.Windows.Forms;
using OpenTK;

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
	public class NullObjectAction : IObjectAction
	{
		public void Update(CamViewState state, Point mouseLoc, Point beginLoc, Vector3 beginLocSpace, Vector3 lastLocSpace, Vector3 selectionCenter, float selectionRadius, CamViewState.LockedAxis lockedAxis)
		{
			state.InvalidateSelectionStats();
		}

		public string GetStatusText()
		{
			return null;
		}

		public Cursor GetCursor()
		{
			return CursorHelper.Arrow;
		}
	}
}