namespace Riscy;

public class RomProgrammer
{
    private byte[] rom1 = new byte[2048];
    /*
     * in:
     * instr7-0, cycle1-0, <0
     * out:
     * instruction_write
     * register_write
     * register_select_1
     * register_select_0
     * rrs_write
     */

    private byte[] rom2 = new byte[2048];
    /*
     * in:
     * instr7-0, cycle1-0, !=0
     * out:
     * sli_enable
     * add_enable
     * add_sub
     * add_inc
     * instruction_register_enable
     * skip-forward-enable
     * jump-enable
     */

    public void ProgramRoms()
    {
        // rom 1
        for (int i = 0; i < 2048; i++)
        {
            var opcode = (byte) (i >> 7);
            var rd = (byte) (i >> 5 & 3);
            var rs = (byte) (i >> 3 & 3);
            var cycle = (byte) (i >> 1 & 3);
            var lessThanZero = (byte) (i & 1);
            
            // instruction write
            if (cycle != 0b00) 
                rom1[i] |= 0b10000000;
            // register write
            if (cycle == 0b00 && opcode is not 0b0111 or 0b0001 && !(opcode == 0b1000 && rs is 0b00 or 0b01)) // this line is disgusting
                rom1[i] |= 0b01000000; 
            // register select & rrs write
            if (cycle == 0b01)
                rom1[i] |= (byte) (rs << 1);
            else
                rom1[i] |= (byte) ((rd << 1) & 0b1);
        }
        
        // rom 2
        for (int i = 0; i < 2048; i++)
        {
            var opcode = (byte) (i >> 7);
            var rd = (byte) (i >> 5 & 3);
            var rs = (byte) (i >> 3 & 3);
            var cycle = (byte) (i >> 1 & 3);
            var notEqualZero = (byte) (i & 1);
            
            // sli enable
            if (opcode >> 2 != 0b11) 
                rom2[i] |= 0b10000000;
            // add enable
            if (opcode is not 0b0111 or 0b0001 && !(opcode == 0b1000 && rs is 0b00 or 0b01)) 
                rom2[i] |= 0b01000000;
            // add sub
            if (opcode == 0b0001 || (opcode == 0b1000 && rs == 0b00)) 
                rom2[i] |= 0b00100000;
            // add inc
            if (opcode == 0b1000 && rs is 0b00 or 0b01) 
                rom2[i] |= 0b00010000;
            // instruction register enable
            if (cycle != 0b00) 
                rom2[i] |= 0b00001000;
            // skip forward enable
            if (cycle == 0b00 || (cycle == 0b10 && opcode == 0b0100 &&
                                  ((rs == 0b00 && notEqualZero == 0) || (rs == 0b01 && notEqualZero == 1))))
                rom2[i] |= 0b00000100;
            // jump enable
            if (!(cycle == 0b10 && opcode == 0b0101)) 
                rom2[i] |= 0b00000010;
        }
    }

    public void SaveRomTables()
    {
        File.WriteAllBytes("rom1", rom1);
        File.WriteAllBytes("rom2", rom2);
    }
}