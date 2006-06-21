using Xacc.Languages.CSLex;
#pragma warning disable 162
using Xacc.ComponentModel;
using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;
namespace Xacc.Languages
{
  sealed class Changelog : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "ChangeLog"; } }
	  public override string[] Extensions {get { return new string[]{"ChangeLog"}; } }
	  LexerBase lexer = new ChangelogLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
	  public override bool Match(string filename)
    {
      return filename.ToLower() == "changelog.txt";
    }
  }
}


sealed class ChangelogLexer : LexerBase {

	public ChangelogLexer () {
	YY_BOL = 128;
	YY_EOF = 129;

	}

	const int EXPDATE = 1;
	const int YYINITIAL = 0;
	static readonly int[] yy_state_dtrans = {
		0, 		20
	};
	static readonly int[] yy_acpt = {
0,
4,4,4,4,4,4,4,4,0,4,4,4,4,0,4,4,
0,4,4,0,0,4	};
	static readonly int[] yy_cmap = unpackFromString(
1,130,
"9:9,2,8,9:2,4,9:18,3,9:12,1,6,9,5:10,7,9:69,0:2")[0];

	static readonly int[] yy_rmap = {
0,1,2,2,1,2,2,3,4,5,6,7,5,8,9,5,10,8,4,4,11,6,12};

	static readonly int[,] yy_nxt = {
{1,2,3,11,9,10,15,15,4,15},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,5,5,5,5,5,5,6,-1,5},
{-1,17,9,9,9,7,14,12,-1,9},
{-1,8,8,8,8,8,8,8,-1,8},
{-1,9,9,9,9,9,9,12,-1,9},
{-1,9,9,9,9,21,14,12,-1,9},
{-1,9,16,3,9,9,9,12,-1,9},
{-1,13,9,9,13,13,13,13,-1,13},
{-1,9,9,9,9,7,9,12,-1,9},
{-1,9,16,16,9,9,9,12,-1,9},
{1,18,19,22,9,10,15,15,4,15},
{-1,9,16,19,9,9,9,12,-1,9}};

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
                            { BEGIN(YYINITIAL); return TYPE; }
						break;
					case 4: 
  { return NEWLINE;}
						break;
					case 1: 

						break;
					case 2: case 10: case 15: case 18: 
 { return ERROR; }
						break;
					case 7: case 13: 
                                 { BEGIN(EXPDATE); return KEYWORD;}
						break;
					case 5: 
                   { return PLAIN; }
						break;
					case 3: case 11: case 16: case 19: case 22: 
      { ; }
						break;
					case 6: case 12: 
         { return PREPROC; }
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
