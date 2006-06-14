using System;

namespace CS_Lex
{
  /// <summary>
  /// Summary description for CAcceptAnchor.
  /// </summary>
  /***************************************************************
  Class: CAcceptAnchor
  **************************************************************/
  class CAcceptAnchor
  {
    /***************************************************************
          Member Variables
          **************************************************************/
    CAccept m_accept;
    int m_anchor;

    /***************************************************************
          Function: CAcceptAnchor
          **************************************************************/
    CAcceptAnchor
      (
      )
    {
      m_accept = null;
      m_anchor = CSpec.NONE;
    }
  }

}
