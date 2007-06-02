// Gardens Point Parser Generator (Runtime component)
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)
using System;

namespace Xacc.Languages.gppg
{
  [System.CLSCompliant(false)]
  public class ParserStack<T>
  {
    public T[] array;
    public int top = 0;

    public ParserStack(int capacity)
    {
      array = new T[capacity];
    }

    public void Push(T value)
    {
      if (top >= array.Length)
      {
        T[] newarray = new T[array.Length * 2];
        Array.Copy(array, newarray, top);
        array = newarray;
      }
      array[top++] = value;
    }

    public T Pop()
    {
      T res = array[--top];
      array[top] = default(T);
      return res;
    }

    public T Top()
    {
      return Top(0);
    }

    public T Top(int i)
    {
      return array[top - 1 - i];
    }

    public bool IsEmpty()
    {
      return top == 0;
    }

    public ParserStack<T> Clone()
    {
      ParserStack<T> c = new ParserStack<T>(array.Length);
      Array.Copy(array, c.array, array.Length);
      c.top = top;

      return c;
    }

    public void Clear()
    {
      Array.Clear(array, 0, top);
      top = 0;
    }
  }
}