using Xacc.Languages.CSLex;
using Xacc.ComponentModel;
using System.Drawing;
namespace Xacc.Languages
{
  sealed class CssLang : CSLex.Language
  {
	  public override string Name {get {return "CSS"; } }
	  public override string[] Extensions {get { return new string[]{"css"}; } }
	  LexerBase lexer = new CssLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks


sealed class CssLexer : LexerBase {

	public CssLexer () {
	YY_BOL = 65536;
	YY_EOF = 65537;

	}

	const int ML_COMMENT = 3;
	const int INDEF = 2;
	const int INCLASS = 1;
	const int YYINITIAL = 0;
	static readonly int[] yy_state_dtrans = {
		0, 		28, 		31, 		33
	};
	static readonly int[] yy_acpt = {
0,
4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,
4,4,4,4,4,0,4,4,4,4,4,0,4,4,0,4,
0,4,0	};
	static readonly int[] yy_cmap = unpackFromString(
1,65538,
"4:9,1,14,4,16,15,4:18,1,4:2,12,4:6,3,4:2,9,13,2,9:10,6,10,4:5,11,8:25,4:4,8" +
",4,11,8:25,5,4,7,4:65410,0:2")[0];

	static readonly int[] yy_rmap = {
0,1,2,3,1,4,1,1,5,6,7,1,1,8,1,9,1,1,10,11,12,1,13,1,14,15,10,16,17,18,19,20,21,22,23,12};

	static readonly int[,] yy_nxt = {
{1,2,3,23,23,4,23,23,5,23,23,24,29,32,6,25,6},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,5,5,-1,5,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,8,8,-1,8,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,9,9,-1,9,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,10,10,-1,10,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,13,13,-1,13,-1,-1,-1,-1,-1},
{-1,-1,26,18,26,26,26,-1,26,26,-1,26,26,26,-1,26,26},
{-1,-1,26,26,26,26,26,-1,26,26,-1,26,26,26,-1,26,26},
{-1,-1,19,-1,19,19,19,19,19,19,19,19,19,19,-1,19,19},
{-1,-1,21,35,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,10,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,22,-1,5,5,-1,5,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,-1,-1},
{-1,-1,19,-1,19,19,19,19,19,19,19,19,19,19,6,19,19},
{1,2,34,23,23,23,11,12,13,23,23,13,23,23,6,25,6},
{-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,8,-1,-1,-1,-1,-1},
{-1,-1,26,26,26,26,26,-1,26,26,-1,26,26,26,6,26,26},
{1,2,15,26,26,26,26,16,26,26,17,26,26,26,6,30,26},
{-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,9,-1,-1,-1,-1,-1},
{1,2,19,20,19,19,19,19,19,19,19,19,19,19,6,27,19},
{-1,-1,-1,14,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}};

	public override Yytoken yylex ()
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
					case 15: case 26: case 30: 
                   { return KEYWORD;}
						break;
					case 6: case 25: 
             { return NEWLINE;}
						break;
					case 10: 
                                         { return STRING;}
						break;
					case 13: 
                                   { return NUMBER;}
						break;
					case 18: 
           { ENTER(ML_COMMENT); return COMMENT; }
						break;
					case 11: 
            { ENTER(INDEF); return OPERATOR;}
						break;
					case 7: 
               { ENTER(ML_COMMENT); return COMMENT; }
						break;
					case 21: 
                   { EXIT(); return COMMENT;}
						break;
					case 14: 
             { ENTER(ML_COMMENT); return COMMENT; }
						break;
					case 5: case 24: 
                                     { return STRING;}
						break;
					case 17: 
          { EXIT(); return OPERATOR; }
						break;
					case 19: case 27: 
                       { return COMMENT;}
						break;
					case 3: case 23: case 29: case 32: case 34: 
 { return PLAIN;}
						break;
					case 20: 
              { return COMMENT;}
						break;
					case 8: 
                                        { return STRING;}
						break;
					case 9: 
                                        { return STRING;}
						break;
					case 16: 
          { EXIT(); EXIT(); return OPERATOR;}
						break;
					case 12: 
            { EXIT(); return OPERATOR; }
						break;
					case 1: 

						break;
					case 2: 
        {;}
						break;
					case 4: 
              { ENTER(INCLASS); return OPERATOR;}
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
