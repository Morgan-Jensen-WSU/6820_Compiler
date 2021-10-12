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
value3	dd	3


;-----------------------------
; uninitialized data
;-----------------------------
section	.bss

addresult	resq	1	; an int
mulresult	resq	1	; an int
subresult	resq	1	; an int
expresult	resq	1	; an int


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
	mov	rax, [qword value3]
	add	rax, 10
	mov	[qword addresult], rax

	mov	rax, [qword addresult]
	call printInt

	mov	rax, [qword value3]
	imul	rax, 10
	mov	[qword mulresult], rax

	mov	rax, [qword mulresult]
	call printInt

	mov	rax, [qword value3]
	sub	rax, 10
	mov	[qword subresult], rax

	mov	rax, [qword subresult]
	call printInt

	mov	rdi, 1
	mov	rax, 10
	mov	rdx, [qword value3]
exp_start:
	cmp	rdi, rdx
	jz exp_done
	imul	rax, 10
	inc	rdi
	jmp	exp_start
exp_done:
	mov	[qword expresult], rax

	mov	rax, [qword expresult]
	call printInt

exit:
	mov	rax, 60
	xor	rdi, rdi
	syscall
