using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Duality.Resources;

namespace Duality
{
	public class ReplaySystem
	{
		private enum ReplaySystemStatus
		{
			Recording,
			StartPlaying,
			Playing,
			Idle
		}

		private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();
		const string ReplaySavePath = "scene";
		const string ReplayDirectory = "replay";
		private bool _isPlaying;
		private static ReplaySystemStatus _replaySystemStatus;

		public ReplaySystem()
		{
			_backgroundWorker.DoWork += SaveCurrentScene;
			_replaySystemStatus = ReplaySystemStatus.Idle;
		}

		public void StartRecording()
		{
			if (_replaySystemStatus == ReplaySystemStatus.Idle)
				_replaySystemStatus = ReplaySystemStatus.Recording;

		}

		public void StopRecording()
		{
			if (_replaySystemStatus == ReplaySystemStatus.Recording)
			{
				_backgroundWorker.RunWorkerCompleted += delegate { _replaySystemStatus = ReplaySystemStatus.Idle; };
				_backgroundWorker.DoWork -= SaveCurrentScene;
			}
		}
		private string[] _sceneFileNamess;
		private int _currentSceneIndex;
		public void Play()
		{
			switch (_replaySystemStatus)
			{
				case ReplaySystemStatus.Playing:
					Scene.Current = Resource.Load<Scene>(_sceneFileNamess[_currentSceneIndex]);
					_currentSceneIndex++;
					if (_currentSceneIndex <= _sceneFileNamess.Length)
					{
						_currentSceneIndex = 0;
						Log.Editor.Write("Finished playback");
					}
					break;
				case ReplaySystemStatus.Idle:
					_replaySystemStatus = ReplaySystemStatus.Playing;
					_sceneFileNamess = Directory.GetFiles(ReplayDirectory).ToArray();
					_currentSceneIndex = 0;
					break;
			}

			
		}

		public void Update()
		{
			if (_replaySystemStatus == ReplaySystemStatus.Recording)
			{
				if (!_backgroundWorker.IsBusy)
					_backgroundWorker.RunWorkerAsync();
			}
		}
		private void SaveCurrentScene(object sender, DoWorkEventArgs e)
		{
			Stream stream = File.Create(Path.Combine(ReplayDirectory, ReplaySavePath + Time.MainTimer.Milliseconds + ".res"));
			Scene.Current.Save(stream);
		}
	}
}