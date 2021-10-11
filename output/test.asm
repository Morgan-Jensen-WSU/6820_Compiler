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

stringPrinter	db	"%s",0
numberPrinter	db	"%d",0x0d,0x0a,0
int_format	db	"%i", 0
thisIsATest	dd	100
S0		db	"please work", 0x0d, 0x0a, 0


;-----------------------------
; uninitialized data
;-----------------------------
section	.bss

beans	resq	1	; an int
mulresult1	resq	1	; an int
mulresult2	resq	1	; an int
subresult	resq	1	; an int


;-----------------------------
; Code
;-----------------------------
section	.text

printInt:
	push	rbp		; Avoid stack alignment isses
	push	rax		; save rax and rcx
	push	rcx

	mov	rdi, numberPrinter		; set printf format parameter
	mov	rsi, rax		; set printf value parameter
	xor	rax, rax		; set rax to 0 (number of float/vector regs used is 0)

	call	[rel printf wrt ..got]
	pop	rcx		; restore rcx
	pop	rax		; restore rax
	pop	rbp		; avoid stack alignment issues
	ret

printString:
	push	rbp		; Avoid stack alignment isses
	push	rax		; save rax and rcx
	push	rcx

	mov	rdi, stringPrinter		; set printf format parameter
	mov	rsi, rax		; set printf value parameter
	xor	rax, rax		; set rax to 0 (number of float/vector regs used is 0)

	call	[rel printf wrt ..got]
	pop	rcx		; restore rcx
	pop	rax		; restore rax
	pop	rbp		; avoid stack alignment issues
	ret

main:
	mov	rax, [qword thisIsATest]
	add	rax, 100
	mov	[qword beans], rax

	mov	rax, [qword beans]
	call printInt

	mov	rax, [qword beans]
	imul	rax, 10
	mov	[qword mulresult1], rax

	mov	rax, [qword mulresult1]
	call printInt

	mov	rax, [qword beans]
	imul	rax, 100
	mov	[qword mulresult2], rax

	mov	rax, [qword mulresult2]
	call printInt

	mov	rax, [qword beans]
	sub	rax, 10
	mov	[qword subresult], rax

	mov	rax, [qword subresult]
	call printInt

	mov	rax, [qword S0]
	call printString

exit:
	mov	rax, 60
	xor	rdi, rdi
	syscall
