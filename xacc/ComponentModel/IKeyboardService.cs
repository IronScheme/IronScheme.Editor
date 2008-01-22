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
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Xacc.Collections;
using System.Reflection;
using LSharp;

namespace Xacc.ComponentModel
{
  /// <summary>
  /// Provide keyboard handling services
  /// </summary>
  public interface IKeyboardService : IService
  {

//    bool Register(string method, ICollection keys);
//    bool Register(string method, ICollection keys, bool ignoreshift, ApplicationState state);
  }

	/// <summary>
	/// Summary description for KeyboardHandler.
	/// </summary>
	sealed class KeyboardHandler : ServiceBase, IKeyboardService
	{
    readonly HashTree tree = new HashTree();
    readonly Stack stack = new Stack();
    readonly static TypeConverter keyconv = new KeysConverterEx();
    readonly Hashtable targets = new Hashtable();

    sealed class KeysConverterEx : KeysConverter
    {
      public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
      {
        if (value is string)
        {
          // Ctrl+Shift+Alt+(J,K,L)
          string[] tokens = ((string)value).Split('+');
          string last = tokens[tokens.Length - 1].Trim();
          if (last.StartsWith("(") && last.EndsWith(")"))
          {
            string[] skeys = last.TrimEnd(')').TrimStart('(').Split(',');
            Keys[] keys = new Keys[skeys.Length];
            Keys mod = 0;

            for (int i = 0; i < tokens.Length - 1; i++)
            {
              mod |= (Keys)ConvertFrom(context, culture, tokens[i].Trim());
            }

            for (int i = 0; i < keys.Length; i++)
            {
              keys[i] = (Keys)ConvertFrom(context, culture, skeys[i].Trim());
              keys[i] |= mod;
            }

            return keys;
          }
        }
        return base.ConvertFrom (context, culture, value);
      }
    }

    public void DumpKeys()
    {
      Set t = new Set();
      foreach (Keys[] keys in targets.Values)
      {
        if (tree.Contains(keys))
        {
          foreach (Action a in tree[keys] as ArrayList)
          {
            string sa = string.Format("{0,-60} Keys: {1}", a, KeyString(keys));
            t.Add(sa);
          }
        }
      }
      ArrayList st = new ArrayList(t);
      st.Sort();

      foreach (string h in st)
      {
        Console.WriteLine(h);
      }
    }

    public void DumpTargets()
    {
      Set t = new Set();
      foreach (Action a in targets.Keys)
      {
        string sa = string.Format("{0,-60} Keys: {1}", a, KeyString(targets[a] as Keys[]));
        t.Add(sa);
      }
      ArrayList st = new ArrayList(t);
      st.Sort();

      foreach (string h in st)
      {
        Console.WriteLine(h);
      }
    }

    internal abstract class Action
    {
      public ApplicationState State = 0;
      protected static readonly object[] NOPARAMS = {};
      public IService target;
      
      public bool StartInvoke()
      {
        try
        {
          if (target is ServiceBase)
          {
            if ((((ServiceBase)target).MenuState & State) == State)
            {
              System.Diagnostics.Trace.WriteLine(this.ToString());
              Invoke();
              return true;
            }
          }
          else
          {
            System.Diagnostics.Trace.WriteLine(this.ToString());
            Invoke();
            return true;
          }
        }
        catch (Exception ex)
        {
#if DEBUG
          System.Diagnostics.Trace.WriteLine(ex.ToString(), "Action.Invoke");
#else
          throw ex;
#endif
        }
        return false;
      }

      public abstract void Invoke();
    }

    static string GetServiceName(IService svc)
    {
      if (Attribute.IsDefined(svc.GetType(), typeof(NameAttribute)))
      {
        NameAttribute na = Attribute.GetCustomAttribute(svc.GetType(), typeof(NameAttribute)) as NameAttribute;
        return na.Name;
      }
      return svc.ToString();
    }

    internal sealed class ToggleAction : Action
    {
      public PropertyInfo pi;
      public override void Invoke()
      {
        pi.SetValue(target,!((bool)pi.GetValue(target, NOPARAMS)), NOPARAMS);
      }

      public override string ToString()
      {
        return string.Format("Toggle: {0}.{1} [{2}]", GetServiceName(target), pi.Name, State);
      }

      public override bool Equals(object obj)
      {
        ToggleAction a = obj as ToggleAction;
        if (a == null)
        {
          return false;
        }
        return a.target == target && a.pi == pi && a.State == State;
      }

      public override int GetHashCode()
      {
        return target.GetHashCode() ^ pi.GetHashCode() ^ (int)State;
      }
    }

    internal sealed class ClosureAction : Action
    {
      public Closure clos;

      public override void Invoke()
      {
        clos.Invoke();
      }

      public override string ToString()
      {
        return string.Format("Closure:{0} [{1}]", clos.ToString().Replace("LSharp.Closure ", string.Empty), State);
      }

      public override bool Equals(object obj)
      {
        ClosureAction a = obj as ClosureAction;
        if (a == null)
        {
          return false;
        }
        return a.clos == clos && a.State == State;
      }

      public override int GetHashCode()
      {
        return clos.GetHashCode() ^ (int)State;
      }
    }

    internal sealed class InvokeAction : Action
    {
      public MethodInfo mi;
      public override void Invoke()
      {

        mi.Invoke(target, NOPARAMS);
      }

      public override string ToString()
      {
        return string.Format("Action: {0}.{1} [{2}]", GetServiceName(target), mi.Name, State);
      }

      public override bool Equals(object obj)
      {
        InvokeAction a = obj as InvokeAction;
        if (a == null)
        {
          return false;
        }
        return a.target == target && a.mi == mi && a.State == State;
      }

      public override int GetHashCode()
      {
        return target.GetHashCode() ^ mi.GetHashCode() ^ (int)State;
      }
    }

    internal sealed class DialogInvokeAction : Action
    {
      public MethodInfo mi;
      object[] pars = null;
      
      public override void Invoke()
      {
        if (pars == null)
        {
          ParameterInfo[] pis = mi.GetParameters();
          int plen = pis.Length;
          pars = new object[plen];

          for (int i = 0; i < pars.Length; i++)
          {
            pars[i] = TypeDescriptor.GetConverter(pis[i].ParameterType).ConvertToString(pis[i].DefaultValue);
          }
        }
        Controls.DialogInvokeActionForm df = new Xacc.Controls.DialogInvokeActionForm(mi);

        if (df.ShowDialog(ServiceHost.Window.MainForm) == DialogResult.OK)
        {
          pars = df.GetValues();
          try
          {
            mi.Invoke(target, pars);
          }
          catch (Exception ex)
          {
            MessageBox.Show(ServiceHost.Window.MainForm, ex.GetBaseException().Message, "Error running command!",
              MessageBoxButtons.OK, MessageBoxIcon.Error);
          }

        }
      }

      public override string ToString()
      {
        return string.Format("Dialog: {0}.{1} [{2}]", GetServiceName(target), mi.Name, State);
      }

      public override bool Equals(object obj)
      {
        InvokeAction a = obj as InvokeAction;
        if (a == null)
        {
          return false;
        }
        return a.target == target && a.mi == mi && a.State == State;
      }

      public override int GetHashCode()
      {
        return target.GetHashCode() ^ mi.GetHashCode() ^ (int)State;
      }
    }

    public void AddToggle(IService t, PropertyInfo pi)
    {
      ToggleAction a = new ToggleAction();
      a.target = t;
      a.pi = pi;

      targets[a] = new Keys[] {Keys.None};
    }


    public void AddTarget(IService t, MethodInfo mi)
    {
      InvokeAction a = new InvokeAction();
      a.target = t;
      a.mi = mi;

      targets[a] = new Keys[] {Keys.None};
    }

  	public KeyboardHandler()
		{
      ServiceHost.Window.MainForm.KeyDown +=new KeyEventHandler(MainForm_KeyDown);
      ServiceHost.Window.MainForm.KeyUp += new KeyEventHandler(MainForm_KeyUp);
    }

    public bool Register(Closure clos, ICollection keys, bool ignoreshift, ApplicationState state)
    {
      if (ignoreshift)
      {
        ArrayList k = new ArrayList(keys);
        for (int i = 0; i < k.Count; i++)
        {
          k[i] = "Shift+" + k[i];
        }
        Register(clos, k, state);
      }
      return Register(clos, keys, state);
    }

    public bool Register(Closure clos, ICollection keys, bool ignoreshift)
    {
      return Register(clos, keys, ignoreshift, ApplicationState.Normal);
    }

    public bool Register(Closure clos, ICollection keys)
    {
      return Register(clos, keys, ApplicationState.Normal);
    }

    public bool Register(Closure clos, ICollection keys, ApplicationState state)
    {
      ArrayList kk = new ArrayList(keys);
      Keys[] k = new Keys[keys.Count];

      if (keys.Count > 1)
      {
        for (int i = 0; i < k.Length; i++)
        {
          k[i] = (Keys)keyconv.ConvertFrom(kk[i]);
        }
      }
      else
      {
        object o = keyconv.ConvertFrom(kk[0]);
        if (o is Keys[])
        {
          k = o as Keys[];
        }
        else
        {
          k[0] = (Keys)o;
        }
      }

      ClosureAction a = new ClosureAction();
      a.clos = clos;
      a.State = state;

      return Register(a, k);
    }

    public bool Register(string method, ICollection keys, bool ignoreshift, ApplicationState state)
    {
      if (ignoreshift)
      {
        ArrayList k = new ArrayList(keys);
        for (int i = 0; i < k.Count; i++)
        {
          k[i] = "Shift+" + k[i];
        }
        Register(method, k, state);
      }

      return Register(method, keys, state);
    }

    public bool Register(string method, ICollection keys, bool ignoreshift)
    {
      return Register(method, keys, ignoreshift, ApplicationState.Normal);
    }


    public bool Register(string method, ICollection keys)
    {
      return Register(method, keys, ApplicationState.Normal);
    }

    public bool Register(string method, ICollection keys, ApplicationState state)
    {
      const BindingFlags BF = BindingFlags.Public | BindingFlags.DeclaredOnly 
              | BindingFlags.NonPublic | BindingFlags.Instance;

      ArrayList kk = new ArrayList(keys);
      
      Keys[] k = new Keys[keys.Count];

      if (keys.Count > 1)
      {
        for (int i = 0; i < k.Length; i++)
        {
          k[i] = (Keys)keyconv.ConvertFrom(kk[i]);
        }
      }
      else
      {
        object o = keyconv.ConvertFrom(kk[0]);
        if (o is Keys[])
        {
          k = o as Keys[];
        }
        else
        {
          k[0] = (Keys)o;
        }
      }

      // service.member(type,...)
      Type[] types = null;
      string[] tokens = method.Split('.', '(');
      string last = tokens[tokens.Length - 1].Trim();
      if (last.EndsWith(")"))
      {
        string[] stypes = last.TrimEnd(')').Split(',');
        types = new Type[stypes.Length];

        for (int i = 0; i < types.Length; i++)
        {
          types[i] = LSharp.TypeCache.FindType(stypes[i].Trim());
        }
      }

      IService svc = typeof(ServiceHost).GetProperty(tokens[0], 
        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly
        ).GetValue(null, new object[0]) as IService;

      if (svc == null)
      {
        Trace.WriteLine("Invalid service: {0}", tokens[0]);
        return false;
      }

      Action a = null;
      PropertyInfo pi = svc.GetType().GetProperty(tokens[1], BF, null, typeof(bool), new Type[0], null);
      if (pi != null)
      {
        ToggleAction ta = new ToggleAction();
        ta.target = svc;
        ta.pi = pi;
        a = ta;
      }

      if (types == null)
      {
        MethodInfo mi = svc.GetType().GetMethod(tokens[1], BF, null, new Type[0], null);
        if (mi != null && mi.ReturnType == typeof(void))
        {
          InvokeAction ta = new InvokeAction();
          ta.target = svc;
          ta.mi = mi;
          a = ta;
        }
      }
      else
      {
        MethodInfo mi = svc.GetType().GetMethod(tokens[1], BF, null, types, null);
        if (mi != null)
        {
          DialogInvokeAction ta = new DialogInvokeAction();
          ta.target = svc;
          ta.mi = mi;
          a = ta;
        }
      }

      if (a == null)
      {
        Trace.WriteLine("WARNING: {0} NOT BOUND to {1} . Target not found.", KeyString(k), method);
        return false;
      }

      a.State = state;

      return Register(a, k);
    }

    static string KeyString(Keys[] keys)
    {
      string[] ks = new string[keys.Length];
      for (int i = 0; i < ks.Length; i++)
      {
        ks[i] = "(" + keyconv.ConvertToString(keys[i]) + ")";
      }

      return string.Join(" ", ks);
    }

    internal bool Register(Action handler, params Keys[] keys)
    {
      if (keys.Length == 0)
      {
        return false;
      }
      targets[handler] = keys;
      ArrayList handlers = tree[keys] as ArrayList;
      if (handlers == null)
      {
        tree[keys] = (handlers = new ArrayList());
      }

      for (int i = 0; i < handlers.Count; i++)
      {
        if (((Action)handlers[i]).State == handler.State)
        {
          handlers[i] = handler;
          goto SUCCESS;
        }
      }

      handlers.Add(handler);

    SUCCESS:

      //tree[keys] = handler;
      Trace.WriteLine("Key: {0,-35} {1}", KeyString(keys), handler);
      return true;
    }

    bool MatchStack(object[] keys)
    {
#if CHECK
      System.Diagnostics.Trace.Write("Current:\t");
      foreach (Keys k in keys)
      {
        System.Diagnostics.Trace.Write("(" + k + ") ");
      }
      System.Diagnostics.Trace.WriteLine("");
#endif
      if (tree.Contains(keys))
      {
        ArrayList hs = (tree[keys] as ArrayList);

#if CHECK
        System.Diagnostics.Trace.Write("Accepts:\t");
        foreach (Keys k in keys)
        {
          System.Diagnostics.Trace.Write("(" + k + ") ");
        }
#endif
        bool success = false;

        foreach (Action h in new ArrayList(hs))
        {
          success = h.StartInvoke();
          if (success)
          {
            break;
          }
        }
#if CHECK
        if (!success)
        {
          System.Diagnostics.Trace.WriteLine("Action: No action bound at current state, ignoring.");
        }
#endif
        stack.Clear();
        return success;
      }
      else
      {
        if (keys.Length == 1)
        {
          return false;
        }

        object[] subkeys = new object[keys.Length - 1];
        Array.Copy(keys, 1, subkeys, 0, subkeys.Length);

        if (MatchStack(subkeys))
        {
          return true;
        }

        if (!tree.IsInPath(keys))
        {
          object[] current = stack.ToArray();
          Array.Reverse(current);
          stack.Clear();
          for (int i = 1; i < current.Length; i++)
          {
            stack.Push(current[i]);
          }
        }
      }
      return false;
    }

    internal void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
      stack.Push(e.KeyData);
      object[] keys = stack.ToArray();
      Array.Reverse(keys);

      //System.Diagnostics.Trace.Write("KeyDown:\t");
      //foreach (Keys k in keys)
      //{
      //  System.Diagnostics.Trace.Write("(" + k + ") ");
      //}
      //System.Diagnostics.Trace.WriteLine("");
      
      e.Handled = 
        MatchStack(keys);

#warning FIX MULTIPLE KEYSTROKES SOMEHOW
    }

    void MainForm_KeyUp(object sender, KeyEventArgs e)
    {
      //System.Diagnostics.Trace.Write("KeyUp:\t");
      //System.Diagnostics.Trace.Write("(" + e.KeyData + ") ");
      //System.Diagnostics.Trace.WriteLine("");
    }

  }
}
