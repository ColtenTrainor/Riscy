namespace Riscy;

public class RomProgrammer
{
    private string _outFileName;
    private string _format;
    private string _fileExt;
    
    private byte[] rom1 = new byte[2048];
    /*
     * in:
     * instr7-0, cycle1-0, <0
     * out:
     * instruction_write
     * instruction_register_enable
     * register_select_1
     * register_select_0
     * rrs_write
     */

    private byte[] rom2 = new byte[2048];
    /*
     * in:
     * instr7-0, cycle1-0, !=0
     * out:
     * register write
     * sli_enable
     * add_enable
     * add_sub
     * add_inc
     * skip-forward-enable
     * jump-enable
     */

    public RomProgrammer(string outFileName, string format, string fileExt)
    {
        _outFileName = outFileName;
        _format = format;
        _fileExt = fileExt;
    }

    public void ProgramRoms()
    {
        // rom 1
        for (int i = 0; i < 2048; i++)
        {
            var rd = (byte) (i >> 5 & 3);
            var rs = (byte) (i >> 3 & 3);
            var cycle = (byte) (i >> 1 & 3);
            
            // instruction write & instruction register enable
            if (cycle != 0b00) 
                rom1[i] |= 0b11000000;
            // register select & rrs write
            if (cycle == 0b01)
                rom1[i] |= (byte) (rs << 9);
            else
                rom1[i] |= (byte) ((rd << 7) | 0b000010000);
        }
        
        // rom 2
        for (int i = 0; i < 2048; i++)
        {
            var opcode = (byte) (i >> 7);
            var rs = (byte) (i >> 5 & 3);
            var cycle = (byte) (i >> 3 & 3);
            var lessThanZero = (byte) (i >> 1 & 1);
            var notEqualZero = (byte) (i & 1);
            
            // register write
            if (cycle == 0b00 && opcode is not 0b0111 or 0b0001 && !(opcode == 0b1000 && rs is 0b00 or 0b01)) // this line is disgusting
                rom1[i] |= 0b10000000; 
            // sli enable
            if (opcode >> 2 != 0b11) 
                rom2[i] |= 0b01000000;
            // add enable
            if (opcode is not 0b0111 or 0b0001 && !(opcode == 0b1000 && rs is 0b00 or 0b01)) 
                rom2[i] |= 0b00100000;
            // add sub
            if (opcode == 0b0001 || (opcode == 0b1000 && rs == 0b00)) 
                rom2[i] |= 0b00010000;
            // add inc
            if (opcode == 0b1000 && rs is 0b00 or 0b01) 
                rom2[i] |= 0b00001000;
            // skip forward enable
            if (cycle == 0b00 || (cycle == 0b10 && opcode == 0b0100 && // skipz & skipnz
                                  ((rs == 0b00 && notEqualZero == 0) || (rs == 0b01 && notEqualZero == 1))))
                rom2[i] |= 0b00000100;
            if (cycle == 0b00 || (cycle == 0b10 && opcode == 0b0100 && // skipl & skipge
                                  ((rs == 0b10 && lessThanZero == 1) || (rs == 0b11 && lessThanZero == 0))))
                rom2[i] |= 0b00000100;
            // jump enable
            if (!(cycle == 0b10 && opcode == 0b0101)) 
                rom2[i] |= 0b00000010;
        }
    }

    public void SaveRomTables()
    {
        if (_format.Equals("r"))
        {
            File.WriteAllBytes(_outFileName + "_1" + _fileExt, rom1);
            File.WriteAllBytes(_outFileName + "_2" + _fileExt, rom2);
        }
        else
        {
            var file = File.AppendText(_outFileName + "_1" + _fileExt);
            foreach (var b in rom1)
            {
                file.WriteLine(b.ToOutString(_format));
            }
            file = File.AppendText(_outFileName + "_2" + _fileExt);
            foreach (var b in rom2)
            {
                file.WriteLine(b.ToOutString(_format));
            }
        }
    }
}