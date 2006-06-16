using Xacc.Languages.CSLex;
using Xacc.ComponentModel;
namespace Xacc.Languages
{
  sealed class PatchLanguage : CSLex.Language
  {
	  public override string Name {get {return "Patch"; } }
	  public override string[] Extensions {get { return new string[]{"patch", "diff"}; } }
	  LexerBase lexer = new PatchLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}


sealed class PatchLexer : LexerBase {

	public PatchLexer () {
	YY_BOL = 65536;
	YY_EOF = 65537;

	}

	const int YYINITIAL = 0;
	static readonly int[] yy_state_dtrans = {
		0
	};
	static readonly int[] yy_acpt = {
0,
4,4,4,4,4,4,4,4,0,4,4,0,4,4,4,4,
4,4,4	};
	static readonly int[] yy_cmap = unpackFromString(
1,65538,
"3:9,1,9,3:21,1,3:10,4,3,5,3:18,2,3:35,6,3,8,3:2,7,3:65430,0:2")[0];

	static readonly int[] yy_rmap = {
0,1,2,3,4,5,1,1,6,7,8,9,10,11,12,13,6,9,14,15};

	static readonly int[,] yy_nxt = {
{1,2,3,17,4,5,19,17,17,6},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,2,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,10,17,17,17,17,17,17,-1},
{-1,4,4,4,4,4,4,4,4,-1},
{-1,5,5,5,5,5,5,5,5,-1},
{-1,8,8,8,8,8,8,8,8,-1},
{-1,9,12,9,9,9,9,9,9,-1},
{-1,9,17,14,14,14,14,14,14,-1},
{-1,-1,17,17,17,17,17,17,17,-1},
{-1,-1,7,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,17,17,17,17,17,17,18,-1},
{-1,9,15,14,14,14,14,14,14,-1},
{-1,-1,11,17,17,17,17,17,17,-1},
{-1,-1,17,17,17,17,17,17,16,-1},
{-1,-1,17,17,17,17,17,13,17,-1}};

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
					case 8: 
            { return OTHER; }
						break;
					case 4: 
                     { return KEYWORD; }
						break;
					case 1: 

						break;
					case 2: 
      { ; }
						break;
					case 7: case 11: 
               { return TYPE; }
						break;
					case 5: 
                     { return COMMENT; }
						break;
					case 3: case 10: case 13: case 14: case 15: case 16: case 17: case 18: 
case 19: 
         { return DOCCOMMENT; }
						break;
					case 6: 
  { return NEWLINE; }
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
