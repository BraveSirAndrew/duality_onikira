using System.ComponentModel;
using System.IO;
using System.Linq;
using Duality.Resources;

namespace Duality
{
	public class ReplaySystem
	{
		private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();
		const string ReplaySavePath = "scene";
		const string ReplayDirectory = "replay";
		public ReplaySystem()
		{
			_backgroundWorker.DoWork += SaveCurrentScene;
		}

		public void SaveFrame(Scene scene)
		{
			if (!_backgroundWorker.IsBusy)
				_backgroundWorker.RunWorkerAsync(scene);
		}

		public void Play()
		{
			foreach (var fileName in Directory.GetFiles(ReplayDirectory).OrderByDescending(x=> x))
			{
				Scene.Current = Resource.Load<Scene>(fileName);
			}
		}

		private void SaveCurrentScene(object sender, DoWorkEventArgs e)
		{
			Stream stream = File.Create(Path.Combine(ReplayDirectory,ReplaySavePath+Time.MainTimer.Milliseconds+".res"));
			var currentScene = (Scene) e.Argument;
			currentScene.Save(stream);
		}
	}
}