; add.asm — compute 3 + 4, store result at address 22
;
; The Baby has no ADD instruction, so addition uses double negation:
;
;   Step 1: ACC = -A          (LDN)
;   Step 2: ACC = -A - B      (SUB)
;   Step 3: store temp = -A-B (STO)
;   Step 4: ACC = -(-A-B)     (LDN from temp)  → ACC = A + B
;   Step 5: store result      (STO)
;
; Address layout:
;   1–6  : instructions
;   20   : A (operand, value 3)
;   21   : B (operand, value 4)
;   22   : result (written by program)
;   23   : temp   (intermediate, -(A+B))

ORG 1
LDN 20   ; ACC = -3
SUB 21   ; ACC = -3 - 4 = -7
STO 23   ; temp[23] = -7
LDN 23   ; ACC = -(-7) = 7
STO 22   ; result[22] = 7
STP

ORG 20
DAT 3    ; A = 3
DAT 4    ; B = 4
DAT 0    ; result placeholder (address 22)
DAT 0    ; temp placeholder   (address 23)
