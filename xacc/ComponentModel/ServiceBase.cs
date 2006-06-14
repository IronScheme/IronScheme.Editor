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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Reflection;
using Xacc.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Diagnostics;

using RealTrace = Xacc.Diagnostics.Trace;
using ToolStripMenuItem = Xacc.Controls.ToolStripMenuItem;

namespace Xacc.ComponentModel
{
  
  /// <summary>
  /// Provides an abstract base class for Service implementations.
  /// </summary>
  [Image("Service.Default.png")]
  [LicenseProvider(typeof(LicFileLicenseProvider))]
  public abstract class ServiceBase : Disposable, IService, IComponent, ISynchronizeInvoke, ISupportInitialize, System.IServiceProvider
  {
    readonly License license = null;
    ToolStripMenuItem toplevel;
    Hashtable attrmap;
   
    readonly string propname;
    readonly ServiceTrace trace;
    readonly Hashtable submenus = new Hashtable();

    //readonly Hashtable contents = new Hashtable();

    /// <summary>
    /// Gets the property name if the service if available.
    /// </summary>
    public string PropertyName
    {
      get {return propname;}
    }

    ISite IComponent.Site
    {
      get {return ServiceHost.INSTANCE; }
      set {;}
    }

    #region Tracing
    /// <summary>
    /// Context bound trace class
    /// </summary>
    protected sealed class ServiceTrace
    {
      readonly ServiceBase ctr;

      /// <summary>
      /// Creates an instance of ServiceTrace
      /// </summary>
      /// <param name="ctr">the service container</param>
      public ServiceTrace(ServiceBase ctr)
      {
        this.ctr = ctr;
      }

      /// <summary>
      /// Write a line to the trace listeners
      /// </summary>
      /// <param name="format">the string format</param>
      /// <param name="args">the string format parameters</param>
      [Conditional("TRACE")]
      public void WriteLine(string format, params object[] args)
      {
        RealTrace.WriteLine(ctr.Name, format, args);
      }

      /// <summary>
      /// Write a line to the trace listeners
      /// </summary>
      /// <param name="value">the value</param>
      [Conditional("TRACE")]
      public void WriteLine(object value)
      {
        RealTrace.WriteLine(ctr.Name, value.ToString());
      }

      [Conditional("TRACE")]
      public void Assert(bool condition, string message)
      {
        if (!condition)
        {
          RealTrace.WriteLine(ctr.Name, "Assert failed: {0}", message);
        }
      }
    }

    /// <summary>
    /// Gets context bound trace
    /// </summary>
    protected ServiceTrace Trace
    {
      get { return trace; }
    }

    #endregion

    /// <summary>
    /// Checks if service has a toplevel menu
    /// </summary>
    public bool HasMenu
    {
      get {return toplevel != null;}
    }

    /// <summary>
    /// Gets the name of the service
    /// </summary>
    public string Name
    {
      get { return NameAttribute.GetName(GetType()); }
    }

    /// <summary>
    /// Gets the toplevelmenu
    /// </summary>
    protected ToolStripMenuItem TopLevelMenu
    {
      get { return toplevel; }
    }

    /// <summary>
    /// Called when object is disposed
    /// </summary>
    /// <param name="disposing">true is Dispose() was called</param>
    protected override void Dispose(bool disposing)
    {
      Trace.WriteLine("Dispose({0})", disposing.ToString().ToLower());
      if(disposing)
      {
        if (license != null) 
        {
          license.Dispose();
        }
      }
    }


    /// <summary>
    /// Creates an instance of a service
    /// </summary>
    protected ServiceBase() : this(null)
    {
    }

    /// <summary>
    /// Creates an instance of a service
    /// </summary>
    /// <param name="t">The type of the service to register</param>
    ServiceBase(Type t)
    {
      trace = new ServiceTrace(this);
      Type tt = GetType();
      try
      {
        if ( t == null)
        {
          foreach(Type it in tt.GetInterfaces())
          {
            if (it != typeof(IService))
            {
              if (typeof(IService).IsAssignableFrom(it))
              {
                t = it;
                break;
              }
            }
          }
        }

        if (t == null)
        {
          throw new ArgumentException("No service interfaces has been defined", "t");
        }

        license = LicenseManager.Validate(t, this);
        //if (license == null) //no license...

        propname = ServiceHost.GetPropertyName(t);
        ServiceHost.INSTANCE.Add(t, this);
      }
      catch (Exception ex)
      {
        Trace.WriteLine("{1}() : {0}", ex, tt);
      }
    }

    /// <summary>
    /// Gets called after all processes has been loaded
    /// </summary>
    [SuppressMenu]
    protected virtual void Initialize()
    {

    }

    /// <summary>
    /// Gets or sets the menu state, if used
    /// </summary>
    internal ApplicationState MenuState
    {
      get {return ServiceHost.State; }
    }


    #region ISynchronizeInvoke Members

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
    ///	path='doc/members/member[@name="M:System.Windows.Forms.Control.EndInvoke(System.IAsyncResult)"]/*'/>
    public object EndInvoke(IAsyncResult asyncResult)
    {
      return ServiceHost.Window.MainForm.EndInvoke(asyncResult);
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
    ///	path='doc/members/member[@name="M:System.Windows.Forms.Control.Invoke(System.Delegate, System.Object[])"]/*'/>
    public object Invoke(Delegate method, object[] args)
    {
      return ServiceHost.Window.MainForm.Invoke(method, args);
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
    ///	path='doc/members/member[@name="P:System.Windows.Forms.Control.InvokeRequired"]/*'/>
    [SuppressMenu]
    public bool InvokeRequired
    {
      get { return ServiceHost.Window.MainForm.InvokeRequired; }
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
    ///	path='doc/members/member[@name="M:System.Windows.Forms.Control.BeginInvoke(System.Delegate, System.Object[])"]/*'/>
    public IAsyncResult BeginInvoke(Delegate method, object[] args)
    {
      return ServiceHost.Window.MainForm.BeginInvoke(method, args);
    }

    #endregion

    #region ISupportInitialize Members

    void ISupportInitialize.BeginInit()
    {
      const BindingFlags BF = BindingFlags.Public | BindingFlags.DeclaredOnly 
              | BindingFlags.NonPublic | BindingFlags.Instance;

      Type tt = GetType();
      MenuService ms = ServiceHost.Menu as MenuService;

      if (Attribute.IsDefined(tt, typeof(MenuAttribute), false))
      {
        MenuAttribute mat = tt.GetCustomAttributes(typeof(MenuAttribute), false)[0] as MenuAttribute;

        toplevel = ms[mat.Text];

        if (toplevel == null)
        {
          toplevel = new ToolStripMenuItem(mat.Text);
          ms.AddTopLevel(toplevel);
        }

        attrmap = ms.GetAttributeMap(toplevel);

        EventHandler ev = new EventHandler(DefaultHandler); 

        ArrayList submenus = new ArrayList();

        foreach (MethodInfo mi in tt.GetMethods(BF))
        {
          if (mi.GetParameters().Length == 0)
          {
            bool hasat = Attribute.IsDefined(mi, typeof(MenuItemAttribute));

            if (mi.ReturnType == typeof(void) && (!mi.IsPrivate || hasat) 
              && !Attribute.IsDefined(mi, typeof(SuppressMenuAttribute), true))
            {
              (ServiceHost.Keyboard as KeyboardHandler).AddTarget(this, mi);
            }
            if (hasat)
            {
              MenuItemAttribute mia = mi.GetCustomAttributes(typeof(MenuItemAttribute), false)[0] as MenuItemAttribute;
              mia.invoke = mi;
              mia.ctr = this;
              submenus.Add(mia);
            }
          }
        }

        foreach (PropertyInfo pi in tt.GetProperties(BF))
        {
          if (pi.PropertyType == typeof(bool) && pi.CanRead && pi.CanWrite)
          {
            bool hasat = Attribute.IsDefined(pi, typeof(MenuItemAttribute));

            if (!Attribute.IsDefined(pi, typeof(SuppressMenuAttribute), true))
            {
              (ServiceHost.Keyboard as KeyboardHandler).AddToggle(this, pi);
            }

            if (hasat)
            {
              MenuItemAttribute mia = pi.GetCustomAttributes(typeof(MenuItemAttribute), false)[0] as MenuItemAttribute;
              mia.invoke = pi;
              mia.istogglemenu = true;
              mia.ctr = this;
              submenus.Add(mia);
            }
          }
        }

        foreach (ToolStripMenuItem mi in toplevel.DropDownItems)
        {
          object mia = attrmap[mi];
          if (mia != null)
          {
            submenus.Add(mia);
          }
        }

        submenus.Sort();
        int previdx = -1;

        int counter = 0;

        ToolBarService tbs = ServiceHost.ToolBar as ToolBarService;

#warning TODO: FIX THIS CRAP
        toplevel.DropDownItems.Clear();
        this.submenus.Clear();

        foreach (MenuItemAttribute mia in submenus)
        {
          ToolStripMenuItem pmi = null;
          if (mia.mi == null)
          {
            pmi = new ToolStripMenuItem(mia.Text);
            pmi.Click += ev;
          }
          else
          {
            pmi = mia.mi as ToolStripMenuItem;
          }

          if (mia.istogglemenu)
          {
            PropertyInfo pi = mia.invoke as PropertyInfo;
            try
            {
              bool v = (bool) pi.GetValue(this, new object[0]);
              pmi.Checked = v;
            }
            catch
            {
              //something not ready, sorts itself out
            }
          }
        
          if (previdx != -1 && mia.Index > previdx + 1)
          {
            toplevel.DropDownItems.Add("-");
            counter++;
          }
          int imgidx = -1;
          if (mia.Image != null)
          {
            pmi.Tag = mia.Image;
            imgidx = ServiceHost.ImageListProvider[mia.Image];
          }
          mia.mi = pmi;

          ToolStripMenuItem top = toplevel;

          // check text
          string[] tokens = mia.Text.Split('\\');
          if (tokens.Length > 1)
          {

            ToolStripMenuItem sub = this.submenus[tokens[0]] as ToolStripMenuItem;
            if (sub == null)
            {
              this.submenus[tokens[0]] = sub = new ToolStripMenuItem(tokens[0]);
              top.DropDownItems.Add(sub);
            }
            top = sub;

            pmi.Text = tokens[1];
            top.DropDownItems.Add(pmi);
            counter--;
          }
          else
          {
            top.DropDownItems.Add(pmi);
          }

          attrmap[pmi] = mia;
          
          if (SettingsService.idemode)
          {
            if (mia.AllowToolBar)
            {
              tbs.Add(toplevel, mia);
            }
          }

          previdx = mia.Index;
          counter++;
        }
      }
      Initialize();
    }

    internal void DefaultHandler(object sender, EventArgs e)
    {
      if (InvokeRequired)
      {
        Invoke( new EventHandler(DefaultHandler), new object[] {sender, e});
        return;
      }
      try
      {
        ToolStripMenuItem menu = sender as ToolStripMenuItem;
        MenuItemAttribute mia = attrmap[menu] as MenuItemAttribute;
      
        if (mia == null)
        {
          menu = menu.clonedfrom;
          mia = attrmap[menu] as MenuItemAttribute;
        }

        if (mia.istogglemenu)
        {
          PropertyInfo pi = mia.invoke as PropertyInfo;
          menu.Checked = !menu.Checked;
          pi.SetValue(this, menu.Checked, new object[0]);
        }
        else
        {
          MethodInfo mi = mia.invoke as MethodInfo;
          mi.Invoke(this, new object[0]);
        }
      }
      catch (Exception ex)
      {
#if DEBUG
        Trace.WriteLine(ex);
#else
        throw ex;
#endif
      }
    }

    void ISupportInitialize.EndInit()
    {
      
    }

    #endregion

    #region IServiceProvider Members

    object System.IServiceProvider.GetService(Type serviceType)
    {
      return ServiceHost.INSTANCE.GetService(serviceType);
    }

    #endregion
  }
}
