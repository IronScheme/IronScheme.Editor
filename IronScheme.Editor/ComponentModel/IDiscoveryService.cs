#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


#region Includes
using System;
using Microsoft.Win32;
using System.Reflection;

#endregion

namespace IronScheme.Editor.ComponentModel
{

  /// <summary>
  /// Provides system discovery services
  /// </summary>
  [Name("Discovery")]
  public interface IDiscoveryService : IService
  {
    /// <summary>
    /// Gets the runtime.
    /// </summary>
    /// <value>The runtime.</value>
    Runtime.CLR Runtime {get;}
    /// <summary>
    /// The current runtime
    /// </summary>
    NetRuntime RuntimeVersion {get;}

    /// <summary>
    /// The .NET runtime root
    /// </summary>
    string NetRuntimeRoot {get;}

    /// <summary>
    /// The .NET SDK directory, if available
    /// </summary>
    string NetRuntimeSDK {get;}
    
    /// <summary>
    /// Whether .NET 1.1 runtime is installed
    /// </summary>
    bool Net11Installed {get;}

    /// <summary>
    /// .NET 1.1 SDK directory
    /// </summary>
    string Net11SdkInstallRoot {get;}
    
    /// <summary>
    /// Whether .NET 2.0 runtime is installed
    /// </summary>
    bool Net20Installed {get;}

    bool Net40Installed { get; }

    /// <summary>
    /// .NET 2.0 SDK directory
    /// </summary>
    string Net20SdkInstallRoot {get;}

    /// <summary>
    /// Whether NSIS is installed
    /// </summary>
    bool NSISInstalled {get;}

    /// <summary>
    /// NSIS directory
    /// </summary>
    string NSISPath {get;}

    /// <summary>
    /// The Visual C++ root
    /// </summary>
    string VCInstallRoot {get;}

  }

  /// <summary>
  /// Runtime version
  /// </summary>
  public enum NetRuntime
  {
    /// <summary>
    /// .NET 1.1
    /// </summary>
    Net11,
    /// <summary>
    /// .NET 2.0
    /// </summary>
    Net20,
    /// <summary>
    /// .NET unknown
    /// </summary>
    Unknown,
    Net40
  }


  class DiscoveryService : ServiceBase, IDiscoveryService
  {
    static readonly RegistryKey NETFX = Registry.LocalMachine.OpenSubKey(
      @"SOFTWARE\Microsoft\.NETFramework");

    public string VCInstallRoot 
    {
      get 
      {
        string
          vcinstdir = Environment.GetEnvironmentVariable("VCTOOLKITINSTALLDIR");
        if (vcinstdir == null || vcinstdir == string.Empty)
        {
          vcinstdir = Environment.GetEnvironmentVariable(@"VCINSTALLDIR\vc7");
        }
        if (vcinstdir == null || vcinstdir == string.Empty)
        {
          //"VS71COMNTOOLS"
          //"VS80COMNTOOLS"
          string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
          vcinstdir = pf + @"\Microsoft Visual Studio .NET 2003\Vc7\";

          //EnvironmentVariables.Add(@"VCINSTALLDIR", vcinstdir);
        }

        vcinstdir = vcinstdir.TrimEnd('\\');

        //EnvironmentVariables.Add("PATH"			, vcinstdir + @"\bin;" + vcinstdir + @"..\Common7\IDE;");
        //EnvironmentVariables.Add("INCLUDE"	, vcinstdir + @"\include;");
        //EnvironmentVariables.Add("LIB"			, vcinstdir + @"\lib;");
        return vcinstdir;
      }
    }

    public string NetRuntimeSDK
    {
      get 
      {
        switch (RuntimeVersion)
        {
          case NetRuntime.Net11:
            return Net11SdkInstallRoot;
          case NetRuntime.Net20:
            return Net20SdkInstallRoot;
        }
        return null;
      }
    }

    public bool Net11Installed
    {
      get 
      { 
        RegistryKey k = NETFX.OpenSubKey("policy\\v1.1");
        if (k == null)
        {
          return false;
        }
        k.Close();
        return true;
      }
    }

    public bool Net20Installed
    {
      get 
      { 
        RegistryKey k = NETFX.OpenSubKey("policy\\v2.0");
        if (k == null)
        {
          return false;
        }
        k.Close();
        return true;
      }
    }

    public bool Net40Installed
    {
      get
      {
        RegistryKey k = NETFX.OpenSubKey("policy\\v4.0");
        if (k == null)
        {
          return false;
        }
        k.Close();
        return true;
      }
    }

    public bool NSISInstalled
    {
      get 
      { 
        RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\NSIS");
        if (k == null)
        {
          return false;
        }
        k.Close();
        return true;
      }
    }

    public string NSISPath
    {
      get 
      { 
        RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\NSIS");
        if (k == null)
        {
          return null;
        }
        object o = k.GetValue("");
        return o as string;
      }
    }

    public NetRuntime RuntimeVersion
    {
      get
      {
        foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
        {
          try
          {
            if (ass.CodeBase.EndsWith("mscorlib.dll"))
            {
              switch (ass.ImageRuntimeVersion)
              {
                case "v1.1.4322":
                  return NetRuntime.Net11;
                case "v2.0.50727":
                  return NetRuntime.Net20;
                case "v4.0.30319":
                  return NetRuntime.Net40;
              }
            }
          }
          catch { }
        }
        return NetRuntime.Unknown;
      }
    }

    public string Net11SdkInstallRoot
    {
      get 
      {
        if (NETFX != null)
        {
          string root = NETFX.GetValue("sdkInstallRootv1.1") as string;
          return root == null ? null : root.TrimEnd('\\'); 
        }
        return string.Empty;
      }
    }

    public string Net20SdkInstallRoot
    {
      get 
      { 
        if (NETFX != null)
        {
          string root = NETFX.GetValue("sdkInstallRootv2.0") as string;
          return root == null ? null : root.TrimEnd('\\'); 
        }
        return string.Empty;
      }
    }

    public Runtime.CLR Runtime
    {
      get {return IronScheme.Editor.Runtime.Compiler.CLRRuntime;}
    }

    public string NetRuntimeRoot
    {
      get 
      {
        string root = string.Empty;
        if (Runtime == IronScheme.Editor.Runtime.CLR.Microsoft)
        {
          switch( RuntimeVersion )
          {
            case NetRuntime.Net11:
              root = "v1.1.4322";
              break;
            case NetRuntime.Net20:
              root = "v2.0.50727";
              break;
            case NetRuntime.Net40:
              root = "v4.0.30319";
              break;

          }
          return NetInstallRoot + root;
        }
        else
        {
          return root;
        }
      }
    }

    static string NetInstallRoot
    {
      get { return NETFX.GetValue("InstallRoot") as string; }
    }
	}
}
