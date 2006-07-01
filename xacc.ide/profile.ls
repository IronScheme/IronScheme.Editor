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
(using "Xacc.ComponentModel")
(using "Xacc.CodeModel")
(using "Xacc.Build")
(using "System.Drawing")
(using "System.Windows.Forms")

; profile start
(= settings (settings servicehost))

(set_editorfontsize settings 9)
;(set_editorfontname settings "Bitstream Vera Sans Mono")
(set_editorfontname settings "Consolas") ; set size to 10.33
; (set_editorfontname settings "Lucida Console")
; (set_editorfontname settings "Courier New")
(set_tabsize settings 2)

; override token colors
(= language (language servicehost))

(defmacro tokencolor (tc col)
	`(settokenclasscolor language (,tc tokenclass) (,col color)))

(defmacro tokencolor2 (tc col col2)
	`(settokenclasscolor language (,tc tokenclass) (,col color) (,col2 color)))
	
(defmacro tokencolor3 (tc col col2 col3 style)
	`(settokenclasscolor language (,tc tokenclass) (,col color) (,col2 color) (,col3 color) (,style fontstyle)))
	
(defmacro tokencolor4 (tc col style)
	`(settokenclasscolor language (,tc tokenclass) (,col color) (empty color) (,style fontstyle)))	

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
(tokencolor2  Pair          DarkBlue        LightGray)
(tokencolor   Comment       DarkGreen)
(tokencolor   DocComment    DimGray)
(tokencolor   Operator      DarkBlue)
(tokencolor   Other         DeepPink)

; hide toolbar till i can get it to work
(set_showtoolbar (view servicehost) false)

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
	
; 1st param servicename.method()-case-sensitive 2nd param Keys[]
(bind   "File.NextWindow"       "Ctrl+Tab")
(bind   "File.PreviousWindow"   "Ctrl+Shift+Tab")

; help
(bindx  Normal          "Help.ReadMe"             "F1")

; file commands
(bindx  Normal          "File.NewFile"            "Ctrl+N")
(bindx  Normal          "File.OpenFile"           "Ctrl+O")
(bindx  File            "File.CloseFile"          "Ctrl+F4")
(bindx  File            "File.SaveFile"           "Ctrl+S")

; view commands
(bindx  Normal          "View.ShowToolbar"          "Ctrl+D1")
(bindx  Normal          "View.ShowProjectExplorer"  "Ctrl+D2")
(bindx  Normal          "View.ShowOutline"          "Ctrl+D3")
(bindx  Normal          "View.ShowResults"          "Ctrl+D4")
(bindx  Normal          "View.ShowConsole"          "Ctrl+D5")
(bindx  Normal          "View.ShowCommand"          "Ctrl+D6")

; project commands
(bindx  Project         "Project.AddNewFile"      "Ctrl+Shift+A")
(bindx  Project         "Project.AddExistingFile" "Ctrl+Insert")
(bindx  Project         "Project.BuildAll"        "Ctrl+Shift+B")
(bindx  Project         "Project.Build"           "Ctrl+B")
(bindx  Project         "Project.Run"             "Ctrl+F5")
(bindx  Normal          "Project.Open"            "Ctrl+P")

; debug commands
(bindx  Project         "Debug.Start"             "F5")
(bindx  DebugBreak      "Debug.Continue"          "F5")
(bindx  DebugBreak      "Debug.Step"              "F10")
(bindx  DebugBreak      "Debug.StepInto"          "F11")
(bindx  DebugBreak      "Debug.StepOut"           "Shift+F11")
(bindx  ProjectBuffer   "Debug.ToggleBP"          "F9")
(bindx  ProjectBuffer   "Debug.ToggleAllBP"       "Ctrl+F9")
(bindx  ProjectBuffer   "Debug.ToggleBPState"     "Shift+F9")
(bindx  ProjectBuffer   "Debug.ToggleAllBPState"  "Ctrl+Shift+F9")
(bindx  Debug           "Debug.Exit"              "Shift+F5")

; script commands
(bindx  File            "Scripting.Run"           "Ctrl+Enter")
(bindx  Buffer          "Scripting.RunSelected"   "Ctrl+Shift+Enter")

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





; autocomplete commands
(bindx  AutoComplete    "Edit.HideAutoComplete"   "Escape")
(bindx  AutoComplete    "Edit.HideAutoComplete"   "Left")
(bindx  AutoComplete    "Edit.HideAutoComplete"   "Right")
(bindx  AutoComplete    "Edit.HideAutoComplete"   "Home")
(bindx  AutoComplete    "Edit.HideAutoComplete"   "End")
(bindx  AutoComplete    "Edit.HideAutoComplete"   "Tab")
(bindx  AutoComplete    "Edit.HideAutoComplete"   "Space")
(bindx  AutoComplete    "Edit.HideAutoComplete"   "Alt+Right")

(bindx  Buffer          "Edit.ShowAutoComplete"   "Alt+Right")

(bindx  AutoComplete    "Edit.AutoCompleteNextChoice"       "Down")
(bindx  AutoComplete    "Edit.AutoCompletePreviousChoice"   "Up")
(bindx  AutoComplete    "Edit.AutoCompleteNextPage"         "PageDown")
(bindx  AutoComplete    "Edit.AutoCompletePreviousPage"     "PageUp")
(bindx  AutoComplete    "Edit.AutoCompleteSelectChoice"     "Enter")

; advanced commands 
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

; these will fail under Mono
(bindx  Buffer          "Edit.CommentSelection"   "Ctrl+(K,C)")
(bindx  Buffer          "Edit.UnCommentSelection" "Ctrl+(K,U)")
(bind (fn ()(prl "CLEAR")) "Ctrl+(K,K)")

; override defaults if necessary
(set_tracecall toploop tc)
(set_tracereturn toploop tr)

; some experimental 'macro' code - DOESNT WORK
;(defun strjoin (sep &rest l)
;	(join string sep (toarray l (typeof string))))
;	
;(defmacro strcat (&rest l) 
;	`(strjoin "" ,@l))
;	
; sample macro
;(defmacro makeprop (t n)
;  `` ,(strjoin "\n" 	,(strcat "" ,t " " ,(tolower ,n) ";")
;          						,(strcat "public " ,t " " ,n )
;          						,(strcat "{")
;          						,(strcat "   get {return " ,(tolower ,n) ";}")
;						          ,(strcat "   set {" ,(tolower ,n) " = value;}")
;						          ,(strcat "}"))
;  )
;  
;(= fs (file servicehost))
;
;(defun make-prop ()
;	(= atb (get_item fs (current fs)))
;	(= tokens (split (selectiontext atb) (tochararray " ")))
;	(set_selectiontext atb (makeprop (nth 0 tokens) (nth 1 tokens)))
;)
;
;(bindx Buffer make-prop "Ctrl+(M,P)")

(prl "Profile loaded")
