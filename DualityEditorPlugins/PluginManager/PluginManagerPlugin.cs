using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Duality;
using Duality.Editor;
using Duality.Editor.Forms;
using Duality.Editor.Properties;
using Mono.Options;
using NuGet;
using PluginManager.Modules;
using PluginManager.Properties;
using WeifenLuo.WinFormsUI.Docking;

namespace PluginManager
{
	public class PluginManagerPlugin : EditorPlugin
	{
		private const string DefaultRepositoryURI = "https://www.myget.org/F/dualityplugins/";

		private PluginManagerView _pluginManagerView = null;
		private ToolStripMenuItem _menuItemPluginManagerView = null;
		private IPackageRepository _sourceRepository;
		private PackageManager _manager;

		public override string Id
		{
			get { return "PluginManager"; }
		}

		protected override IDockContent DeserializeDockContent(Type dockContentType)
		{
			if (dockContentType == typeof(PluginManagerView))
				return RequestPluginManagerView();

			return base.DeserializeDockContent(dockContentType);
		}

		protected override void InitPlugin(MainForm main)
		{
			base.InitPlugin(main);

			_menuItemPluginManagerView = main.RequestMenu(GeneralRes.MenuName_View, PluginManagerRes.MenuItemName_PluginManagerView);
			_menuItemPluginManagerView.Image = PluginManagerResCache.IconPluginManagerView.ToBitmap();
			_menuItemPluginManagerView.Click += menuItemPluginManagerView_Click;
		}

		private void OnCommandEntered(object sender, CommandEnteredEventArgs e)
		{
			var pluginName = "";
			var listPlugins = false;
			var showHelp = false;

			var commandOptionsSet = new OptionSet()
			{
				{"install=", "the name of the plugin to install", v => pluginName = v},
				{"list", "list all available plugins", v => listPlugins = v != null},
				{"?|h|help", "show this help message", v => showHelp = v != null}
			};

			try
			{
				commandOptionsSet.Parse(e.Command.Split(new []{" "}, StringSplitOptions.None));
			}
			catch (Exception)
			{
				ShowHelp(commandOptionsSet);
			}

			if (listPlugins)
			{
				ListPlugins();
				return;
			}

			if (!string.IsNullOrEmpty(pluginName))
			{
				InstallPlugin(pluginName);
				return;
			}

			if (showHelp)
			{
				ShowHelp(commandOptionsSet);
			}
		}

		private void ListPlugins()
		{
			_pluginManagerView.WriteText(_sourceRepository.GetPackages().ToList().Select(s => s.Id).ToString(s => s, "\n"));
		}

		private void InstallPlugin(string pluginName)
		{
			var package = _manager.SourceRepository.GetPackages().Where(p => p.Id == pluginName).ToList().FirstOrDefault();
			if (package == null)
			{
				_pluginManagerView.WriteText(string.Format("Couldn't find plugin '{0}'.", pluginName));
				return;
			}

			_pluginManagerView.WriteText("Checking Duality version compatibility...\n");

			var dualityDependency = package.FindDependency("Duality", null);
			var dualityVersion = new SemanticVersion(typeof(DualityApp).Assembly.GetName().Version);
			if (dualityDependency.VersionSpec.MinVersion.Version.MajorRevision != dualityVersion.Version.MajorRevision)
			{
				_pluginManagerView.WriteText(string.Format("Plugin is incompatible with this version of Duality. Please update to at least version {0}.", package.Version.Version.Major));
				return;
			}

			_pluginManagerView.WriteText("Installing plugin...");

			foreach (var packageDependencySet in package.DependencySets)
			{
				foreach (var dependency in packageDependencySet.Dependencies)
				{
					if (dependency.Id.ToLower() == "duality")
						continue;

					var dependencyPackage = _sourceRepository.FindPackage(dependency.Id);

					if (dependencyPackage == null)
					{
						_pluginManagerView.WriteText(string.Format("Couldn't find dependency {0}", dependency.Id));
						continue;
					}

					_manager.InstallPackage(dependencyPackage, false, false);
				}
			}

			_manager.InstallPackage(package, true, false);
		}

		private void ShowHelp(OptionSet commandOptionsSet)
		{
			using (var writer = new StringWriter())
			{
				commandOptionsSet.WriteOptionDescriptions(writer);
				_pluginManagerView.WriteText(writer.ToString());
			}
		}

		private IDockContent RequestPluginManagerView()
		{
			if (_pluginManagerView == null || _pluginManagerView.IsDisposed)
			{
				_pluginManagerView = new PluginManagerView();
				_pluginManagerView.FormClosed += (sender, args) => _pluginManagerView = null;
			}

			_pluginManagerView.Show(DualityEditorApp.MainForm.MainDockPanel);
			if (_pluginManagerView.Pane != null)
			{
				_pluginManagerView.Pane.Activate();
				_pluginManagerView.Focus();
			}

			if (string.IsNullOrEmpty(_pluginManagerView.RepositoryURI))
				_pluginManagerView.RepositoryURI = DefaultRepositoryURI;

			_pluginManagerView.CommandEntered += OnCommandEntered;

			_sourceRepository = PackageRepositoryFactory.Default.CreateRepository(_pluginManagerView.RepositoryURI);
			_manager = new PackageManager(_sourceRepository, DualityApp.PluginDirectory);

			return _pluginManagerView;
		}

		private void menuItemPluginManagerView_Click(object sender, EventArgs e)
		{
			RequestPluginManagerView();
		}
	}
}