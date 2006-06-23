// Gardens Point Parser Generator (Runtime component)
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)


namespace gppg
{
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
        System.Array.Copy(array, newarray, top);
        array = newarray;
      }
      array[top++] = value;
    }

    public T Pop()
    {
      return array[--top];
    }

    public T Top()
    {
      return array[top - 1];
    }

    public bool IsEmpty()
    {
      return top == 0;
    }
  }
}