#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IronScheme.Editor.Algorithms
{
  static class XString
  {
    readonly static Dictionary<string, Regex> tokenizecache = new Dictionary<string,Regex>();

    public static string[] Tokenize(string name, params string[] delimiters)
    {
      for (int i = 0; i < delimiters.Length; i++)
      {
        delimiters[i] = Regex.Escape(delimiters[i]);
      }

      string del = string.Join("|", delimiters);

      Regex re = null;
      
      if (tokenizecache.ContainsKey(del))
      {
        re = tokenizecache[del] as Regex;
      }
      else
      {
         tokenizecache.Add(del, re = new Regex(del, RegexOptions.Compiled));
      }

      List<string> tokens = new List<string>();
      int lastend = 0;

      foreach (Match m in re.Matches(name))
      {
        if (m.Index > lastend)
        {
          tokens.Add(name.Substring(lastend, m.Index - lastend));
        }
        tokens.Add(m.Value);
        lastend = m.Index + m.Length;
      }
      
      tokens.Add(name.Substring(lastend));

      return tokens.ToArray();
    }
  }
	/// <summary>
	/// Implementation of the Boyer Moore string search algorithm.
	/// </summary>
	/// <example>
	/// <code>
	/// int index = 0, start = 0;
	/// string sub = "hello", text = "to the hello world I say hello";
	/// 
	/// while (index > -1)
	/// {
	///		index = BoyerMoore.IndexOf(sub, text, start);
	///		Console.WriteLine(index);
	///		start += index + 1;
	///	}
	/// </code>
	/// </example>
	public sealed class BoyerMoore
	{
    static string     temppat = string.Empty, revpat = string.Empty;
    static Dictionary<char, int> tempjmp, revjmp;

		BoyerMoore(){}

    /// <summary>
    /// Finds a substring in text searching from the front.
    /// </summary>
    /// <param name="pattern">the substring to search for</param>
    /// <param name="text">the text to search in</param>
    /// <returns>the start index of the substring if found, else -1 if not found</returns>
    public static int IndexOf(string pattern, string text)
    {
      return IndexOf(pattern, text, 0);
    }

    /// <summary>
    /// Finds a substring in text searching from the front and starting from start.
    /// </summary>
    /// <param name="pattern">the substring to search for</param>
    /// <param name="text">the text to search in</param>
    /// <param name="start">the start index of 'text' where to search from</param>
    /// <returns>the start index of the substring if found, else -1 if not found</returns>
    public static int IndexOf(string pattern, string text, int start)
    {
      if (temppat != pattern)
      {
        tempjmp = PreIndexOf(temppat = pattern);
      }

      return IndexOf(pattern, text, start, tempjmp);
    }

    static Dictionary<char, int> PreIndexOf(string pattern)
		{
			int m = pattern.Length - 1;
      Dictionary<char, int> pa = new Dictionary<char, int>(m * 3);

			for (int i = 0; i <= m; i++)
			{
				pa[pattern[i]] = m - i;
			}

			return pa;
		}

    static int IndexOf(string pattern, string text, int start, Dictionary<char, int> jmptbl)
		{
			/*const*/int	n		= text.Length,
			/*const*/			m		= pattern.Length,
			/*const*/			far	= m - 1;
			int index = start;

			if (m == 0 || start >= n)
			{
				return -1;
			}

			while (index + m <= n)
			{
				char t = text[far + index];
				if (t == pattern[far])
				{
					if (far == 0)
					{
						return index;
					}

					int near = far - 1;
					while ((t = text[near + index]) == pattern[near])
					{
						if (near-- == 0)
						{
							return index;
						}
					}
					index++;
				}
				if (jmptbl.ContainsKey(t))
				{
					index += jmptbl[t];
				}
				else
				{
					index += m;
				}
			}
			return -1;
		}

    /// <summary>
    /// Finds a substring in text searching from the rear.
    /// </summary>
    /// <param name="pattern">the substring to search for</param>
    /// <param name="text">the text to search in</param>
    /// <returns>the start index of the substring if found, else -1 if not found</returns>
    public static int LastIndexOf(string pattern, string text)
    {
      return LastIndexOf(pattern, text, text.Length);
    }

    /// <summary>
    /// Finds a substring in text searching from the rear and starting at start.
    /// </summary>
    /// <param name="pattern">the substring to search for</param>
    /// <param name="text">the text to search in</param>
    /// <param name="start">the end index of 'text' where to search from</param>
    /// <returns>the start index of the substring if found, else -1 if not found</returns>
    public static int LastIndexOf(string pattern, string text, int start)
    {
      if (revpat != pattern)
      {
        revjmp = PreLastIndexOf(revpat = pattern);
      }

      return LastIndexOf(pattern, text, start, revjmp);
    }


    static Dictionary<char, int> PreLastIndexOf(string pattern)
		{
			int m = pattern.Length - 1;
      Dictionary<char, int> pa = new Dictionary<char, int>(m * 3);

			for (int i = m; i >= 0; i--)
			{
				pa[pattern[i]] = i;
			}

			return pa;
		}


    static int LastIndexOf(string pattern, string text, int start, Dictionary<char, int> jmptbl)
		{
			/*const*/int	n		= text.Length < start ? text.Length : start,
			/*const*/			m		= pattern.Length;
			int index = n - m;
			char f = pattern[0];

			if (m == 0 || n < 0)
			{
				return -1;
			}

			while (index >=  0)
			{
				char t = text[index];
				if (t == f)
				{
					int near = 1;
					while ((t = text[near + index]) == pattern[near])
					{
						if (++near >= m)
						{
							return index;
						}
					}
					index--;
				}
        if (jmptbl.ContainsKey(t))
				{
					index -= jmptbl[t];
				}
				else
				{
					index -= m;
				}
			}
			return -1;
		}
	}
}
