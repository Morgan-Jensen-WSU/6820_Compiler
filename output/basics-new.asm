;-----------------------------
; exports
;-----------------------------
GLOBAL main


;-----------------------------
; imports
;-----------------------------
extern printf
extern scanf
extern exit


;-----------------------------
; initialized data
;-----------------------------
section	.data

value3	dd	3


;-----------------------------
; uninitialized data
;-----------------------------
seciton	.bss

addresult	resq	1	; an int
mulresult	resq	1	; an int
subresult	resq	1	; an int
expresult	resq	1	; an int


