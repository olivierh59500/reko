﻿/* 
 * Copyright (C) 1999-2009 John Källén.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */

using Decompiler.Core;
using Decompiler.Core.Code;
using Decompiler.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Decompiler.Arch.M68k
{

    public class InstructionFormatter 
    {
        private StringWriter writer;

        public InstructionFormatter(StringBuilder sb)
        {
            writer = new StringWriter(sb);
        }

        public void Write(M68kInstruction instr)
        {
            writer.Write(instr.code);
            if (instr.dataWidth != null)
            {
                writer.Write(DataSizeSuffix(instr.dataWidth));
            }
            writer.Write('\t');
            if (instr.op1 != null)
            {
                WriteOperand(instr.op1);
                if (instr.op2 != null)
                {
                    writer.Write(',');
                    WriteOperand(instr.op2);
                }
            }
        }

        private string DataSizeSuffix(PrimitiveType dataWidth)
        {
            switch (dataWidth.BitSize)
            {
                case 8: return ".b";
                case 16: return ".w";
                case 32: return ".l";
                default: throw new InvalidOperationException(string.Format("Unsupported data width {0}.", dataWidth.BitSize));
            }
        }

        private void WriteOperand(MachineOperand op)
        {
            RegisterOperand reg = op as RegisterOperand;
            if (reg != null)
            {
                writer.Write(reg.Register.Name);
                return;
            }
            ImmediateOperand imm = op as ImmediateOperand;
            if (imm != null)
            {
                writer.Write("#$");
                writer.Write(imm.FormatValue(imm.Value));
                return;
            }
            MemoryOperand mop = op as MemoryOperand;
            if (mop != null)
            {
                writer.Write("(");
                writer.Write(mop.Base.Name);
                writer.Write(")");
            }

        }

    }
}