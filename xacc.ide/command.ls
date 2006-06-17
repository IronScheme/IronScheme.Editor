; default command file
; file is persisted (autosaved)
; run this file by pressing Ctrl + Enter


;(bind (fn ()((open (file servicehost) "untitled.cs"))) "F3")

;(= toolbar (toolbar servicehost))
;(= tb (get_toolbar toolbar))
;(set_Height tb 40)
;(set_TextAlign tb (Underneath ToolBarTextAlign))

;(prl (text (open (file servicehost) "profile.ls")))

(defun dt ()(dumptargets keyboard))

; different ways to do the same things
;(register keyboard dt '("Ctrl+(D,T)"))                           ; normal
;(register keyboard (fn ()(dumpkeys keyboard)) '("Ctrl+(D,K)"))   ; inline function
;(bindx Buffer "Edit.SelectLine" "Ctrl+(K,S)")                    ; uses macro & state

; emacs emulation - this is not working properly
;(set_menuaccel (menu servicehost) false)                          ; disables normal mnemonics
;(bindx Buffer "File.Save(string)" "Alt+F"   "S")                  ; this is 2 keystrokes ie "M-f s"
;(bindx Normal "Help.ShowAbout"    "Ctrl+H"  "A")                  ; this is 2 keystrokes ie "C-h a"


