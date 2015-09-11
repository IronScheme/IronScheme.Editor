#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion

using System.Reflection;
using IronScheme.Editor.ComponentModel;

[assembly: AssemblyTitle("IronScheme.Editor.Languages")]
[assembly: AssemblyDescription("IronScheme.Editor.Languages")]
[assembly:PluginProvider(typeof(Common_PluginLoader))]

sealed class Common_PluginLoader : AssemblyPluginProvider
{
  public override void LoadAll(IPluginManagerService svc)
  {
    new IronScheme.Editor.Languages.MercuryLang();
    new IronScheme.Editor.Languages.PatchLanguage();
    new IronScheme.Editor.Languages.ScalaLang();
    new IronScheme.Editor.Languages.PowerShellLang();
    new IronScheme.Editor.Languages.CssLang();

    new IronScheme.Editor.Languages.HLSLLang();
    new IronScheme.Editor.Languages.JavaScriptLanguage();
    new IronScheme.Editor.Languages.BooLanguage();
    new IronScheme.Editor.Languages.RubyLang();
    new IronScheme.Editor.Languages.FSharpLang();
    new IronScheme.Editor.Languages.IronPythonLang();
    new IronScheme.Editor.Languages.CppLang();
    new IronScheme.Editor.Languages.NemerleLang();
    new IronScheme.Editor.Languages.VBNETLang();
    new IronScheme.Editor.Languages.NSISLang();
    new IronScheme.Editor.Languages.SqlLang();
    new IronScheme.Editor.Languages.CatLanguage();
    new IronScheme.Editor.Languages.CSLexLang();
    new IronScheme.Editor.Languages.YaccLang();
    new IronScheme.Editor.Languages.LSharp.Parser();

  }
}

