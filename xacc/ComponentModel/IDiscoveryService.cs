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
using System.IO;
using System.Collections;

using Xacc.CodeModel;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Reflection;

#endregion

namespace Xacc.ComponentModel
{

  /// <summary>
  /// Provides system discovery services
  /// </summary>
  [Name("Discovery")]
  public interface IDiscoveryService : IService
  {
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
    Unknown
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
          if (ass.CodeBase.EndsWith("mscorlib.dll"))
          {
            switch( ass.ImageRuntimeVersion)
            {
              case "v1.1.4322":
                return NetRuntime.Net11;
              case "v2.0.50727":
                return NetRuntime.Net20;
            }
          }
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
      get {return Xacc.Runtime.Compiler.CLRRuntime;}
    }

    public string NetRuntimeRoot
    {
      get 
      {
        string root = string.Empty;
        if (Runtime == Xacc.Runtime.CLR.Microsoft)
        {
          switch( RuntimeVersion )
          {
            case NetRuntime.Net11:
              root = "v1.1.4322";
              break;
            case NetRuntime.Net20:
              root = "v2.0.50727";
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
