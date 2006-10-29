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
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Drawing;
using Xacc.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using Xacc.Controls;
using Xacc.Languages;
using Xacc.Build;

using SR = System.Resources;
#endregion

namespace Xacc.ComponentModel
{
  /// <summary>
  /// Provides services for manage syntax languages
  /// </summary>
	[Name("Language settings")]
	public interface ILanguageService : IService
	{
    /// <summary>
    /// Suggest a Language for a filename
    /// </summary>
    /// <param name="filename">the filename</param>
    /// <returns>a Language instance</returns>
    Language        Suggest               (string filename);

    /// <summary>
    /// Gets the default language
    /// </summary>
    Language        Default               {get;}

    /// <summary>
    /// Gets the language associated with a specific extension
    /// </summary>
		Language				this									[string extension] {get;}

		/// <summary>
		/// Returns a list of strings of registered extensions.
		/// </summary>
		string[]				AllRegistered					{get;}

    /// <summary>
    /// Checks if an extension has been registered
    /// </summary>
    /// <param name="extension">the extension to test</param>
    /// <returns>true if registered</returns>
		bool						IsRegistered					(string extension);

    /// <summary>
    /// Checks if an extension has been registered
    /// </summary>
    /// <param name="lang">the language to test</param>
    /// <returns>true if registered</returns>
		bool						IsRegistered					(Language lang);

    /// <summary>
    /// Gets the language.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    Language GetLanguage(string name);

    /// <summary>
    /// Registers a language
    /// </summary>
    /// <param name="lang">the language to register</param>
		void						Register							(Language lang);

    /// <summary>
    /// Registers a language
    /// </summary>
    /// <param name="lang">the language to register</param>
    /// <param name="replace">if an existing item should be replaced</param>
		void						Register							(Language lang, bool replace);

    /// <summary>
    /// Gets an array of all registered languages
    /// </summary>
		Language[]			Languages							{get;}

    /// <summary>
    /// Adds a language specific action
    /// </summary>
    /// <param name="name">the name of the language</param>
    /// <param name="a">the type of the action</param>
    void AddAction(string name, Type a);


    //event EventHandler Registered;
	}

	sealed class LanguageService : ServiceBase, ILanguageService
	{
    readonly Dictionary<string, Language> extmap = new Dictionary<string, Language>();
    readonly Dictionary<string, Language> langmap = new Dictionary<string, Language>();

    public event EventHandler Registered;

    internal static readonly Dictionary<TokenClass, ColorInfo> typemapping = new Dictionary<TokenClass, ColorInfo>();

    static void AddTokenType(TokenClass c, Color col, Color back, Color border, FontStyle s)
    {
      typemapping[c] = new ColorInfo(col, back, border, s);
    }

    static void AddTokenType(TokenClass c, Color col, Color back, FontStyle s)
    {
      typemapping[c] = new ColorInfo(col, back, s);
    }

    static void AddTokenType(TokenClass c, Color col, Color back)
    {
      AddTokenType(c, col, back, 0);
    }

    static void AddTokenType(TokenClass c, Color col)
    {
      AddTokenType(c, col, Color.Transparent);
    }

    static LanguageService()
    {
      AddTokenType(TokenClass.Error, Color.Red);
      AddTokenType(TokenClass.Warning, Color.Black); //not used
      AddTokenType(TokenClass.Ignore, Color.Black); //not used
      AddTokenType(TokenClass.Any, Color.Black);
      AddTokenType(TokenClass.Identifier, Color.Black);
      AddTokenType(TokenClass.Type, Color.Teal);
      AddTokenType(TokenClass.Keyword, Color.Blue);
      AddTokenType(TokenClass.Preprocessor, Color.DarkBlue);
      AddTokenType(TokenClass.String, Color.Maroon);
      AddTokenType(TokenClass.Character, Color.DarkOrange);
      AddTokenType(TokenClass.Number, Color.Red);
      AddTokenType(TokenClass.Pair, Color.DarkBlue, Color.LightGray);
      AddTokenType(TokenClass.Comment, Color.DarkGreen);
      AddTokenType(TokenClass.DocComment, Color.DimGray);
      AddTokenType(TokenClass.Operator, Color.DarkBlue);
      AddTokenType(TokenClass.Other, Color.DeepPink);
    }

    public LanguageService()
    {
      new Languages.PlainText();
      new Languages.Changelog();
      new Languages.CSLexLang();
      new Languages.YaccLang();
      new Languages.XmlLanguage();
      new LSharp.Parser();
      new CSharp.Parser();
    }

    public Language GetLanguage(string name)
    {
      Language l = langmap[name] as Language;
      if (l == null)
      {
        return Default;
      }
      return l;
    }

    public void SetTokenClassColor(TokenClass t, Color c)
    {
      AddTokenType(t, c);
    }

    public void SetTokenClassColor(TokenClass t, Color c, Color back)
    {
      AddTokenType(t, c, back);
    }

    public void SetTokenClassColor(TokenClass t, Color c, Color back, FontStyle style)
    {
      AddTokenType(t, c, back, style);
    }

    public void SetTokenClassColor(TokenClass t, Color c, Color back, Color border, FontStyle style)
    {
      AddTokenType(t, c, back, border, style);
    }

		public Language this[string extension]
		{
			get 
			{
				return IsRegistered(extension.ToLower()) ? extmap[extension.ToLower()] as Language : null;
			}
		}

    readonly Hashtable langactions = new Hashtable();

    ArrayList GetActions(string name)
    {
      ArrayList actions = langactions[name] as ArrayList;
      if (actions == null)
      {
        langactions[name] = (actions = new ArrayList());
      }
      return actions;
    }

    public void AddAction(string name, Type a)
    {
      GetActions(name).Add(a);
    }

    public Language Default 
    {
      get {return this["*"];}
    }

    public Language Suggest (string filename)
    {
      string fullpath = Path.GetFullPath(filename);
      filename = Path.GetFileName(filename);
      string ext = Path.GetExtension(filename).TrimStart('.');

      Language s = this[ext];

      if (s == null || s == Default)
      {
        //get suggestion
        foreach (Language l in Languages)
        {
          if (l.Match(filename))
          {
            return l;
          }
        }

        if (File.Exists(fullpath))
        {
          Stream ss = new FileStream(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
          using (TextReader r = new StreamReader(ss, true))
          {
            string startline; 
            while ((startline = r.ReadLine()) != null)
            {
              startline = startline.Trim();
              if (startline.Length > 0)
              {
                foreach (Language l in Languages)
                {
                  if (l.MatchLine(startline))
                  {
                    return l;
                  }
                }
                break; //sneaky, was trying to avoid 'generous' goto use, but alas it failed me!
              }
            }
          }
        }
        s = Default;
      }
      return s;
    }

		public string[] AllRegistered
		{
			get {return new ArrayList(extmap.Keys).ToArray(typeof(string)) as string[];}
		}

		public Language[] Languages
		{
			get {return new ArrayList(langmap.Values).ToArray(typeof(Language)) as Language[];}
		}


		public bool IsRegistered(string extension)
		{
			return extmap.ContainsKey(extension.ToLower());
		}

		public bool IsRegistered(Language lang)
		{
			return extmap.ContainsValue(lang);
		}

		public void Register(Language lang)
		{
			Register(lang, true);
		}
			
		public void Register(Language lang, bool replace)
		{
			foreach (string ext in lang.Extensions)
			{
				string x = ext.ToLower();

				if (extmap.ContainsKey(x))
				{
					if (replace)
					{
						extmap.Remove(x);
					}
					else
					{
						if (extmap[x] != lang)
						{
							throw new ArgumentException(String.Format("'{0}' has already been registered", x));
						}
						else
						{
#if TRACE
							Trace.WriteLine(String.Format("'{0}' has already been registered", x),"WARNING");
#endif
							return;
						}
					}
				}
				Trace.WriteLine(string.Format("Registering ext '{0}' with {1}", x, lang.Name));
				extmap.Add(x, lang);
			}
      if (lang.Extensions.Length == 0)
      {
        string x = string.Empty;
        Trace.WriteLine(string.Format("Registering ext '{0}' with {1}", x, lang.Name));
        extmap[x] = lang;
      }

      //if (!langmap.ContainsKey(lang))
      {
        Trace.WriteLine(string.Format("Adding {0}", lang.Name));
        langmap[lang.Name] = lang;
      }

      lang.actions = GetActions(lang.Name);

      if (Registered != null)
      {
        Registered(lang, EventArgs.Empty);
      }
		}
	}
}
