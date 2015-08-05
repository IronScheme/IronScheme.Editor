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
using System.Reflection;
using System.Runtime.CompilerServices;
using IronScheme.Editor.ComponentModel;

[assembly: AssemblyTitle("xacc")]
[assembly: AssemblyDescription("Main lib for xacc")]
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
    new IronScheme.Editor.Languages.ILLanguage();
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
    new IronScheme.Editor.Languages.XmlLanguage();
    new IronScheme.Editor.Languages.LSharp.Parser();
    new IronScheme.Editor.Languages.CSharp.Parser();

  }
}

