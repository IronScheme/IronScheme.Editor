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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using ISite = System.ComponentModel.ISite;

namespace Xacc.ComponentModel
{
	/// <summary>
	/// A common interface for all services
	/// </summary>
	public interface IService
	{
    /// <summary>
    /// Gets the property name of the service in service host if any
    /// </summary>
    string PropertyName {get;}
	}

	/// <summary>
	/// The interface used for providing services.
	/// </summary>
	[Name("Service host","Provides a single store for hosting all services")]
	public interface IServiceProvider : IService
	{
		/// <summary>
		/// Retrieves an already registered service.
		/// </summary>
		/// <param name="servicetype">The type of the service you are requesting</param>
		/// <returns>An instance of the service requested or null if not found</returns>
		IService				GetService						(Type servicetype);
	}


	/// <summary>
	/// Provides a single store for hosting all services
	/// </summary>
	public sealed class ServiceHost : IServiceProvider, IDisposable, ISite
	{
    readonly static Dictionary<Type, IService> services = new Dictionary<Type, IService>();
    readonly static Dictionary<Type, string> propmap = new Dictionary<Type, string>();
    static ApplicationState state = 0;


		/// <summary>
		/// Creates a reference to the service store.
		/// </summary>
		/// <remarks>
		/// This class references static instances, so it can be recklessly recreated without side effects.
		/// </remarks>
		ServiceHost()
		{
			if (services.Count == 0)
			{
				Add(typeof(IServiceProvider), this);
			}

      foreach (PropertyInfo pi in typeof(ServiceHost).GetProperties(
        BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic))
      {
        Type rt = pi.PropertyType;
        if (typeof(IService).IsAssignableFrom(rt))
        {
          propmap.Add(rt, pi.Name);
        }
      }

		}

    //static bool shinit = false;

    static void InitServiceHost()
    {
      if (!SettingsService.idemode)
      {
        PluginManager pm = new PluginManager();
        pm.LoadAssembly(typeof(ServiceHost).Assembly);
      }
      //shinit = true;
    }

    internal static event EventHandler StateChanged;

    internal static ApplicationState State
    {
      get {return state;}
      set 
      {
        if (state != value)
        {
          state = value;
          if (StateChanged != null)
          {
            StateChanged(null, EventArgs.Empty);
          }
        }
      }
    }

    string IService.PropertyName {get {return null;}}

    static internal string GetPropertyName(Type svctype)
    {
      return propmap[svctype];
    }

    static internal readonly ServiceHost INSTANCE = new ServiceHost();

    static ServiceHost()
    {
      InitServiceHost();
    }

		/// <summary>
		/// Add a service to the store.
		/// </summary>
		/// <param name="serviceType">The type of the service to register</param>
		/// <param name="service">The instance of the service</param>
		/// <remarks>If the service type already exists, it will be replaced. 
		/// The can be only one service provider currently.</remarks>
		public void Add(Type serviceType, IService service)
		{
			Type t = serviceType;
			if (services.ContainsKey(t))
			{
				if (t != typeof(IServiceProvider))
				{
					Trace.WriteLine("Loading {0}", serviceType.Name);
					services[t] = service;	
				}
			}
			else
			{
				ServiceBase svb = service as ServiceBase;
				if (svb == null)
				{
					Trace.WriteLine("Loading {0}", serviceType.Name);
				}
				else
				{
					Trace.WriteLine("Loading {0}", svb.Name);
				}
				services.Add(t, service);
			}
		}

		string Name
		{
			get { return NameAttribute.GetName(GetType());}
		}

    class Trace 
    {
      public static void WriteLine(string format, params object[] args)
      {
        Diagnostics.Trace.WriteLine("ServiceHost", format, args);
      }
    }

		/// <summary>
		/// Get a list of all registered service types.
		/// </summary>
		public Type[] ServiceTypes
		{
			get
			{
				return new List<Type>(services.Keys).ToArray();
			}
		}

    internal static void Initialize()
    {
      foreach (IService svc in services.Values)
      {
        if (svc is System.ComponentModel.ISupportInitialize)
        {
          ((System.ComponentModel.ISupportInitialize)svc).BeginInit();
        }
      }

      foreach (IService svc in services.Values)
      {
        if (svc is System.ComponentModel.ISupportInitialize)
        {
          ((System.ComponentModel.ISupportInitialize)svc).EndInit();
        }
      }
    }

		/// <summary>
		/// Retrieves an already registered service.
		/// </summary>
		/// <param name="servicetype">The type of the service you are requesting</param>
		/// <returns>An instance of the service requested or null if not found</returns>
		/// <example><code>
		/// ISomeService s = new ServiceHost().GetService(typeof(ISomeService)) as ISomeService;
		/// if (s != null)
		/// {
		///		// use service
		/// }
		/// </code></example>
    public IService GetService(Type servicetype)
		{
			return services[servicetype] as IService;
		}



    public static T Get<T>() where T:class, IService
    {
      return services[typeof(T)] as T;
    }

    /// <summary>
    /// Gets the IKeyboardService
    /// </summary>
    public static IKeyboardService Keyboard
    {
      get { return Get<IKeyboardService>();}
    }

    /// <summary>
    /// Gets the IEditService
    /// </summary>
    internal static IEditService Edit
    {
      get { return Get<IEditService>(); }
    }

    /// <summary>
    /// Gets the IHelpService
    /// </summary>
    internal static IHelpService Help
    {
      get { return Get<IHelpService>();}
    }

    /// <summary>
    /// Gets the IToolBarService
    /// </summary>
		public static IToolBarService ToolBar
		{
			get { return Get<IToolBarService>();}
		}

    /// <summary>
    /// Gets the IStatusBarService
    /// </summary>
    public static IStatusBarService StatusBar
    {
      get { return Get<IStatusBarService>(); }
    }

    /// <summary>
    /// Gets the IFileManagerService
    /// </summary>
    public static IFileManagerService File
		{
			get { return Get<IFileManagerService>();}
		}

    /// <summary>
    /// Gets the IMenuService
    /// </summary>
    public static IMenuService Menu	
		{
			get { return Get<IMenuService>();}
		}

    /// <summary>
    /// Gets the ISettingsService
    /// </summary>
    public static ISettingsService Settings	
    {
      get { return Get<ISettingsService>();}
    }

    /// <summary>
    /// Gets the IViewService
    /// </summary>
    public static IViewService View
    {
      get { return Get<IViewService>();}
    }

    /// <summary>
    /// Gets the IErrorService
    /// </summary>
    public static IErrorService Error	
		{
			get { return Get<IErrorService>();}
		}

    /// <summary>
    /// Gets the IScriptingService
    /// </summary>
    public static IScriptingService Scripting
    {
      get { return Get<IScriptingService>();}
    }

    /// <summary>
    /// Gets the IProjectManagerService
    /// </summary>
    public static IProjectManagerService Project	
		{
			get { return Get<IProjectManagerService>();}
		}

    /// <summary>
    /// Gets the ILanguageService
    /// </summary>
    public static ILanguageService Language	
		{
			get { return Get<ILanguageService>();}
		}

    /// <summary>
    /// Gets the IConsoleService
    /// </summary>
    public static IConsoleService Console	
		{
			get { return Get<IConsoleService>();}
		}

    /// <summary>
    /// Gets the IWindowService
    /// </summary>
    public static IWindowService Window	
		{
			get { return Get<IWindowService>();}
		}

    /// <summary>
    /// Gets the IToolsService
    /// </summary>
    public static IToolsService Tools
    {
      get { return Get<IToolsService>();}
    }

    /// <summary>
    /// Gets the IDiscoveryService
    /// </summary>
    public static IDiscoveryService Discovery
		{
			get { return Get<IDiscoveryService>();}
		}

    /// <summary>
    /// Gets the IImageListProviderService
    /// </summary>
    public static IImageListProviderService ImageListProvider	
		{
			get { return Get<IImageListProviderService>();}
		}

    /// <summary>
    /// Gets the ICodeModelManagerService
    /// </summary>
    public static ICodeModelManagerService CodeModel	
		{
			get { return Get<ICodeModelManagerService>();}
		}

    /// <summary>
    /// Gets the IFontManagerService
    /// </summary>
    public static IFontManagerService Font	
		{
			get { return Get<IFontManagerService>();}
		}

    /// <summary>
    /// Gets the IDebugService
    /// </summary>
    public static IDebugService Debug
		{
			get { return Get<IDebugService>();}
		}

    /// <summary>
    /// Gets the IPluginManagerService
    /// </summary>
    public static IPluginManagerService Plugin	
		{
			get { return Get<IPluginManagerService>();}
		}

		void IDisposable.Dispose()
		{
			foreach (IService svc in services.Values)
			{
				IDisposable idis = svc as IDisposable;

				if (idis != null && !(svc is ServiceHost))
				{
					idis.Dispose();
				}
			}
    }

    object System.IServiceProvider.GetService(Type serviceType)
    {
      return services[serviceType];
    }

    #region ISite Members

    System.ComponentModel.IComponent ISite.Component
    {
      get
      {
        // TODO:  Add ServiceHost.Component getter implementation
        return null;
      }
    }

    System.ComponentModel.IContainer ISite.Container
    {
      get
      {
        // TODO:  Add ServiceHost.Container getter implementation
        return null;
      }
    }

    bool ISite.DesignMode
    {
      get
      {
        // TODO:  Add ServiceHost.DesignMode getter implementation
        return false;
      }
    }

    string ISite.Name
    {
      get
      {
        // TODO:  Add ServiceHost.System.ComponentModel.ISite.Name getter implementation
        return null;
      }
      set
      {
        // TODO:  Add ServiceHost.System.ComponentModel.ISite.Name setter implementation
      }
    }

    #endregion
  }
}
