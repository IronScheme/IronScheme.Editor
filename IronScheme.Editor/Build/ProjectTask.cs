#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using BuildProject = Microsoft.Build.BuildEngine.Project;

namespace IronScheme.Editor.Build
{
  public class ProjectOutput : Task
  {
    ITaskItem[] input = { };
    string[] output = { };

    [Required]
    public ITaskItem[] Input
    {
      get { return input; }
      set { input = value; }
    }

    [Output]
    public string[] Output
    {
      get { return output; }
    }

    public override bool Execute()
    {
      int i = 0;

      Engine e = Engine.GlobalEngine;

      e.BinPath = ToolLocationHelper.GetPathToDotNetFramework(TargetDotNetFrameworkVersion.Version20);

      this.output = new string[input.Length];

      foreach (ITaskItem ti in input)
      {
        string location = ti.ItemSpec;
        BuildProject p =  e.GetLoadedProject(location);

        if (p == null)
        {
          p = e.CreateNewProject();
          p.Load(location);
        }

        string assname = p.GetEvaluatedProperty("AssemblyName") ?? p.GetEvaluatedProperty("MSBuildProjectName");
        string outpath = p.GetEvaluatedProperty("OutputPath") ?? ".";
        string outtype = p.GetEvaluatedProperty("OutputType") ?? "WinExe";

        string ext = ".exe";

        if (string.Compare(outtype, "Library", true) == 0)
        {
          ext = ".dll";
        }

        this.output[i] = System.IO.Path.Combine(outpath, assname + ext);

        this.output[i] = this.output[i] ?? "fake";

        Log.LogMessage("Project: {1,-20}\tOutput: {0}", this.output[i], p.GetEvaluatedProperty("MsBuildProjectName"));

        i++;
      }
      return true;
    }
  }
}
