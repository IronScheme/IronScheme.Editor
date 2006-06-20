// Gardens Point Parser Generator (Runtime component)
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)


namespace gppg
{
  public class Rule
  {
    public int lhs; // symbol
    public int[] rhs; // symbols

    public Rule(int lhs, int[] rhs)
    {
      this.lhs = lhs;
      this.rhs = rhs;
    }
  }
}
