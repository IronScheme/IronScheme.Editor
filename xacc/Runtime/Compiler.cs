#define MS
//#define MS
//#define MONO
//#define PNET

using System;

namespace Xacc.Runtime
{
  public enum CLR
  {
    Microsoft,
    Mono,
    DotGNU
  }

	/// <summary>
	/// Summary description for Compiler.
	/// </summary>
	public sealed class Compiler
	{
		Compiler(){}

#if MS
    const string CC = "csc.exe";
    const string DEFAULTARGS = "-nologo";
    const string REFPREFIX = "-r:";
    const string OUTPREFIX = "-out:";
#elif MONO
    const string CC = "mcs";
    const string DEFAULTARGS = "";
    const string REFPREFIX = "-r:"; // ????
    const string OUTPREFIX = "-out:"; // ????
#elif PNET
    const string CC = "cscc";
    const string DEFAULTARGS = "";
    const string REFPREFIX = "-l";
    const string OUTPREFIX = "-o";
#endif

    public static CLR CLRRuntime
    {
      get 
      {
        return CLR.
#if MS
          Microsoft
#elif MONO
          Mono
#elif PNET
          DotGNU
#endif
          ;
      }
    }

    public static string Command
    {
      get {return CC;}
    }

    public static string DefaultArgs
    {
      get {return DEFAULTARGS;}
    }

    public static string MakeReferences(params string[] dlls)
    {
      string output = string.Empty;
      foreach (string dll in dlls)
      {
        output += MakeReference(dll) + " ";
      }
      return output.Trim();
    }

    public static string MakeReference(string dll)
    {
      return REFPREFIX + dll;
    }

    public static string MakeOut(string name)
    {
      return OUTPREFIX + name;
    }

    public static string MakeDebug(bool dbg)
    {
#if MS
      return dbg ? "-debug" : "-o";
#elif MONO
      return dbg ? "-debug" : "-optimize";
#elif PNET
      return dbg ? "-g" : "-O2";
#endif
    }

    public static string MakeTarget(string type)
    {
#if MS
      return "-t:" + type;
#elif MONO
      return "-t:" + type; //????
#elif PNET
      return (type == "library") ? "-shared" : "";
#endif
    }

   
	}
}
