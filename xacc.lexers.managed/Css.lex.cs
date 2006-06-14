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
	YY_BOL = 128;
	YY_EOF = 129;

	}

	const int ML_COMMENT = 3;
	const int INDEF = 2;
	const int INCLASS = 1;
	const int YYINITIAL = 0;
	static readonly int[] yy_state_dtrans = {
		0, 		33, 		36, 		38
	};
	static readonly int[] yy_acpt = {
0,
4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,
4,4,4,4,4,4,4,4,0,4,4,4,4,4,4,4,
0,4,4,0,4,0,0	};
	static readonly int[] yy_cmap = unpackFromString(
1,130,
"4:9,1,8,4,10,9,4:18,1,4:2,15,4:6,3,4:2,12,16,2,12:10,6,13,4:5,14,11:25,4:4," +
"11,4,14,11:25,5,4,7,4:2,0:2")[0];

	static readonly int[] yy_rmap = {
0,1,2,3,1,1,4,1,5,6,7,8,1,1,1,9,1,10,1,1,1,11,12,13,1,14,1,15,16,1,17,11,18,19,20,11,21,22,23,13};

	static readonly int[,] yy_nxt = {
{1,2,3,26,26,4,26,26,5,27,5,6,26,26,28,34,37},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,6,6,-1,6,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,8,-1,8,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,9,-1,9,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,10,-1,10,-1,-1},
{-1,-1,-1,16,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,15,15,-1,15,-1,-1},
{-1,-1,31,21,31,31,31,-1,-1,31,31,31,31,-1,31,31,31},
{-1,-1,31,31,31,31,31,-1,-1,31,31,31,31,-1,31,31,31},
{-1,-1,22,-1,22,22,22,22,-1,22,22,22,22,22,22,22,22},
{-1,-1,24,39,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,10,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,5,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,25,-1,-1,-1,-1,6,6,-1,6,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,14,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,31,31,31,31,31,-1,19,31,31,31,31,-1,31,31,31},
{1,2,11,29,29,29,12,13,14,30,14,15,29,29,15,29,29},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,8,-1,-1,8,-1,-1},
{1,2,17,31,31,31,31,18,19,32,35,31,31,20,31,31,31},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,9,-1,-1,9,-1,-1},
{1,2,22,23,22,22,22,22,-1,22,22,22,22,22,22,22,22}};

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
					case 15: 
                                   { return NUMBER;}
						break;
					case 23: 
              { return COMMENT;}
						break;
					case 22: 
                       { return COMMENT;}
						break;
					case 6: case 28: 
                                     { return STRING;}
						break;
					case 10: 
                                         { return STRING;}
						break;
					case 24: 
                   { EXIT(); return COMMENT;}
						break;
					case 13: 
            { EXIT(); return OPERATOR; }
						break;
					case 18: 
          { EXIT(); EXIT(); return OPERATOR;}
						break;
					case 11: case 29: 
          { return PLAIN;}
						break;
					case 7: 
               { ENTER(ML_COMMENT); return COMMENT; }
						break;
					case 21: 
           { ENTER(ML_COMMENT); return COMMENT; }
						break;
					case 14: case 30: 
                      { return NEWLINE;}
						break;
					case 5: case 27: 
                        { return NEWLINE;}
						break;
					case 17: case 31: 
                   { return KEYWORD;}
						break;
					case 19: case 32: case 35: 
                    { return NEWLINE;}
						break;
					case 3: case 26: case 34: case 37: 
            { return PLAIN;}
						break;
					case 20: 
          { EXIT(); return OPERATOR; }
						break;
					case 8: 
                                        { return STRING;}
						break;
					case 9: 
                                        { return STRING;}
						break;
					case 16: 
             { ENTER(ML_COMMENT); return COMMENT; }
						break;
					case 12: 
            { ENTER(INDEF); return OPERATOR;}
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
