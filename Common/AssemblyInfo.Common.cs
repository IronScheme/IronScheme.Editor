#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: CLSCompliant(true)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Llewellyn Pritchard")]
[assembly: AssemblyCopyright("2003-2015 Llewellyn Pritchard")]
[assembly: AssemblyProduct("IronScheme.Editor")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("0.9.0.*")]
[assembly: ComVisible(true)]

#if !DEBUG
#pragma warning disable 1699
 // to create a release build, generate a key called 'xacc.key' in the source root
[assembly: AssemblyKeyFile("../../../xacc.key")]
#pragma warning restore 1699
#endif