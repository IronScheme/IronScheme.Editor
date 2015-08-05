#region License
/*	  xacc                																											*
 *		Copyright (C) 2003-2006  Llewellyn@Pritchard.org                          *
 *																																							*
 *		This program is free software; you can redistribute it and/or modify			*
 *		it under the terms of the GNU Lesser General Public License as            *
 *   published by the Free Software Foundation; either version 2.1, or					*
 *		(at your option) any later version.																				*
 *																																							*
 *		This program is distributed in the hope that it will be useful,						*
 *		but WITHOUT ANY WARRANTY; without even the implied warranty of						*
 *		MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the							*
 *		GNU Lesser General Public License for more details.												*
 *																																							*
 *		You should have received a copy of the GNU Lesser General Public License	*
 *		along with this program; if not, write to the Free Software								*
 *		Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA */
#endregion

#region Includes
using System;
using System.Collections;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using IronScheme.Editor.ComponentModel;
#endregion


[assembly: PluginProvider(typeof(DefaultPluginProvider))]

namespace IronScheme.Editor.ComponentModel
{
  /// <summary>
  /// Provides services to load plugin assemblies
  /// </summary>
  [Name("Plugin manager", "Provides services for loading assembly plugins")]
	public interface IPluginManagerService : IService
	{
    /// <summary>
    /// Load an assembly
    /// </summary>
    /// <param name="assembly">the assembly to load</param>
		void LoadAssembly(Assembly assembly);
    void LoadFile(string filename);
	}

  /// <summary>
  /// Define the type of the PluginProvider class
  /// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false)]
	public class PluginProviderAttribute : Attribute
	{
		Type type;

    /// <summary>
    /// Creates an instance of PluginProviderAttribute
    /// </summary>
    /// <param name="asspluginprvimpl">the plugin provider type</param>
		public PluginProviderAttribute(Type asspluginprvimpl)
		{
			type = asspluginprvimpl;
		}

    /// <summary>
    /// The type
    /// </summary>
		public Type Type
		{
			get {return type;}
		}
	}

  /// <summary>
  /// Base class for PluginProviders
  /// </summary>
	public abstract class AssemblyPluginProvider
	{
    /// <summary>
    /// Loads all
    /// </summary>
    /// <param name="svc">the calling service</param>
		public abstract void LoadAll(IPluginManagerService svc);
	}

	class DefaultPluginProvider : AssemblyPluginProvider
	{

    public override void LoadAll(IPluginManagerService svc)
    {
      Configuration.IdeSupport.about.progressBar1.Value = 10;
      new LanguageService();

      Configuration.IdeSupport.about.progressBar1.Value += 20;

      new ImageListProvider();
      if (SettingsService.idemode)
      {
        new ToolBarService();
        new MenuService();
        new StatusBarService();
      }

      Configuration.IdeSupport.about.progressBar1.Value += 10;

      new DiscoveryService();
      new CodeModelManager();
      new ErrorService();

      // figure some way out to order these for toolbar/menu
      new FileManager();
      new EditService();
      new ProjectManager();
      new BuildService();
      new DebugService();
      new ToolsService();
      new HelpService();
      new UpdaterService();

      Configuration.IdeSupport.about.progressBar1.Value += 10;

      new ShellService();
      new ScriptingService();
      new StandardConsole();
      new SettingsService();
      new PropertyService(); 

      Configuration.IdeSupport.about.progressBar1.Value = 55;
    }
	}

	sealed class PluginManager : ServiceBase, IPluginManagerService
	{
		static readonly Hashtable loaded = new Hashtable();
    readonly FileSystemWatcher fsw ;

    public PluginManager()
    {
      /*
      if (SettingsService.idemode)
      {
        if (!Directory.Exists(Application.StartupPath + "/Plugins"))
        {
          Directory.CreateDirectory(Application.StartupPath + "/Plugins");
        }

        fsw = new FileSystemWatcher(Application.StartupPath + "/Plugins", "Plugin.*.dll");
        fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite 
          | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        fsw.EnableRaisingEvents = true;
        fsw.Changed += new FileSystemEventHandler(fsw_Changed);
        fsw.Created +=new FileSystemEventHandler(fsw_Created);
        fsw.Deleted +=new FileSystemEventHandler(fsw_Deleted);
        fsw.Renamed +=new RenamedEventHandler(fsw_Renamed);
      }*/
    }

    public void LoadFile(string filename)
    {
      try
      {
        Assembly ass = Assembly.LoadFile(Path.Combine(Application.StartupPath, filename));
        LoadAssembly(ass);
      }
      catch (FileNotFoundException)
      {
        Trace.WriteLine("{0} could not be found", Path.Combine(Application.StartupPath, filename));
      }
    }


		public void LoadAssembly(Assembly ass)
		{
			if (!loaded.ContainsKey(ass))
			{
				Trace.WriteLine("Loading assembly: {0}", ass.FullName);
        try
        {
          foreach (PluginProviderAttribute ppa in 
            ass.GetCustomAttributes(typeof(PluginProviderAttribute), false))
          {
            AssemblyPluginProvider app = Activator.CreateInstance(ppa.Type) as AssemblyPluginProvider;
            if (app == null)
            {
              throw new Exception(string.Format("default constructor expected on {0}", ppa.Type));
            }
            else
            {
              app.LoadAll(this);
              loaded.Add(ass,null);
              Trace.WriteLine("Loaded assembly: {0}", ass.FullName);
            }
            break;
          }
        }
        catch (TypeLoadException)
        {
          Trace.WriteLine("Failed to load assembly: {0}", ass.FullName);
        }
        catch (FileLoadException)
        {
          Trace.WriteLine("Failed to load assembly: {0}", ass.FullName);
        }

        if (ass == typeof(PluginManager).Assembly && SettingsService.idemode)
        {
          if (Directory.Exists("Plugins"))
          {
            foreach (string file in Directory.GetFiles("Plugins", "Plugin.*.dll"))
            {
              byte[] data = null;
              byte[] dbgdata = null;

              using (Stream s = File.OpenRead(file))
              {
                data = new byte[s.Length];
                s.Read(data, 0, data.Length);
              }

              if (File.Exists(Path.ChangeExtension(file, "pdb")))
              {
                using (Stream s = File.OpenRead(Path.ChangeExtension(file, "pdb")))
                {
                  dbgdata = new byte[s.Length];
                  s.Read(dbgdata, 0, dbgdata.Length);
                }

              }
      
              Assembly pass = Assembly.Load(data, dbgdata);
              LoadAssembly(pass);
            }
          }
        }
			}
    }

    private void fsw_Changed(object sender, FileSystemEventArgs e)
    {
;
      //renamed...
    }

    private void fsw_Created(object sender, FileSystemEventArgs e)
    {
      byte[] data = null;
      byte[] dbgdata = null;

      using (Stream s = File.OpenRead(e.FullPath))
      {
        data = new byte[s.Length];
        s.Read(data, 0, data.Length);
      }

      if (File.Exists(Path.ChangeExtension(e.FullPath, "pdb")))
      {
        using (Stream s = File.OpenRead(Path.ChangeExtension(e.FullPath, "pdb")))
        {
          dbgdata = new byte[s.Length];
          s.Read(dbgdata, 0, dbgdata.Length);
        }

      }
      
      Assembly pass = Assembly.Load(data, dbgdata);
      LoadAssembly(pass);
    }

    private void fsw_Deleted(object sender, FileSystemEventArgs e)
    {
;
    }

    private void fsw_Renamed(object sender, RenamedEventArgs e)
    {
;
    }
  }
}
