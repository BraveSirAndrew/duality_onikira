using System.Drawing;
using System.Windows.Forms;
using OpenTK;

namespace Duality.Editor.Plugins.CamView.CamViewStates
{
	public interface IObjectAction
	{
		void Update(CamViewState state, Point mouseLoc, Point beginLoc, Vector3 beginLocSpace, Vector3 lastLocSpace, Vector3 selectionCenter, float selectionRadius, CamViewState.LockedAxis lockedAxis);
		string GetStatusText();
		Cursor GetCursor();
	}
}