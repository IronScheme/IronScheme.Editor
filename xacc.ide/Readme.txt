What is xacc.ide?
=================

xacc.ide is a small and fast integrated development environment (IDE) mainly 
targeted at .NET development. It has syntax support for 27 languages and project 
support for various compilers. It has support for step debugging .NET based 
executables. Also featured is brace matching and folding support for some languages.

xacc.ide is also opensource software (OSS). The core is released under the 
GNU LGPL, and the rest BSD license.

Who is developing xacc.ide?
===========================

xacc.ide is developed solely by Llewellyn Pritchard aka leppie. Although it is 
the authors pet project, he sees it as a business (that doesn’t pay yet!) and spend 
most of his free time developing and designing parts for xacc.ide. leppie has a solid 
4 years of experience specializing in C#/.NET and related techonologies.

What does ‘xacc’ mean?
======================

The original meaning has been lost a bit. xacc is meant to stand for XML Attribute 
Compiler Compiler. The name was decided before the realization of the uselessness of 
XML as a general purpose coding language (its by no means a bad langauge!).

How is ‘xacc’ pronounced?
=========================

Like eX-act but with a silent t.

When was xacc/xacc.ide started?
===============================

xacc was started in October 2003. The project been hosted at SourceForge since it’s 
inception.

Requirements
============

Xacc.ide requires the .NET framework 2.0 to run and compile C# and VB.NET applications. 
For other projects, their respective compilers are required to build applications.

Recommended
===========

The following is a list of recommended tools:

•	.NET SDK 2.0
•	Nullsoft Install System (NSIS)

Features
========

•	Super fast code editor/IDE written in 100% C#
•	Syntax highlighting for +-27 general purpose languages
•	Easily customized project support (moving to MSBuild)
•	Integrated debugger for .NET based applications (moving to MDbg)
•	Builtin scripting support (currently LSharp, adding IronPython & PowerShell support)
•	Extensive keybinding support
•	Installation not needed, xcopy support
•	Small distribution
•	Activily developed

Supported Languages
===================

•	C# (1)
•	L# (2)
•	VB.NET
•	SQL
•	PowerShell
•	MSIL
•	F#
•	IronPython
•	Ruby
•	JavaScript
•	C/C++
•	Boo
•	Nemerle
•	XML
•	Mercury
•	HLSL
•	Cat
•	CSLex
•	Yacc
•	NSIS
•	Scala
•	CSS

1. C# has parser support, navigation & code model
2. L# has parser support

Command Window
==============

In this window you can run arbitrary LSharp scripts. The whole content can be evaluated 
by pressing Ctrl + Enter, or only the select line by pressing Ctrl + Shift + Enter.

Contact information
===================

Website	  http://xacc.sourceforge.net
Email	    llewellyn@pritchard.org
Blog	    http://xacc.wordpress.com
Download	http://sourceforge.net/projects/xacc

Credits
=======

I would like to thank all the people that has helped testing. Without you, many bugs would 
be unfound. Here’s an incomplete list:
•	Marc Clifton
•	David Stone
•	Paul Watson
•	Radek Polak
•	Perica
•	Daniel Grunwald
•	All the members of Code Project
•	All bug reporters

The authors of CSlex. (http://www.infosys.tuwien.ac.at/cuplex/lex.htm)
The authors of GPPG. (http://plas.fit.qut.edu.au/gppg/)
Rhys Whetherley for the initial skeleton (gap buffer) for the editor.
Rob Blackwell for Lsharp. (http://www.lsharp.org/)
The author of DockPanelSuite. (http://sourceforge.net/projects/dockpanelsuite/)
The author of TreeViewAdv. (http://sourceforge.net/projects/treeviewadv/)
Phil Wright of ComponentFactory for the Office 2007 style toolstrip renderer.
(http://www.codeproject.com/cs/menu/Office2007Renderer.asp)

Licensing
=========

xacc.dll

The main xacc library is released under the GNU Library GPL. See license.txt in the source 
root for specific details. In laymens terms it means you can use the library unaltered in a 
commercial application, but as soon as changes are made, those sources need to be released. 
I am trying to make it as flexible as possible to prevent this. Another limitation is that 
you may not base your commercial application on xacc, it may only be used as part of a 
commercial application.

xacc art work

No license determined yet. Distributed graphics that are not part of the ‘KDE’ or 
‘Visual C++ Express’ deal, are licensed under LGPL. Please enquire which graphics you are 
interested in, before using them in your own project.

xacc components

All other components that form part of xacc (including Languages, tools and IDE bootstrap) 
are under a general BSD license. In other words, do what you want with it, as long as I 
still get credit for the original code.

Others

All other source code and components under their original licensing.



