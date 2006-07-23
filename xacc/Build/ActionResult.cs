using System;
using System.Collections.Generic;
using System.Text;
using Xacc.CodeModel;

namespace Xacc.Build
{
  /// <summary>
  /// The type of the ActionResult
  /// </summary>
  public enum ActionResultType
  {
    /// <summary>
    /// Invalid state
    /// </summary>
    Invalid,
    /// <summary>
    /// Ok
    /// </summary>
    Ok,
    /// <summary>
    /// Information
    /// </summary>
    Info,
    /// <summary>
    /// Warning
    /// </summary>
    Warning,
    /// <summary>
    /// Error
    /// </summary>
    Error,
  }

  /// <summary>
  /// Class to store the results of invoked Actions.
  /// </summary>
  public struct ActionResult
  {
    Location loc;
    internal string code;
    internal string msg;
    internal ActionResultType type;

    /// <summary>
    /// The type of the ActionResult
    /// </summary>
    public ActionResultType Type
    {
      get { return loc.Error ? ActionResultType.Error : loc.Warning ? ActionResultType.Warning : type; }
    }

    /// <summary>
    /// The message of the ActionResult
    /// </summary>
    public string Message
    {
      get { return msg.Trim(); }
    }

    public string ErrorCode
    {
      get { return code; }
    }

    /// <summary>
    /// The location that generated the ActionResult
    /// </summary>
    public Location Location
    {
      get { return loc; }
    }

    /// <summary>
    /// Creates an instance of an ActionResult
    /// </summary>
    /// <param name="msg">the message</param>
    /// <param name="loc">the location</param>
    public ActionResult(string msg, Location loc)
    {
      this.msg = msg;
      this.loc = loc;
      type = ActionResultType.Info;
      this.code = string.Empty;
    }

    /// <summary>
    /// Creates an instance of an ActionResult
    /// </summary>
    /// <param name="type">result type</param>
    /// <param name="line">line number</param>
    /// <param name="message">the message</param>
    /// <param name="filename">the filename</param>
    public ActionResult(ActionResultType type, int line, string message, string filename)
      : this(type, line, 0, message, filename, string.Empty)
    {
    }

    /// <summary>
    /// Creates an instance of an ActionResult
    /// </summary>
    /// <param name="type">result type</param>
    /// <param name="line">line number</param>
    /// <param name="column">column</param>
    /// <param name="message">the message</param>
    /// <param name="filename">the filename</param>
    public ActionResult(ActionResultType type, int line, int column, string message, string filename, string code)
    {
      this.code = code;
      loc = new Location(line, column, 0, column + 1, filename);
      switch (type)
      {
        case ActionResultType.Error:
          loc.Error = true;
          break;
        case ActionResultType.Warning:
          loc.Warning = true;
          break;
      }
      this.type = type;
      msg = message;
    }

    /// <summary>
    /// Creates an instance of an ActionResult of type Info
    /// </summary>
    /// <param name="line">line number</param>
    /// <param name="message">the message</param>
    /// <param name="filename">the filename</param>
    public ActionResult(int line, string message, string filename)
      :
      this(ActionResultType.Info, line, message, filename)
    {
    }
  }
}
