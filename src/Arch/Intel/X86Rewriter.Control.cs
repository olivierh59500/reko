﻿#region License
/* 
 * Copyright (C) 1999-2010 John Källén.
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
#endregion

using Decompiler.Core;
using Decompiler.Core.Expressions;
using Decompiler.Core.Machine;
using Decompiler.Core.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Decompiler.Arch.Intel
{
    public partial class X86Rewriter
    {
        private TestCondition CreateTestCondition(ConditionCode cc, Identifier identifier)
        {
            var tc = new TestCondition(cc, identifier);
            return tc;
            /*
            if (i < 2)
                return tc;
            if (instrs[i-1].code != Opcode.test)
                return tc;
            var ah = instrs[i-1].op1 as RegisterOperand;
            if (ah == null || ah.Register != Registers.ah)
                return tc;
            var m = instrs[i-1].op2 as ImmediateOperand;
            if (m == null)
                return tc;

            if (instrs[i-2].code != Opcode.fstsw)
                return tc;
            int mask = m.Value.ToInt32();

            var fpuf = orw.FlagGroup(FlagM.FPUF);
            switch (cc)
            {
            case ConditionCode.PE:
                if (mask == 0x05) return new TestCondition(ConditionCode.LE, fpuf);
                if (mask == 0x41) return new TestCondition(ConditionCode.GE, fpuf);
                if (mask == 0x44) return new TestCondition(ConditionCode.NE, fpuf);
                break;
            case ConditionCode.PO:
                if (mask == 0x44) return new TestCondition(ConditionCode.EQ, fpuf);
                if (mask == 0x41) return new TestCondition(ConditionCode.GE, fpuf);
                if (mask == 0x05) return new TestCondition(ConditionCode.GT, fpuf);
                break;
            case ConditionCode.EQ:
                if (mask == 0x40) return new TestCondition(ConditionCode.NE, fpuf);
                if (mask == 0x41) return new TestCondition(ConditionCode.LT, fpuf);
                break;
            case ConditionCode.NE:
                if (mask == 0x40) return new TestCondition(ConditionCode.EQ, fpuf);
                if (mask == 0x41) return new TestCondition(ConditionCode.GE, fpuf);
                if (mask == 0x01) return new TestCondition(ConditionCode.GT, fpuf);
                break;
            }
            throw new NotImplementedException(string.Format(
                "FSTSW/TEST AH,0x{0:X2}/J{1} not implemented.", mask, cc));
             */
        }

        private void RewriteCall(MachineOperand callTarget, PrimitiveType opsize)
        {
            var sp = StackPointer();
            emitter.Assign(sp, emitter.Sub(sp, opsize.Size));
            Address addr = OperandAsCodeAddress(callTarget);
            if (addr != null)
            {
                emitter.Call(addr);
            }
            else
            {
                var target = SrcOp(callTarget);
                if (target.DataType.Size == 2)
                    target = emitter.Seq(orw.AluRegister(Registers.cs), target);
                emitter.Call(target);
            }
        }

        private void RewriteConditionalGoto(ConditionCode cc, MachineOperand op1)
        {
            emitter.Branch(CreateTestCondition(cc, orw.FlagGroup(IntelInstruction.UseCc(di.Instruction.code))), OperandAsCodeAddress(op1));
        }


        private void RewriteInt()
        {
            emitter.SideEffect(PseudoProc("__syscall", PrimitiveType.Void, SrcOp(di.Instruction.op1)));
        }

        private void RewriteJmp()
        {
            if (IsRealModeReboot(di.Instruction))
			{
                PseudoProcedure reboot = host.EnsurePseudoProcedure("__bios_reboot", PrimitiveType.Void, 0);
                reboot.Characteristics = new Decompiler.Core.Serialization.ProcedureCharacteristics();
                reboot.Characteristics.Terminates = true;
                emitter.SideEffect(PseudoProc(reboot, PrimitiveType.Void));
				return;
			}
				
			if (di.Instruction.op1 is ImmediateOperand)
			{
				Address addr = OperandAsCodeAddress(di.Instruction.op1);
                emitter.Goto(addr);
				return;
			}
            emitter.Goto(SrcOp(di.Instruction.op1));
        }

        private void RewriteLoop(FlagM useFlags, ConditionCode cc)
        {
            Identifier cx = orw.AluRegister(Registers.ecx, di.Instruction.dataWidth);
            emitter.Assign(cx, emitter.Sub(cx, 1));
            Identifier flag;
            if (useFlags != 0)
            {
                throw new NotImplementedException();
                //CodeEmitter e = emitter;

                //// Splice in a new block.

                //blockNew = proc.AddBlock(state.InstructionAddress.GenerateName("l", "_loop"));
                //proc.AddEdge(blockHead, blockNew);

                //emitter = ProcedureRewriter.CreateEmitter(blockNew);
                //Block tgt = EmitBranchInstruction(emitter.Eq0(cx), instrCur.op1);
                //e.Branch(new TestCondition(cc, orw.FlagGroup(useFlags)), tgt);
            }
            else
            {
                EmitCcInstr(cx, FlagM.ZF);
                emitter.Branch(new TestCondition(ConditionCode.NE, orw.FlagGroup(FlagM.ZF)), OperandAsCodeAddress(di.Instruction.op1));
            }
        }

        public void RewriteRet()
        {
            EmitReturnInstruction(
                this.arch.WordWidth.Size + (di.Instruction.code == Opcode.retf ? PrimitiveType.Word16.Size : 0),
                di.Instruction.Operands == 1 ? ((ImmediateOperand)di.Instruction.op1).Value.ToInt32() : 0);
        }

        private void EmitReturnInstruction(int cbReturnAddress, int cbBytesPop)
        {
            emitter.Return(cbReturnAddress, cbBytesPop);
        }

        /// <summary>
        /// A jump to 0xFFFF:0x0000 in real mode is a reboot.
        /// </summary>
        /// <param name="instrCur"></param>
        /// <returns></returns>
        private bool IsRealModeReboot(IntelInstruction instrCur)
        {
            AddressOperand addrOp = instrCur.op1 as AddressOperand;
            bool isRealModeReboot = addrOp != null && addrOp.Address.Linear == 0xFFFF0;
            return isRealModeReboot;
        }

        public Address OperandAsCodeAddress(MachineOperand op)
        {
            AddressOperand ado = op as AddressOperand;
            if (ado != null)
                return ado.Address;
            ImmediateOperand imm = op as ImmediateOperand;
            if (imm != null)
            {
                if (arch.ProcessorMode == ProcessorMode.ProtectedFlat)
                {
                    return new Address(imm.Value.ToUInt32());
                }
                else
                    return new Address(di.Address.Selector, imm.Value.ToUInt32());
            }
            return null;
        }

    }
}