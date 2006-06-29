using Xacc.Languages.CSLex;
using Xacc.ComponentModel;
using System.Drawing;
using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;
namespace Xacc.Languages
{
  sealed class RubyLang : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "Ruby"; } }
	  public override string[] Extensions {get { return new string[]{"rb"}; } }
	  LexerBase lexer = new RubyLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks


sealed class RubyLexer : LexerBase {

	public RubyLexer () {
	YY_BOL = 256;
	YY_EOF = 257;

	}

	const int ML_COMMENT = 1;
	const int YYINITIAL = 0;
	static readonly int[] yy_state_dtrans = {
		0, 		71
	};
	static readonly int[] yy_acpt = {
0,
4,4,4,4,4,4,4,4,4,4,4,4,1,4,4,1,
0,4,4,4,4,4,4,4,4,4,0,4,4,4,4,4,
4,4,0,4,4,4,0,4,4,0,4,4,0,4,4,0,
4,4,0,4,0,4,0,4,0,4,0,4,0,4,0,4,
0,4,0,4,0,4,0,4,0,4,0,4,0,4,4,4,
4,4,0,4,4,0,4,4,4,4,4,4,4,4,4,4,
4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,
4,4,4,4,4,4,4,4,4,4,4,4,0,4,4,0,
4,0,4,0,4,4,4,4,4,4,4,4,4,4,4,4,
4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,
0,0,4,4,4,0,0,4,4,4,4,4,0,4,0,0,
4,0,0,4,4,4,4,4,4,4	};
	static readonly int[] yy_cmap = unpackFromString(
1,258,
"54:9,1,2,54:2,48,54:18,1,51,50,12,54,51:2,47,51:3,44,51,44,46,51,41,39:9,51" +
":3,3,51:2,53,43,16,43,26,17,35,18,52,19,52:2,36,45,20,52:6,40,52:2,42,52:2," +
"51,11,51:2,34,54,13,4,23,10,5,24,6,32,7,52,22,14,37,8,25,28,38,21,15,30,27," +
"49,33,29,31,52,51:4,54:129,9,0")[0];

	static readonly int[] yy_rmap = {
0,1,2,1,1,3,4,5,6,7,1,1,1,1,8,9,1,10,11,12,13,14,15,16,17,18,19,20,21,9,22,23,1,24,25,13,26,27,28,29,30,1,31,32,33,34,35,23,16,36,27,37,38,39,40,41,42,20,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85
,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,17,109,110,111,112,113,114,115,44,116,117,118,119,107,120,121,122,123,124,125,126,127,128,129,130,131,132,133,134,135,136,137,138,139,140,107,141,142,143,144,145,146,147,147,148,149,150,151,152,153,154,155,156,157,158,159,160,161,162,163,164,165,166};

	static readonly int[,] yy_nxt = {
{1,2,3,4,5,79,159,21,84,18,30,4,6,87,121,126,164,89,159,159,159,129,159,131,91,36,159,168,159,159,133,169,159,134,170,159,159,171,159,7,159,22,159,159,4,159,20,8,-1,159,23,4,159,32,32},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,135,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,136,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,6,-1,6,6,6,6,6,6,-1,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6},
{-1,-1,-1,-1,-1,27,-1,-1,-1,-1,10,-1,-1,-1,31,-1,-1,27,-1,-1,-1,-1,-1,-1,10,-1,10,37,-1,-1,-1,-1,-1,-1,-1,10,31,10,-1,7,37,7,-1,-1,-1,10,35,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,42,-1,42,42,42,42,42,42,-1,42,45,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,-1,42,42,42,42,42,42,42},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,148,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,14,-1,-1,14,14,14,14,14,-1,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14},
{-1,15,-1,-1,15,15,15,15,15,-1,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15},
{-1,-1,-1,-1,53,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,17,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,19,-1,-1,14,14,14,14,14,-1,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,25,-1,25,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,9,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,122,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,-1,27,-1,-1,-1,-1,10,-1,-1,-1,31,-1,-1,27,-1,-1,-1,-1,-1,-1,10,-1,10,37,-1,39,-1,-1,-1,-1,-1,10,31,10,-1,7,37,7,39,-1,-1,10,35,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,48,-1,48,48,48,48,48,48,-1,48,51,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,48,11,48,48,48,48},
{-1,-1,-1,-1,159,159,159,159,159,-1,122,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,-1,57,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,57,-1,-1,-1,-1,-1,-1,10,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,10,-1,25,-1,25,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,26,-1,6,26,26,26,26,26,-1,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,33,-1,33,-1,-1,55,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,73,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,49,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,122,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,41,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,41,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,10,-1,33,-1,33,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,34,-1,42,34,34,34,34,34,-1,34,81,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,34,14,34,34,34,34,34,34,34},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,122,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,41,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,41,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,38,-1,48,38,38,38,38,38,-1,38,85,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,38,14,38,38,38,38},
{-1,-1,-1,-1,44,44,-1,-1,-1,-1,44,-1,-1,44,-1,-1,44,44,-1,-1,-1,-1,-1,44,44,-1,44,-1,-1,-1,-1,-1,-1,-1,-1,44,-1,-1,-1,44,-1,44,-1,44,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,122,-1,-1,159,159,147,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,42,-1,42,42,42,42,42,42,-1,42,45,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,42,12,42,42,42,42,42,42,42},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,122,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,44,44,-1,-1,-1,-1,44,-1,-1,44,47,-1,44,44,-1,-1,-1,-1,-1,44,44,-1,44,50,-1,-1,-1,-1,-1,-1,-1,44,47,-1,-1,44,50,44,-1,44,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,42,-1,-1,-1,42,-1,-1,42,-1,42,-1,-1,-1,-1,-1,-1,-1,42,-1,-1,42,-1,-1,59,-1,61,42,-1,-1,-1,-1,-1,-1,-1,-1,-1,161,42,-1,-1,-1,-1,-1,42,-1,42,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,122,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,80,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,48,-1,-1,-1,48,-1,-1,48,-1,48,-1,-1,-1,-1,-1,-1,-1,48,-1,-1,48,-1,-1,166,-1,63,48,-1,-1,-1,-1,-1,-1,-1,-1,-1,179,48,-1,-1,-1,-1,-1,-1,-1,48,48,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,122,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,-1,65,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,122,159,82,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,33,-1,33,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,122,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,122,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,125,125,-1,-1,-1,-1,125,-1,-1,125,-1,-1,125,125,-1,-1,-1,-1,-1,125,125,-1,125,-1,-1,-1,-1,-1,-1,-1,-1,125,-1,-1,-1,125,-1,125,-1,125,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,122,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,42,42,-1,-1,-1,-1,42,-1,-1,42,-1,-1,42,42,-1,-1,-1,-1,-1,42,42,-1,42,-1,-1,-1,-1,-1,-1,-1,-1,42,-1,-1,-1,42,-1,42,-1,42,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,122,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,48,48,-1,-1,-1,-1,48,-1,-1,48,-1,-1,48,48,-1,-1,-1,-1,-1,48,48,-1,48,-1,-1,-1,-1,-1,-1,-1,-1,48,-1,-1,-1,48,-1,48,-1,48,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,122,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,-1,-1,67,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,122,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,-1,-1,-1,69,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,122,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,13,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,122,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{1,19,3,29,14,14,14,14,14,28,14,14,26,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,34,14,14,38,14,14,14,14},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,122,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,-1,75,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,122,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,77,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,24,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,16,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,122,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,40,-1,159,-1,-1,159,92,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,117,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,14,-1,-1,34,14,14,14,34,-1,14,34,14,34,14,14,14,14,14,14,14,34,14,14,34,14,14,172,14,88,34,14,14,14,14,14,14,14,14,14,185,34,14,14,14,14,14,34,14,34,14,14,14,14,14},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,122,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,61,61,-1,-1,-1,-1,61,-1,-1,61,-1,-1,61,61,-1,-1,-1,-1,-1,61,61,-1,61,-1,-1,-1,-1,-1,-1,-1,-1,61,-1,-1,-1,61,-1,61,-1,61,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,93,159,43,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,46,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,14,-1,-1,38,14,14,14,38,-1,14,38,14,38,14,14,14,14,14,14,14,38,14,14,38,14,14,174,14,90,38,14,14,14,14,14,14,14,14,14,186,38,14,14,14,14,14,14,14,38,38,14,14,14,14},
{-1,-1,-1,-1,63,63,-1,-1,-1,-1,63,-1,-1,63,-1,-1,63,63,-1,-1,-1,-1,-1,63,63,-1,63,-1,-1,-1,-1,-1,-1,-1,-1,63,-1,-1,-1,63,-1,63,-1,63,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,124,-1,159,-1,-1,159,138,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,14,-1,-1,34,34,14,14,14,-1,34,14,14,34,14,14,34,34,14,14,14,14,14,34,34,14,34,14,14,14,14,14,14,14,14,34,14,14,14,34,14,34,14,34,14,14,14,14,14,14,14,14,14,14,14},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,52,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,14,-1,-1,38,38,14,14,14,-1,38,14,14,38,14,14,38,38,14,14,14,14,14,38,38,14,38,14,14,14,14,14,14,14,14,38,14,14,14,38,14,38,14,38,14,14,14,14,14,14,14,14,14,14,14},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,98,159,159,159,159,159,159,159,159,159,159,159,36,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,54,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,46,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,124,159,159,159,159,159,159,159,159,159,159,159,56,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,82,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,58,-1,-1,159,159,149,159,159,159,159,159,159,159,159,159,159,159,159,159,159,107,159,159,159,137,159,159,159,150,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,60,159,159,159,159,159,159,159,159,159,159,159,159,159,159,109,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,43,159,-1,159,-1,-1,159,108,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,60,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,62,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,113,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,62,159,115,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,62,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,64,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,66,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,36,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,68,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,70,159,159,159,159,159,118,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,60,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,72,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,66,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,82,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,43,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,74,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,124,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,60,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,60,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,76,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,62,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,60,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,78,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,94,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,14,-1,-1,88,88,14,14,14,-1,88,14,14,88,14,14,88,88,14,14,14,14,14,88,88,14,88,14,14,14,14,14,14,14,14,88,14,14,14,88,14,88,14,88,14,14,14,14,14,14,14,14,14,14,14},
{-1,-1,-1,-1,83,83,-1,-1,-1,-1,83,-1,-1,83,-1,-1,83,83,-1,-1,-1,-1,-1,83,83,-1,83,-1,-1,-1,-1,-1,-1,-1,-1,83,-1,-1,-1,83,-1,83,-1,83,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,95,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,139,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,14,-1,-1,90,90,14,14,14,-1,90,14,14,90,14,14,90,90,14,14,14,14,14,90,90,14,90,14,14,14,14,14,14,14,14,90,14,14,14,90,14,90,14,90,14,14,14,14,14,14,14,14,14,14,14},
{-1,-1,-1,-1,173,173,-1,-1,-1,-1,173,-1,-1,173,-1,-1,173,173,-1,-1,-1,-1,-1,173,173,-1,173,-1,-1,-1,-1,-1,-1,-1,-1,173,-1,-1,-1,173,-1,173,-1,173,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,96,159,159,159,-1,159,-1,-1,141,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,86,86,-1,-1,-1,-1,86,-1,-1,86,-1,-1,86,86,-1,-1,-1,-1,-1,86,86,-1,86,-1,-1,-1,-1,-1,-1,-1,-1,86,-1,-1,-1,86,-1,86,-1,86,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,97,142,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,99,159,159,159,159,159,159,159,159,159,159,100,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,101,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,102,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,103,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,104,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,105,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,106,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,108,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,110,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,111,-1,-1,159,151,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,112,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,114,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,152,163,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,153,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,116,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,154,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,99,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,155,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,110,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,156,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,115,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,119,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,116,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,158,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,158,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,120,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,14,-1,-1,123,123,14,14,14,-1,123,14,14,123,14,14,123,123,14,14,14,14,14,123,123,14,123,14,14,14,14,14,14,14,14,123,14,14,14,123,14,123,14,123,14,14,14,14,14,14,14,14,14,14,14},
{-1,-1,-1,-1,128,128,-1,-1,-1,-1,128,-1,-1,128,-1,-1,128,128,-1,-1,-1,-1,-1,128,128,-1,128,-1,-1,-1,-1,-1,-1,-1,-1,128,-1,-1,-1,128,-1,128,-1,128,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,132,132,-1,-1,-1,-1,132,-1,-1,132,-1,-1,132,132,-1,-1,-1,-1,-1,132,132,-1,132,-1,-1,-1,-1,-1,-1,-1,-1,132,-1,-1,-1,132,-1,132,-1,132,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,157,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,140,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,14,-1,-1,127,127,14,14,14,-1,127,14,14,127,14,14,127,127,14,14,14,14,14,127,127,14,127,14,14,14,14,14,14,14,14,127,14,14,14,127,14,127,14,127,14,14,14,14,14,14,14,14,14,14,14},
{-1,-1,-1,-1,130,130,-1,-1,-1,-1,130,-1,-1,130,-1,-1,130,130,-1,-1,-1,-1,-1,130,130,-1,130,-1,-1,-1,-1,-1,-1,-1,-1,130,-1,-1,-1,130,-1,130,-1,130,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,159,159,159,159,143,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,144,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,159,145,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,-1,-1,-1,159,159,159,159,159,-1,159,-1,-1,159,159,159,159,159,159,159,159,159,159,159,159,146,159,159,159,159,159,159,159,159,137,159,159,159,159,137,159,137,159,159,-1,159,-1,-1,-1,159,-1,-1,159,137,-1},
{-1,14,-1,-1,160,160,14,14,14,-1,160,14,14,160,14,14,160,160,14,14,14,14,14,160,160,14,160,14,14,14,14,14,14,14,14,160,14,14,14,160,14,160,14,160,14,14,14,14,14,14,14,14,14,14,14},
{-1,-1,-1,-1,162,162,-1,-1,-1,-1,162,-1,-1,162,-1,-1,162,162,-1,-1,-1,-1,-1,162,162,-1,162,-1,-1,-1,-1,-1,-1,-1,-1,162,-1,-1,-1,162,-1,162,-1,162,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,14,-1,-1,165,165,14,14,14,-1,165,14,14,165,14,14,165,165,14,14,14,14,14,165,165,14,165,14,14,14,14,14,14,14,14,165,14,14,14,165,14,165,14,165,14,14,14,14,14,14,14,14,14,14,14},
{-1,-1,-1,-1,176,176,-1,-1,-1,-1,176,-1,-1,176,-1,-1,176,176,-1,-1,-1,-1,-1,176,176,-1,176,-1,-1,-1,-1,-1,-1,-1,-1,176,-1,-1,-1,176,-1,176,-1,176,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,167,167,-1,-1,-1,-1,167,-1,-1,167,-1,-1,167,167,-1,-1,-1,-1,-1,167,167,-1,167,-1,-1,-1,-1,-1,-1,-1,-1,167,-1,-1,-1,167,-1,167,-1,167,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,14,-1,-1,172,172,14,14,14,-1,172,14,14,172,14,14,172,172,14,14,14,14,14,172,172,14,172,14,14,14,14,14,14,14,14,172,14,14,14,172,14,172,14,172,14,14,14,14,14,14,14,14,14,14,14},
{-1,-1,-1,-1,175,175,-1,-1,-1,-1,175,-1,-1,175,-1,-1,175,175,-1,-1,-1,-1,-1,175,175,-1,175,-1,-1,-1,-1,-1,-1,-1,-1,175,-1,-1,-1,175,-1,175,-1,175,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,178,178,-1,-1,-1,-1,178,-1,-1,178,-1,-1,178,178,-1,-1,-1,-1,-1,178,178,-1,178,-1,-1,-1,-1,-1,-1,-1,-1,178,-1,-1,-1,178,-1,178,-1,178,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,14,-1,-1,174,174,14,14,14,-1,174,14,14,174,14,14,174,174,14,14,14,14,14,174,174,14,174,14,14,14,14,14,14,14,14,174,14,14,14,174,14,174,14,174,14,14,14,14,14,14,14,14,14,14,14},
{-1,14,-1,-1,177,177,14,14,14,-1,177,14,14,177,14,14,177,177,14,14,14,14,14,177,177,14,177,14,14,14,14,14,14,14,14,177,14,14,14,177,14,177,14,177,14,14,14,14,14,14,14,14,14,14,14},
{-1,14,-1,-1,180,180,14,14,14,-1,180,14,14,180,14,14,180,180,14,14,14,14,14,180,180,14,180,14,14,14,14,14,14,14,14,180,14,14,14,180,14,180,14,180,14,14,14,14,14,14,14,14,14,14,14},
{-1,14,-1,-1,181,181,14,14,14,-1,181,14,14,181,14,14,181,181,14,14,14,14,14,181,181,14,181,14,14,14,14,14,14,14,14,181,14,14,14,181,14,181,14,181,14,14,14,14,14,14,14,14,14,14,14},
{-1,14,-1,-1,182,182,14,14,14,-1,182,14,14,182,14,14,182,182,14,14,14,14,14,182,182,14,182,14,14,14,14,14,14,14,14,182,14,14,14,182,14,182,14,182,14,14,14,14,14,14,14,14,14,14,14},
{-1,14,-1,-1,183,183,14,14,14,-1,183,14,14,183,14,14,183,183,14,14,14,14,14,183,183,14,183,14,14,14,14,14,14,14,14,183,14,14,14,183,14,183,14,183,14,14,14,14,14,14,14,14,14,14,14},
{-1,14,-1,-1,184,184,14,14,14,-1,184,14,14,184,14,14,184,184,14,14,14,14,14,184,184,14,184,14,14,14,14,14,14,14,14,184,14,14,14,184,14,184,14,184,14,14,14,14,14,14,14,14,14,14,14}};

	public override IToken lex ()
 {
		int yy_lookahead;
		int yy_anchor = YY_NO_ANCHOR;
		int yy_state = yy_state_dtrans[yy_lexical_state];
		int yy_next_state = YY_NO_STATE;
		int yy_last_accept_state = YY_NO_STATE;
		bool yy_initial = true;
		int yy_this_accept;

		yy_mark_start();
		yy_this_accept = yy_acpt[yy_state];
		if (YY_NOT_ACCEPT != yy_this_accept) {
			yy_last_accept_state = yy_state;
			yy_mark_end();
		}
		while (true) {
			if (yy_initial && yy_at_bol) yy_lookahead = YY_BOL;
			else yy_lookahead = yy_advance();
			yy_next_state = YY_F;
			yy_next_state = yy_nxt[yy_rmap[yy_state],yy_cmap[yy_lookahead]];
			if (YY_EOF == yy_lookahead && true == yy_initial) {
				return Yytoken.EOF;
			}
			if (YY_F != yy_next_state) {
				yy_state = yy_next_state;
				yy_initial = false;
				yy_this_accept = yy_acpt[yy_state];
				if (YY_NOT_ACCEPT != yy_this_accept) {
					yy_last_accept_state = yy_state;
					yy_mark_end();
				}
			}
			else {
				if (YY_NO_STATE == yy_last_accept_state) {
					throw (new System.Exception("Lexical Error: Unmatched Input."));
				}
				else {
					yy_anchor = yy_acpt[yy_last_accept_state];
					if (0 != (YY_END & yy_anchor)) {
						yy_move_end();
					}
					yy_to_mark();
					switch (yy_last_accept_state) {
					case 8: case 23: case 32: 
 { return Plain(); }
						break;
					case 4: case 20: case 29: 
                                       { return Operator(); }
						break;
					case 10: case 25: case 33: 
                                                                                                             { return Number(); }
						break;
					case 13: 
                    { ENTER(ML_COMMENT); return Comment(); }
						break;
					case 1: case 18: case 28: 

						break;
					case 16: 
                   { EXIT(); return Comment();  }
						break;
					case 15: 
                      { return Comment(); }
						break;
					case 2: case 19: 
      { ; }
						break;
					case 9: case 24: case 80: case 122: 
                                                                                                                                                                                                                                                                                        { return Keyword(); }
						break;
					case 7: case 22: case 31: case 37: case 41: case 44: case 47: case 50: 
					                                                                                  { return Number(); }
						break;
					case 5: case 21: case 30: case 36: case 40: case 43: case 46: case 49: 
					case 52: case 54: case 56: case 58: case 60: case 62: case 64: case 66: 
					case 68: case 70: case 72: case 74: case 76: case 78: case 79: case 82: 
					case 84: case 87: case 89: case 91: case 92: case 93: case 94: case 95: 
					case 96: case 97: case 98: case 99: case 100: case 101: case 102: case 103: 
					case 104: case 105: case 106: case 107: case 108: case 109: case 110: case 111: 
					case 112: case 113: case 114: case 115: case 116: case 117: case 118: case 119: 
					case 120: case 121: case 124: case 126: case 129: case 131: case 133: case 134: 
					case 135: case 136: case 137: case 138: case 139: case 140: case 141: case 142: 
					case 143: case 144: case 145: case 146: case 147: case 148: case 149: case 150: 
					case 151: case 152: case 153: case 154: case 155: case 156: case 157: case 158: 
					case 159: case 163: case 164: case 168: case 169: case 170: case 171: 
                                       { return Plain(); }
						break;
					case 12: 
                                                                                                                                                                                                                                  { return String(); }
						break;
					case 3: 
  { return NewLine();}
						break;
					case 11: 
                                                                                                                                                                                                                                      { return String(); }
						break;
					case 14: case 26: case 34: case 38: case 81: case 85: case 88: case 90: 
					case 123: case 127: case 160: case 165: case 172: case 174: case 177: case 180: 
					case 181: case 182: case 183: case 184: case 185: case 186: 
                   { return Comment(); }
						break;
					case 6: 
         { return Comment(); }
						break;
					default:
						yy_error(YY_E_INTERNAL,false);break;
					}
					yy_initial = true;
					yy_state = yy_state_dtrans[yy_lexical_state];
					yy_next_state = YY_NO_STATE;
					yy_last_accept_state = YY_NO_STATE;
					yy_mark_start();
					yy_this_accept = yy_acpt[yy_state];
					if (YY_NOT_ACCEPT != yy_this_accept) {
						yy_last_accept_state = yy_state;
						yy_mark_end();
					}
				}
			}
		}
	}
}
