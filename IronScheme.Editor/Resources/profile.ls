; default profile file
; use this file to setup IDE defaults

; some init stuff
(= tc (tracecall toploop))
(= tr (tracereturn toploop))

; turn off tracing
(set_tracecall toploop false)
(set_tracereturn toploop false)

; default usings !!! REMEMBER THESE !!!
(using "System.Collections")
(using "System.Reflection")
(using "System.Text.RegularExpressions")
(using "IronScheme.Editor.ComponentModel")
(using "IronScheme.Editor.CodeModel")
(using "IronScheme.Editor.Build")
(using "System.Drawing")
(using "System.Windows.Forms")

; profile start
(= settings (settings servicehost))

(set_editorfontsize settings 10)
(set_editorfontname settings "Consolas")
; (set_editorfontname settings "Lucida Console")
; (set_editorfontname settings "Courier New")
(set_tabsize settings 2)

; override token colors
(= language (language servicehost))

(defmacro tokencolor (tc col)
	`(settokenclasscolor language (,tc tokenclass) (,col color)))

(tokencolor   Error         Red)
(tokencolor   Warning       Black)
(tokencolor   Ignore        Black)
(tokencolor   Any           Black)
(tokencolor   Identifier    Black)
(tokencolor   Type          Teal)
(tokencolor   Keyword       Blue)
(tokencolor   Preprocessor  DarkBlue)
(tokencolor   String        Maroon)
(tokencolor   Character     DarkOrange)
(tokencolor   Number        Red)
(tokencolor   Pair          DarkBlue)
(tokencolor   Comment       DarkGreen)
(tokencolor   DocComment    DimGray)
(tokencolor   Operator      DarkBlue)
(tokencolor   Other         DeepPink)


; keybindings
(= keyboard (keyboard servicehost))

; nice macro example :p
(defmacro bind (func &rest keys)
	`(register keyboard ,func '(,@keys)))
	
; binds with shift
(defmacro binds (func &rest keys)
	`(register keyboard ,func '(,@keys) `true))

; binds with state	
(defmacro bindx (state func &rest keys)
	`(register keyboard ,func '(,@keys) (,state applicationstate)))

; binds with shift and state
(defmacro bindsx (state func &rest keys)
	`(register keyboard ,func '(,@keys) `true (,state applicationstate)))
	

; buffer commands
(bindx  Buffer          "Edit.FindReplace"        "Ctrl+F")
(bindx  Edit            "Edit.Undo"               "Ctrl+Z")
(bindx  Edit            "Edit.Undo"               "Alt+Back")
(bindx  Edit            "Edit.Redo"               "Ctrl+Shift+Z")
(bindx  Edit            "Edit.Redo"               "Ctrl+Back")
(bindx  Edit            "Edit.Cut"                "Ctrl+X")
(bindx  Edit            "Edit.Cut"                "Shift+Delete")
(bindx  Edit            "Edit.Copy"               "Ctrl+C")
(bindx  Edit            "Edit.Copy"               "Ctrl+Insert")
(bindx  Edit            "Edit.Paste"              "Ctrl+V")
(bindx  Edit            "Edit.Paste"              "Shift+Insert")
(bindx  Edit            "Edit.SelectAll"          "Ctrl+A")

(bindx  Buffer          "Edit.RemoveBefore"       "Back")
(bindx  Buffer          "Edit.RemoveAfter"        "Delete")

; advanced commands
(bindx  Buffer          "Edit.CommentSelection"   "Ctrl+(K,C)")
(bindx  Buffer          "Edit.UnCommentSelection" "Ctrl+(K,U)")
(bindx  Buffer          "Edit.SelectionToUpper"   "Ctrl+U")
(bindx  Buffer          "Edit.SelectionToLower"   "Ctrl+Shift+U")
(bindx  Buffer          "Edit.IncreaseIndent"     "Tab")
(bindx  Buffer          "Edit.DecreaseIndent"     "Shift+Tab")

(bindsx Buffer          "Edit.GotoFirstLine"      "Ctrl+Home")
(bindsx Buffer          "Edit.GotoLastLine"       "Ctrl+End")
(bindsx Buffer          "Edit.GotoNextToken"      "Ctrl+Right")
(bindsx Buffer          "Edit.GotoPreviousToken"  "Ctrl+Left")
(bindx  Buffer          "Edit.InsertLine"         "Enter")

(bindsx Navigate        "Edit.NavigateRight"      "Right")
(bindsx Navigate        "Edit.NavigateLeft"       "Left")
(bindsx Navigate        "Edit.NavigateUp"         "Up")
(bindsx Navigate        "Edit.NavigateDown"       "Down")
(bindsx Navigate        "Edit.NavigatePageUp"     "PageUp")
(bindsx Navigate        "Edit.NavigatePageDown"   "PageDown")
(bindsx Navigate        "Edit.NavigateHome"       "Home")
(bindsx Navigate        "Edit.NavigateEnd"        "End")

(bindsx Scroll          "Edit.ScrollUp"           "Ctrl+Up")
(bindsx Scroll          "Edit.ScrollDown"         "Ctrl+Down")
(bindsx Scroll          "Edit.ScrollPageUp"       "Ctrl+PageUp")
(bindsx Scroll          "Edit.ScrollPageDown"     "Ctrl+PageDown")

; override defaults if necessary
(set_tracecall toploop tc)
(set_tracereturn toploop tr)

(prl "Profile loaded")
