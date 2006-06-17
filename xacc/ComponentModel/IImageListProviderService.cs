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
using System.Collections.Specialized;
using System.IO;
using System.Drawing;
using Xacc.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using Xacc.Controls;

using SR = System.Resources;
#endregion


namespace Xacc.ComponentModel
{
	/// <summary>
	/// Provides services to associate images with .NET objects
	/// </summary>
	[Name("ImageList provider")]
	public interface IImageListProviderService : IService
	{
    /// <summary>
    /// Adds a type to the provider
    /// </summary>
    /// <param name="type">the type</param>
		void				Add													(Type type);

    /// <summary>
    /// Adds a type to the provider
    /// </summary>
    /// <param name="type">the type</param>
    /// <param name="img">the image</param>
    void				Add													(Type type, Image img);

    /// <summary>
    /// Gets the imagelist storing all the images
    /// </summary>
		ImageList		ImageList										{get;}

    /// <summary>
    /// Gets the index of the image for the name
    /// </summary>
    /// <remarks>The image will be generated if it doesnt</remarks>
    int         this[string name]           {get;}

    /// <summary>
    /// Gets the index of the image for the name
    /// </summary>
    /// <remarks>The image will be created if it does exist and has not been added</remarks>
    int					this[object typeorinstance]	{get;}

    /// <summary>
    /// Gets an instance of Icon based on a image file
    /// </summary>
    /// <param name="imagefile">the name of the image file</param>
    /// <returns>the icon</returns>
    Icon        GetIcon(string imagefile);

    /// <summary>
    /// Gets an instance of Image based on a image file
    /// </summary>
    /// <param name="imagefile">the name of the image file</param>
    /// <returns>the icon</returns>
    Image GetImage(string imagefile);

	}

  /// <summary>
  /// Add image association to .NET objects/types
  /// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited=true, AllowMultiple=false)]
	public class ImageAttribute : Attribute
	{
		string path;

    /// <summary>
    /// Creates an instance of ImageAttribute
    /// </summary>
    /// <param name="resourcepath">the resource path</param>
		public ImageAttribute(string resourcepath)
		{
			path = resourcepath;
		}

    /// <summary>
    /// The resource path
    /// </summary>
		public string Path
		{
			get
			{
				return
#if VS
				"Xacc.Resources." + 
#endif
				path;
			}
		}

    /// <summary>
    /// The alternative resource path
    /// </summary>
    public string AltPath
    {
      get
      {
        return
#if !VS
          "Xacc.Resources." + 
#endif
          path;
      }
    }
	}

	sealed class ImageListProvider : ServiceBase, IImageListProviderService
	{
		readonly ImageList images = new ImageList();
		readonly Hashtable mapping = new Hashtable();
		readonly Hashtable namemap = new Hashtable();
		readonly Image Empty;

		public ImageListProvider()
		{
			images.ColorDepth = ColorDepth.Depth32Bit;
			images.ImageSize = new Size(16,16);

      Assembly ass = typeof(ImageListProvider).Assembly;

			using (Stream s = ass.GetManifestResourceStream(
#if VS
				"Xacc.Resources." + 
#endif
				"empty.png"))
			{
				Empty = Image.FromStream(s);
				images.Images.Add(Empty);
			}

      Image img = Image.FromStream(ass.GetManifestResourceStream(
#if VS
        "Xacc.Resources." + 
#endif
        "Folder.Closed.png"));

      images.Images.Add(img);

      img = Image.FromStream(ass.GetManifestResourceStream(
#if VS
        "Xacc.Resources." + 
#endif
        "Folder.Open.png"));

      images.Images.Add(img);
		}

    public Icon GetIcon(string imagefile)
    {
      try
      {
        Bitmap bmp =  images.Images[this[imagefile]] as Bitmap;
        if (bmp != null)
        {
          return Icon.FromHandle(bmp.GetHicon());
        }
      }
      catch (NotImplementedException) // MONO
      {
        return ServiceHost.Window.MainForm.Icon;
      }

      return null;
    }

    public Image GetImage(string imagefile)
    {
      Bitmap bmp = images.Images[this[imagefile]] as Bitmap;
      return bmp;
    }

    public void Add(Type type, Image img)
    {
      mapping[type] = images.Images.Count;
      images.Images.Add( img );
    }

		public void Add(Type type)
		{
			if (!mapping.ContainsKey(type))
			{
				foreach(ImageAttribute iat in type.GetCustomAttributes(typeof(ImageAttribute),true))
				{
					//the one and only, i hope;
					if (!namemap.ContainsKey(iat.Path))
					{
						//hopefully the user has an image
						Stream ms = type.Assembly.GetManifestResourceStream(iat.Path);
            if (ms == null)
            {
              ms = type.Assembly.GetManifestResourceStream(iat.AltPath);
            }
						if (ms == null)
						{
							foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
							{
                if (ass == type.Assembly)
                {
                  continue;
                }

                try
                {
                  ms = ass.GetManifestResourceStream(iat.Path);
                  if (ms != null)
                  {
                    break;
                  }
                }
                catch
                {
                }
                try
                {
                  ms = ass.GetManifestResourceStream(iat.AltPath);
                  if (ms != null)
                  {
                    break;
                  }
                }
                catch
                {
                }
							}
						}

      			if (ms != null)
						{
							mapping.Add(type, images.Images.Count);
							images.Images.Add( Image.FromStream( ms, true));
							namemap.Add(iat.Path, mapping[type]);	
              return;
						}
					}
					else
					{
						mapping.Add(type, namemap[iat.Path]);
            return;
					}
				}
				//not found
        mapping.Add(type, 0);
			}
		}

		public ImageList ImageList
		{
			get	{return images;}
		}

    public int this[string name]
    {
      get
      {
        if (name == null)
        {
          return 0;
        }
        if (!namemap.ContainsKey(name))
        {
          //hopefully the user has an image
          Stream ms = null;
          foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
          {
            try
            {
              ms = ass.GetManifestResourceStream(
#if VS
                "Xacc.Resources." + 
#endif                
                name);
              if (ms != null)
              {
                break;
              }
            }
            catch
            {
            }
          }
          
          if (ms != null)
          {
            int i = images.Images.Count;
            images.Images.Add( Image.FromStream( ms, true));
            namemap.Add(name, i);	
          }
          else
          {
            namemap.Add(name, 0);	
          }
        }
        return (int) namemap[name];
      }
    }

		public int this[object typeorinstance]
		{
			get
			{
        try
        {
          if (typeorinstance == null)
          {
            return 0;
          }
          if (typeorinstance is Type)
          {
            Type t = typeorinstance as Type;
            if (!mapping.ContainsKey(t))
            {
              Add(t);
            }
            return (int)mapping[t];
          }
          else
          {
            Type t = typeorinstance.GetType();
            return this[t];
          }
        }
        catch
        {
        }
        return 0;
			}
		}
	}
}
