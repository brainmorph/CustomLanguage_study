namespace BabyVM;

// Manchester Baby (SSEM, 1948) virtual machine.
//
// Execution cycle — exactly as the real Baby:
//   1. CI is incremented first
//   2. The word at store[CI] is fetched into PI
//   3. PI is decoded: bits 15-13 = function, bits 4-0 = operand address S
//   4. The instruction executes, possibly modifying CI or ACC
//   5. Repeat until STP
//
// Consequence: execution begins at address 1, not 0.
// To jump to address N, store the value (N-1) at the jump target word,
// because CI will be incremented once more before the next fetch.
public class Machine
{
    private readonly Store _store;
    private readonly MachineIO _io;

    private int _acc;   // accumulator — the only register visible to programs
    private int _ci;    // current instruction (program counter), 5-bit (0–31)

    public Machine(Store store, MachineIO io)
    {
        _store = store;
        _io = io;
        _acc = 0;
        _ci = 0;  // first cycle increments to 1, so execution starts at address 1
    }

    public void Run()
    {
        while (true)
        {
            // Step 1 — increment CI (happens before fetch, always)
            _ci = (_ci + 1) & 0x1F;

            // Step 2 — fetch
            int pi = _store.Read(_ci);

            // Step 3 — decode
            int function = (pi >> 13) & 0x7;
            int s        = pi & 0x1F;

            // Step 4 — execute
            switch (function)
            {
                case 0: // JMP S — CI = store[S]
                    // Indirect jump: CI is set to the value stored at address S.
                    // The next cycle increments CI before fetching, so to land on
                    // instruction N you must store the value (N-1) at S.
                    _ci = _store.Read(s) & 0x1F;
                    break;

                case 1: // JRP S — CI += store[S]  (jump relative)
                    _ci = (_ci + _store.Read(s)) & 0x1F;
                    break;

                case 2: // LDN S — ACC = -store[S]  (load negated — no plain load exists)
                    _acc = s == 31 ? -_io.ReadInput() : -_store.Read(s);
                    break;

                case 3: // STO S — store[S] = ACC
                    if (s == 31)
                        _io.WriteOutput(_acc);
                    else
                        _store.Write(s, _acc);
                    break;

                case 4: // SUB S — ACC -= store[S]
                case 5: // SUB S — opcode 5 is a duplicate of 4 in the original Baby
                    _acc -= _store.Read(s);
                    break;

                case 6: // CMP — if ACC < 0: skip next instruction
                    if (_acc < 0)
                        _ci = (_ci + 1) & 0x1F;
                    break;

                case 7: // STP — halt
                    return;
            }
        }
    }
}
