using Xacc.Languages.CSLex;
#pragma warning disable 162
using Xacc.ComponentModel;
using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;
namespace Xacc.Languages
{
  sealed class PlainText : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "Plain Text"; } }
	  public override string[] Extensions {get { return new string[]{"*"}; } }
	  LexerBase lexer = new PlainTextLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}


sealed class PlainTextLexer : LexerBase {

	public PlainTextLexer () {
	YY_BOL = 65536;
	YY_EOF = 65537;

	}

	const int YYINITIAL = 0;
	static readonly int[] yy_state_dtrans = {
		0
	};
	static readonly int[] yy_acpt = {
0,
4,4,4,4,4	};
	static readonly int[] yy_cmap = unpackFromString(
1,65538,
"1:9,2,3,1:21,2,4,1:6,4:2,1:2,4,1,4,1:16,4,1:65472,0:2")[0];

	static readonly int[] yy_rmap = {
0,1,2,3,1,1};

	static readonly int[,] yy_nxt = {
{1,2,3,4,5},
{-1,-1,-1,-1,-1},
{-1,2,-1,-1,-1},
{-1,-1,3,-1,-1}};

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
					case 5: 
 {return PLAIN; }
						break;
					case 4: 
  {return NEWLINE;}
						break;
					case 1: 

						break;
					case 3: 
      {;}
						break;
					case 2: 
                   {return PLAIN; }
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
