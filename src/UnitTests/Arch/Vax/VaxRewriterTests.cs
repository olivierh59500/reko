﻿#region License
/* 
 * Copyright (C) 1999-2016 John Källén.
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

using NUnit.Framework;
using Reko.Arch.Vax;
using Reko.Core;
using Reko.Core.Rtl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reko.UnitTests.Arch.Vax
{
    [TestFixture]
    public class VaxRewriterTests : RewriterTestBase
    {
        private VaxArchitecture arch = new VaxArchitecture();
        private Address baseAddr = Address.Ptr32(0x0010000);
        private VaxProcessorState state;
        private MemoryArea image;

        public override IProcessorArchitecture Architecture
        {
            get { return arch; }
        }

        protected override IEnumerable<RtlInstructionCluster> GetInstructionStream(Frame frame, IRewriterHost host)
        {
            return new VaxRewriter(arch, new LeImageReader(image, 0), state, new Frame(arch.WordWidth), host);
        }

        public override Address LoadAddress
        {
            get { return baseAddr; }
        }

        [SetUp]
        public void Setup()
        {
            state = (VaxProcessorState)arch.CreateProcessorState();
        }

        private void BuildTest(params byte[] bytes)
        {
            image = new MemoryArea(baseAddr, bytes);
        }


        [Test]
        public void VaxRw_addb2()
        {
            BuildTest(0x80, 0x01, 0xFC, 0x00, 0xFC, 0x00, 0x3C);	// addb2	#01,+03C00FC400(ap)
            AssertCode(
                "0|L--|00010000(7): 3 instructions",
                "1|L--|v3 = Mem0[ap + 0x3C00FC00:byte] + 0x01",
                "2|L--|Mem0[ap + 0x3C00FC00:byte] = v3",
                "3|L--|CVZN = cond(v3)");
        }

        [Test]
        public void VaxRw_addd2()
        {
            BuildTest(0x60, 0x02, 0xE4, 0x04, 0xE4, 0x04, 0xE0);	// addd2	#0.625,-1FFB1BFC(r4)
            AssertCode(
                "0|L--|00010000(7): 5 instructions",
                "1|L--|v3 = Mem0[r4 + 0xE004E404:real64] + 0.625",
                "2|L--|Mem0[r4 + 0xE004E404:real64] = v3",
                "3|L--|ZN = cond(v3)",
                "4|L--|C = false",
                "5|L--|V = false");
        }

        [Test]
        public void VaxRw_addd3()
        {
            BuildTest(0x61, 0x01, 0x52, 0x75);	// addd3	#0.5625,r2,-(r5)
            AssertCode(
                "0|L--|00010000(4): 6 instructions",
                "1|L--|r5 = r5 - 0x00000008",
                "2|L--|v4 = r2 + 0.5625",
                "3|L--|Mem0[r5:real64] = v4",
                "4|L--|ZN = cond(v4)",
                "5|L--|C = false",
                "6|L--|V = false");
        }

        [Test]
        public void VaxRw_addf2()
        {
            BuildTest(0x40, 0x01, 0x52);	// addf2	
            AssertCode(
                "0|L--|00010000(3): 4 instructions",
                "1|L--|r2 = r2 + 0.5625F",
                "2|L--|ZN = cond(r2)",
                "3|L--|C = false",
                "4|L--|V = false");
        }

        [Test]
        public void VaxRw_addf3()
        {
            BuildTest(0x41, 0x52, 0x53, 0x54);	// addf3	
            AssertCode(
                "0|L--|00010000(4): 4 instructions",
                "1|L--|r4 = r3 + r2",
                "2|L--|ZN = cond(r4)",
                "3|L--|C = false",
                "4|L--|V = false");
        }

        [Test]
        public void VaxRw_addl2()
        {
            BuildTest(0xC0, 0x04, 0xAC, 0x08);	// addl2	#00000004,+08(ap)
            AssertCode(
                "0|L--|00010000(4): 3 instructions",
                "1|L--|v3 = Mem0[ap + 8:word32] + 0x00000004",
                "2|L--|Mem0[ap + 8:word32] = v3",
                "3|L--|CVZN = cond(v3)");
        }

        [Test]
        public void VaxRw_addl3()
        {
            BuildTest(0xC1, 0x04, 0x54, 0x53);	// addl3	#00000004,r4,r3
            AssertCode(
                "0|L--|00010000(4): 2 instructions",
                "1|L--|r3 = r4 + 0x00000004",
                "2|L--|CVZN = cond(r3)");
        }

        [Test]
        public void VaxRw_addp4()
        {
            BuildTest(0x20, 0x04, 0x54, 0x04, 0x50);	// addp4	#0004,r4,#0004,r0
            AssertCode(
                "0|L--|00010000(5): 2 instructions",
                "1|L--|VZN = vax_addp4(0x0004, r4, 0x0004, r0)",
                "2|L--|C = false");
        }

        [Test]
        public void VaxRw_addp6()
        {
            BuildTest(0x21, 0x04, 0x52, 0x04, 0x53, 0x04, 0x54);	// addp6	#0004,-(r2),#0004,-(r3),#0004,-(r4)
            AssertCode(
                "0|L--|00010000(7): 2 instructions",
                "1|L--|VZN = vax_addp6(0x0004, r2, 0x0004, r3, 0x0004, r4)",
                "2|L--|C = false");
        }

        [Test]
        public void VaxRw_addw2()
        {
            BuildTest(0xA0, 0x14, 0xD0, 0xC2, 0xE7);	// addw2	#0014,-183E(r0)
            AssertCode(
                "0|L--|00010000(5): 3 instructions",
                "1|L--|v3 = Mem0[r0 + -6206:word16] + 0x0014",
                "2|L--|Mem0[r0 + -6206:word16] = v3",
                "3|L--|CVZN = cond(v3)");
        }

        [Test]
        public void VaxRw_addw3()
        {
            BuildTest(0xA1, 0x14, 0xD0, 0xC2, 0xE7, 0x55);	// addw3	#0014,-183E(r0),r5
            AssertCode(
                "0|L--|00010000(6): 3 instructions",
                "1|L--|v4 = Mem0[r0 + -6206:word16] + 0x0014",
                "2|L--|r5 = DPB(r5, v4, 0)",
                "3|L--|CVZN = cond(v4)");
        }

        [Test]
        public void VaxRw_adwc()
        {
            BuildTest(0xD8, 0x63, 0x54);	// adwc	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_aobleq()
        {
            BuildTest(0xF3, 0x02, 0x54, 0xF0);	// aobleq	#00000002,r4,0000A7A8
            AssertCode(
                "0|L--|00010000(4): 3 instructions",
                "1|L--|r4 = r4 + 0x00000001",
                "2|L--|CVZN = cond(r4)",
                "3|T--|if (r4 <= 0x00000002) branch 0000FFF4");
        }

        [Test]
        public void VaxRw_ashl()
        {
            BuildTest(0x78, 0x8F, 0x05, 0x53, 0x52);	// ashl	#05,r3,r2
            AssertCode(
                "0|L--|00010000(5): 3 instructions",
                "1|L--|r2 = r3 << 0x05",
                "2|L--|VZN = cond(r2)",
                "3|L--|C = false");
        }

        [Test]
        public void VaxRw_ashp()
        {
            BuildTest(0xF8, 0x08, 0x53, 0x52, 0x51, 0x08, 0x54);	// ashp	
            AssertCode(
                "0|L--|00010000(7): 2 instructions",
                "1|L--|VZN = vax_ashp(0x08, r3, (word16) r2, r1, 0x0008, r4)");
        }

        [Test]
        public void VaxRw_ashq()
        {
            BuildTest(0x79, 0x02, 0x5A, 0x5B);	// ashq	#02,r10,r11
            AssertCode(
                "0|L--|00010000(4): 3 instructions",
                "1|L--|r11 = r10 << 2",
                "2|L--|VZN = cond(r11)",
                "3|L--|C = false");
        }

        [Test]
        public void VaxRw_bbc()
        {
            BuildTest(0xE1, 0x07, 0xE6, 0xF0, 0x02, 0x01, 0x00, 0x07);	// bbc	#00000007,+000102F0(r6),0000A7D8
            AssertCode(
                "0|L--|00010000(8): 1 instructions",
                "1|T--|if ((Mem0[r6 + 0x000102F0:word32] & 0x00000001 << 0x00000007) == 0x00000000) branch 0001000F");
        }

        [Test]
        [Ignore]
        public void VaxRw_bbcc()
        {
            BuildTest(0xE5, 0x02, 0x52, 0x34);	// bbcc	
            AssertCode(
                "0|L--|00010000(8): 1 instructions",
                "1|T--|if ((r2 & 0x00000001 << 0x00000002) == 0x00000000) branch 0001000F");
        }

        [Test]
        public void VaxRw_bbs()
        {
            BuildTest(0xE0, 0x03, 0xA2, 0x14, 0x07);	// bbs	#00000003,+14(r2),00009CB8
            AssertCode(
                "0|L--|00010000(5): 1 instructions",
                "1|T--|if ((Mem0[r2 + 20:word32] & 0x00000001 << 0x00000003) != 0x00000000) branch 0001000C");
        }

        [Test]
        public void VaxRw_beql()
        {
            BuildTest(0x13, 0x2E);	// beql	000080FD
            AssertCode(
                "0|T--|00010000(2): 1 instructions",
                "1|T--|if (Test(EQ,Z)) branch 00010030");
        }

        [Test]
        public void VaxRw_bgequ()
        {
            BuildTest(0x1E, 0x2B);	// bgequ	00009866
            AssertCode(
                "0|T--|00010000(2): 1 instructions",
                "1|T--|if (Test(UGE,C)) branch 0001002D");
        }

        [Test]
        public void VaxRw_bgeq()
        {
            BuildTest(0x18, 0x03);	// bgeq	00008378
            AssertCode(
                "0|T--|00010000(2): 1 instructions",
                "1|T--|if (Test(GE,N)) branch 00010005");
        }

        [Test]
        public void VaxRw_bgtr()
        {
            BuildTest(0x14, 0x03);	// bgtr	00008178
            AssertCode(
                "0|T--|00010000(2): 1 instructions",
                "1|T--|if (Test(GT,ZN)) branch 00010005");
        }

        [Test]
        public void VaxRw_bgtru()
        {
            BuildTest(0x1A, 0x29);	// bgtru	0000B43C
            AssertCode(
                "0|T--|00010000(2): 1 instructions",
                "1|T--|if (Test(UGT,CZ)) branch 0001002B");
        }

        [Test]
        public void VaxRw_bleq()
        {
            BuildTest(0x15, 0x42);	// bleq	00008128
            AssertCode(
                "0|T--|00010000(2): 1 instructions",
                "1|T--|if (Test(LE,ZN)) branch 00010044");
        }

        [Test]
        public void VaxRw_blequ()
        {
            BuildTest(0x1B, 0x16);	// blequ	00008F6E
            AssertCode(
                "0|T--|00010000(2): 1 instructions",
                "1|T--|if (Test(ULE,CZ)) branch 00010018");
        }

        [Test]
        public void VaxRw_blss()
        {
            BuildTest(0x19, 0x04);	// blss	00008155
            AssertCode(
                "0|T--|00010000(2): 1 instructions",
                "1|T--|if (Test(LT,N)) branch 00010044");
        }

        [Test]
        public void VaxRw_blssu()
        {
            BuildTest(0x19, 0x04);	// blss	00008155
            AssertCode(
                "0|T--|00010000(2): 1 instructions",
                "1|T--|if (Test(ULT,C)) branch 00010006");
        }

        [Test]
        public void VaxRw_bneq()
        {
            BuildTest(0x12, 0x02);	// bneq	00008081
            AssertCode(
                "0|T--|00010000(2): 1 instructions",
                "1|T--|if (Test(NE,Z)) branch 00010004");
        }

        [Test]
        public void VaxRw_bicb2()
        {
            BuildTest(0x8A, 0x8F, 0x80, 0x50);	// bicb2	#80,r0
            AssertCode(
                "0|L--|00010000(4): 5 instructions",
                "1|L--|v3 = (byte) r0 & ~0x80",
                "2|L--|r0 = DPB(r0, v3, 0)",
                "3|L--|ZN = cond(v3)",
                "4|L--|C = false",
                "5|L--|V = false");
        }

        [Test]
        public void VaxRw_bicb3()
        {
            BuildTest(0x8B, 0x8F, 0xF0, 0xE6, 0xF4, 0x02, 0x01, 0x00, 0x52);	// bicb3	#F0,+000102F4(r6),r2
            AssertCode(
                "0|L--|00010000(9): 5 instructions",
                "1|L--|v4 = Mem0[r6 + 0x000102F4:byte] & ~0xF0",
                "2|L--|r2 = DPB(r2, v4, 0)",
                "3|L--|ZN = cond(v4)",
                "4|L--|C = false",
                "5|L--|V = false");
        }

        [Test]
        public void VaxRw_bicl2()
        {
            BuildTest(0xCA, 0x8F, 0x80, 0xFF, 0xFF, 0xFF, 0x52);	// bicl2	#FFFFFF80,r2
            AssertCode(
                "0|L--|00010000(7): 4 instructions",
                "1|L--|r2 = r2 & ~0xFFFFFF80",
                "2|L--|ZN = cond(r2)",
                "3|L--|C = false",
                "4|L--|V = false");
        }

        [Test]
        public void VaxRw_bicl3()
        {
            BuildTest(0xCB, 0x8F, 0xFE, 0xFF, 0xFF, 0xFF, 0x52, 0x53);	// bicl3	#FFFFFFFE,r2,r3
            AssertCode(
                "0|L--|00010000(8): 4 instructions",
                "1|L--|r3 = r2 & ~0xFFFFFFFE",
                "2|L--|ZN = cond(r3)",
                "3|L--|C = false",
                "4|L--|V = false");
        }

        [Test]
        public void VaxRw_bicw2()
        {
            BuildTest(0xAA, 0xF0, 0xA9, 0xEE, 0xF8, 0xF1, 0xFD, 0xFC, 0xEF, 0xE6, 0xF4);	// bicw2	-0E071157(r0),-0B191004(fp)
            AssertCode(
                "0|L--|00010000(11): 5 instructions",
                "1|L--|v4 = Mem0[fp + 0xF4E6EFFC:word16] & ~Mem0[r0 + 0xF1F8EEA9:word16]",
                "2|L--|Mem0[fp + 0xF4E6EFFC:word16] = v4",
                "3|L--|ZN = cond(v4)",
                "4|L--|C = false",
                "5|L--|V = false");
        }


        [Test]
        public void VaxRw_bicw3()
        {
            BuildTest(0xAB, 0x8F, 0x00, 0x00, 0x52, 0xAE, 0x0E);	// bicw3	#0000,r2,+0E(sp)
            AssertCode(
                "0|L--|00010000(7): 5 instructions",
                "1|L--|v4 = (word16) r2 & ~0x0000",
                "2|L--|Mem0[sp + 14:word16] = v4",
                "3|L--|ZN = cond(v4)",
                "4|L--|C = false",
                "5|L--|V = false");
        }


        [Test]
        public void VaxRw_bisb2()
        {
            BuildTest(0x88, 0xE1, 0xFE, 0x7F, 0xD0, 0x50, 0x52);	// bisb2	+50D07FFE(r1),r2
            AssertCode(
                "0|L--|00010000(7): 5 instructions",
                "1|L--|v4 = (byte) r2 | Mem0[r1 + 0x50D07FFE:byte]",
                "2|L--|r2 = DPB(r2, v4, 0)",
                "3|L--|ZN = cond(v4)",
                "4|L--|C = false",
                "5|L--|V = false");
        }

        [Test]
        public void VaxRw_bisb3()
        {
            BuildTest(0x89, 0xF3, 0xD4, 0x50, 0x04, 0xD5, 0x50, 0x7B);	// bisb3	-2AFBAF2C(r3),r0,-(r11)
            AssertCode(
                "0|L--|00010000(8): 6 instructions",
                "1|L--|r11 = r11 - 0x00000001", 
                "2|L--|v5 = (byte) r0 | Mem0[r3 + 0xD50450D4:byte]",
                "3|L--|Mem0[r11:byte] = v5",
                "4|L--|ZN = cond(v5)",
                "5|L--|C = false",
                "6|L--|V = false");
        }

        [Test]
        public void VaxRw_bisl2()
        {
            BuildTest(0xC8, 0xE1, 0xFE, 0x7F, 0xD0, 0x50, 0x54);	// bisl2	+50D07FFE(r1),r4
            AssertCode(
                "0|L--|00010000(7): 4 instructions",
                "1|L--|r4 = r4 | Mem0[r1 + 0x50D07FFE:word32]",
                "2|L--|ZN = cond(r4)",
                "3|L--|C = false",
                "4|L--|V = false");
        }

        [Test]
        public void VaxRw_bisl3()
        {
            BuildTest(0xC9, 0xE2, 0xA8, 0x0A, 0x01, 0x00, 0xE2, 0xAC, 0x0A, 0x01, 0x00, 0x5C);	// bisl3	+00010AA8(r2),+00010AAC(r2),ap
            AssertCode(
                "0|L--|00010000(12): 4 instructions",
                "1|L--|ap = Mem0[r2 + 0x00010AAC:word32] | Mem0[r2 + 0x00010AA8:word32]",
                "2|L--|ZN = cond(ap)",
                "3|L--|C = false",
                "4|L--|V = false");
        }

        [Test]
        public void VaxRw_bisw2()
        {
            BuildTest(0xA8, 0x05, 0xE6, 0x22, 0x02, 0x01, 0x00);	// bisw2	#0005,+00010222(r6)
            AssertCode(
                "0|L--|00010000(7): 5 instructions",
                "1|L--|v3 = Mem0[r6 + 0x00010222:word16] | 0x0005",
                "2|L--|Mem0[r6 + 0x00010222:word16] = v3",
                "3|L--|ZN = cond(v3)",
                "4|L--|C = false",
                "5|L--|V = false");
        }

        [Test]
        public void VaxRw_bisw3()
        {
            BuildTest(0xA9, 0x01, 0xA9, 0x00, 0xA9, 0x00);	// bisw3	#0001,+00(r9),+00(r9)
            AssertCode(
                "0|L--|00010000(6): 5 instructions",
                "1|L--|v3 = Mem0[r9 + 0:word16] | 0x0001",
                "2|L--|Mem0[r9 + 0:word16] = v3",
                "3|L--|ZN = cond(v3)",
                "4|L--|C = false",
                "5|L--|V = false");
        }

        [Test]
        public void VaxRw_bpt()
        {
            BuildTest(0x03);	// bpt	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|vax_bpt()");
        }


        [Test]
        public void VaxRw_clrb()
        {
            BuildTest(0x94, 0x50);	// clrb	r0
            AssertCode(
                "0|L--|00010000(2): 6 instructions",
                "1|L--|v3 = 0x00",
                "2|L--|r0 = DPB(r0, v3, 0)",
                "3|L--|Z = true",
                "4|L--|N = false",
                "5|L--|C = false",
                "6|L--|V = false");
        }

        [Test]
        public void VaxRw_clrl()
        {
            BuildTest(0xD4, 0x53);	// clrl	r3
            AssertCode(
                "0|L--|00010000(2): 5 instructions",
                "1|L--|r3 = 0x00000000",
                "2|L--|Z = true",
                "3|L--|N = false",
                "4|L--|C = false",
                "5|L--|V = false");
        }

        [Test]
        public void VaxRw_clrq()
        {
            BuildTest(0x7C, 0x81);	// clrq	(r1)+
            AssertCode(
                "0|L--|00010000(2): 7 instructions",
                "1|L--|v3 = 0x0000000000000000",
                "2|L--|Mem0[r1:word64] = v3",
                "3|L--|r1 = r1 + 0x00000008",
                "4|L--|Z = true",
                "5|L--|N = false",
                "6|L--|C = false",
                "7|L--|V = false");
        }

        [Test]
        public void VaxRw_clrw()
        {
            BuildTest(0xB4, 0xCD, 0xE8, 0xFE);	// clrw	-0118(fp)
            AssertCode(
                "0|L--|00010000(4): 6 instructions",
                "1|L--|v3 = 0x0000",
                "2|L--|Mem0[fp + -280:word16] = v3",
                "3|L--|Z = true",
                "4|L--|N = false",
                "5|L--|C = false",
                "6|L--|V = false");
        }

        [Test]
        public void VaxRw_cmpb()
        {
            BuildTest(0x91, 0xB7, 0x00, 0x2D);	// cmpb	+00(r7),#2D
            AssertCode(
                "0|L--|00010000(4): 2 instructions",
                "1|L--|CZN = cond(Mem0[r7 + 0:byte] - 0x2D)",
                "2|L--|V = false");
        }

        [Test]
        public void VaxRw_cmpl()
        {
            BuildTest(0xD1, 0xAC, 0x04, 0x01);	// cmpl	+04(ap),#00000001
            AssertCode(
                "0|L--|00010000(4): 2 instructions",
                "1|L--|CZN = cond(Mem0[ap + 4:word32] - 0x2D)",
                "2|L--|V = false");
        }

        [Test]
        public void VaxRw_cmpw()
        {
            BuildTest(0xB1, 0xBE, 0x10, 0xCB, 0x3F, 0x03);	// cmpw	+10(sp),+033F(r11)
            AssertCode(
                "0|L--|00010000(6): 2 instructions",
                "1|L--|CZN = cond(Mem0[sp + 16:word16] - Mem0[r11 + 831:word16])",
                "2|L--|V = false");
        }

        [Test]
        public void VaxRw_decl()
        {
            BuildTest(0xD7, 0x58);	// decl	r8
            AssertCode(
                "0|L--|00010000(2): 2 instructions",
                "1|L--|r8 = r8 - 0x00000001",
                "2|L--|CVZN = cond(r8)");
        }

        [Test]
        public void VaxRw_decw()
        {
            BuildTest(0xB7, 0xAE, 0x46);	// decw	+46(sp)
            AssertCode(
                "0|L--|00010000(3): 3 instructions",
                "1|L--|v3 = Mem0[sp + 70:word16] - 0x0001",
                "2|L--|Mem0[sp + 70:word16] = v3",
                "3|L--|CVZN = cond(v3)");
        }

        [Test]
        public void VaxRw_divb3()
        {
            BuildTest(0x87, 0x53, 0x51, 0x90);	// divb3	r3,r1,@(R0)+
            AssertCode(
                "0|L--|00010000(4): 4 instructions",
                "1|L--|v5 = (byte) r1 / (byte) r3",
                "2|L--|Mem0[Mem0[r0:word32]:byte] = v5",
                "3|L--|r0 = r0 + 0x00000004",
                "4|L--|CVZN = cond(v5)");
        }

        [Test]
        public void VaxRw_divd2()
        {
            BuildTest(0x66, 0x24, 0xEA, 0x00, 0xEA, 0x00, 0xEA);	// divd2	#0.5,-15FF1600(r10)
            AssertCode(
                "0|L--|00010000(7): 3 instructions",
                "1|L--|v3 = Mem0[r10 + 0xEA00EA00:real64] / 12",
                "2|L--|Mem0[r10 + 0xEA00EA00:real64] = v3",
                "3|L--|CVZN = cond(v3)");
        }

        [Test]
        public void VaxRw_divd3()
        {
            BuildTest(0x67, 0x20, 0x5A, 0x65 );	// divd3	#80,r10,(r5)
            AssertCode(
                "0|L--|00010000(4): 3 instructions",
                "1|L--|v4 = r10 / 8",
                "2|L--|Mem0[r5:real64] = v4",
                "3|L--|CVZN = cond(v4)");
        }

        [Test]
        public void VaxRw_divf2()
        {
            BuildTest(0x46, 0x32, 0x57);	// divf2	
            AssertCode(
                "0|L--|00010000(3): 2 instructions",
                "1|L--|r7 = r7 / 40F",
                "2|L--|CVZN = cond(r7)");
        }

        [Test]
        public void VaxRw_divf3()
        {
            BuildTest(0x47, 0x32, 0x56, 0x68);	// divf3	
            AssertCode(
                "0|L--|00010000(4): 3 instructions",
                "1|L--|v4 = r6 / @@",
                "2|L--|Mem0[r8:real32] = v4",
                "3|L--|CVZN = cond(v4)");
        }

        [Test]
        public void VaxRw_divl2()
        {
            BuildTest(0xC6, 0x04, 0x50);	// divl2	#00000004,r0
            AssertCode(
                "0|L--|00010000(3): 2 instructions",
                "1|L--|r0 = r0 / 0x00000004",
                "2|L--|CVZN = cond(r0)");
        }

        [Test]
        public void VaxRw_divl3()
        {
            BuildTest(0xC7, 0x04, 0x50, 0xA2, 0x64);	// divl3	#00000004,r0,+64(r2)
            AssertCode(
                "0|L--|00010000(5): 3 instructions",
                "1|L--|v4 = r0 / 0x00000004",
                "1|L--|Mem0[r2 + 100:word32] = v4",
                "2|L--|CVZN = cond(v4)");
        }

        [Test]
        public void VaxRw_divp()
        {
            BuildTest(0x27, 0x00, 0x3B, 0x00, 0x11, 0x4B, 0xD5, 0x50, 0x01, 0x17);	// divp	#0000,#3B,#0000,#11,+0150(r5)[r11],#17
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_divw2()
        {
            BuildTest(0xA6, 0x2D, 0x2B);	// divw2	#002D,#002B
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_divw3()
        {
            BuildTest(0xA7, 0x00, 0xAC, 0x00, 0xAC, 0x00);	// divw3	#0000,+00(ap),+00(ap)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_subl2()
        {
            BuildTest(0xC2, 0x04, 0x5E);	// subl2	#00000004,sp
            AssertCode(
                "0|L--|00010000(5): 3 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_jsb()
        {
            BuildTest(0x16, 0xFF, 0xBD, 0x12, 0x01, 0x00);	// jsb	000192C8
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_calls()
        {
            BuildTest(0xFB, 0x00, 0xEF, 0x0E, 0x54, 0x00, 0x00);	// calls	#00,0000D420
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movl()
        {
            BuildTest(0xD0, 0x01, 0x50);	// movl	#00000001,r0
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_pushl()
        {
            BuildTest(0xDD, 0xAC, 0x08);	// pushl	+08(ap)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_ret()
        {
            BuildTest(0x04);	// ret	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_xfc()
        {
            BuildTest(0xFC);	// xfc	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_svpctx()
        {
            BuildTest(0x07);	// svpctx	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movab()
        {
            BuildTest(0x9E, 0xEF, 0xC9, 0xD3, 0xFD, 0xFF, 0x57);	// movab	FFFE5400,r7
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_moval()
        {
            BuildTest(0xDE, 0xEF, 0x81, 0x5B, 0x00, 0x00, 0x54);	// moval	0000DBD4,r4
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }



        [Test]
        public void VaxRw_pushal()
        {
            BuildTest(0xDF, 0xAC, 0x08);	// pushal	+08(ap)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_blbs()
        {
            BuildTest(0xE8, 0x50, 0x04);	// blbs	r0,0000809C
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movzbl()
        {
            BuildTest(0x9A, 0x8F, 0x5D, 0x7E);	// movzbl	#5D,-(sp)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_brb()
        {
            BuildTest(0x11, 0x05);	// brb	000080BA
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_tstl()
        {
            BuildTest(0xD5, 0x50);	// tstl	r0
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_nop()
        {
            BuildTest(0x01);	// nop	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }



        [Test]
        public void VaxRw_brw()
        {
            BuildTest(0x31, 0x9C, 0x01);	// brw	00008314
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_subl3()
        {
            BuildTest(0xC3, 0x04, 0xAC, 0x08, 0x54);	// subl3	#00000004,+08(ap),r4
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_tstb()
        {
            BuildTest(0x95, 0xA4, 0x02);	// tstb	+02(r4)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movzwl()
        {
            BuildTest(0x3C, 0x8F, 0x01, 0x04, 0x7E);	// movzwl	#0401,-(sp)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_pushab()
        {
            BuildTest(0x9F, 0xC2, 0xEB, 0x05);	// pushab	+05EB(r2)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

 
        [Test]
        public void VaxRw_remque()
        {
            BuildTest(0x0F, 0xC2, 0x04, 0x5E, 0x9E);	// remque	+5E04(r2),(sp)+
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_extzv()
        {
            BuildTest(0xEF, 0xA5, 0x30, 0xFF, 0xFF, 0x52, 0x9E, 0xEF, 0xB6, 0xE6, 0xFD, 0xFF, 0x55, 0x7C, 0x59);	// extzv	+30(r5),EF9ED62D,-1A(r6),+597C55FF(fp)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_cvtbl()
        {
            BuildTest(0x98, 0x61, 0x56);	// cvtbl	(r1),r6
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_casel()
        {
            BuildTest(0xCF);	// casel	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movf()
        {
            BuildTest(0x50, 0x8F, 0x43, 0x00, 0x00, 0x00, 0x37);	// movf	#4.76441477870438E-44,#60
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        [Ignore]
        public void VaxRw_bbsc()
        {
            BuildTest(0xE4);	// bbsc	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtbf()
        {
            BuildTest(0x4C);	// cvtbf	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_rei()
        {
            BuildTest(0x02);	// rei	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cmpv()
        {
            BuildTest(0xEC);	// cmpv	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_insqhi()
        {
            BuildTest(0x5C);	// insqhi	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_emodd()
        {
            BuildTest(0x74, 0x04, 0xE4, 0x04, 0xBC, 0x04, 0xE4, 0x04, 0xE4, 0x04, 0xE4, 0x04, 0xE4, 0x04);	// emodd	#0.75,-1BFB43FC(r4),#0.75,-1BFB1BFC(r4),#0.75
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mulb2()
        {
            BuildTest(0x84, 0x00, 0xA8, 0x00);	// mulb2	#00,+00(r8)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movc3()
        {
            BuildTest(0x28, 0x02, 0xE4, 0x04, 0x74, 0x02, 0x88, 0x02);	// movc3	#0002,-77FD8BFC(r4),#02
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movc5()
        {
            BuildTest(0x2C);	// movc5	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movd()
        {
            BuildTest(0x70, 0x04, 0xE4, 0x04, 0x98, 0x04, 0x31);	// movd	#0.75,+31049804(r4)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cmpd()
        {
            BuildTest(0x71, 0x04, 0x01);	// cmpd	#0.75,#0.5625
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_jmp()
        {
            BuildTest(0x17, 0xEF, 0xF2, 0xFB, 0xFF, 0x3F);	// jmp	40008000
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_mnegl()
        {
            BuildTest(0xCE, 0x01, 0xBC, 0x04);	// mnegl	#00000001,+04(ap)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movb()
        {
            BuildTest(0x90, 0x01, 0x50);	// movb	#01,r0
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }



        [Test]
        public void VaxRw_blbc()
        {
            BuildTest(0xE9, 0x50, 0x03);	// blbc	r0,00009011
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_cvtlb()
        {
            BuildTest(0xF6, 0x50, 0x84);	// cvtlb	r0,(r4)+
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtwl()
        {
            BuildTest(0x32);	// cvtwl	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_caseb()
        {
            BuildTest(0x8F, 0x01, 0x00, 0x50);	// caseb	#01,#00,r0
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_incb()
        {
            BuildTest(0x96, 0x89);	// incb	(r9)+
            AssertCode(
                "0|L--|00010000(2): 4 instructions",
                "1|L--|v3 = Mem0[r9:byte] + 0x01",
                "2|L--|Mem0[r9:byte] = v3",
                "3|L--|r9 = r9 + 0x00000001",
                "4|L--|CVZN = cond(v3)");
        }

        [Test]
        public void VaxRw_incl()
        {
            BuildTest(0xD6, 0x53);	// incl	r3
            AssertCode(
                "0|L--|00010000(2): 2 instructions",
                "1|L--|r3 = r3 + 0x00000001",
                "2|L--|CVZN = cond(r3)");
        }

        [Test]
        public void VaxRw_incw()
        {
            BuildTest(0xB6, 0xAE, 0x32);	// incw	+32(sp)
            AssertCode(
                "0|L--|00010000(3): 3 instructions",
                "1|L--|v3 = Mem0[sp + 50:word16] + 0x0001",
                "2|L--|Mem0[sp + 50:word16] = v3",
                "3|L--|CVZN = cond(v3)");
        }


        [Test]
        public void VaxRw_subp6()
        {
            BuildTest(0x23, 0x3D, 0x0A, 0xD7, 0x70, 0x3D, 0xD7, 0xA3, 0x7C, 0x00, 0x9E);	// subp6	#003D,#0A,+3D70(r7),+7CA3(r7),#0000,(sp)+
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mnegw()
        {
            BuildTest(0xAE, 0xAC, 0x5E, 0x9E);	// mnegw	+5E(ap),(sp)+
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }



        [Test]
        public void VaxRw_cvtps()
        {
            BuildTest(0x08, 0xE2, 0xFE, 0x7F, 0xE8, 0x50, 0x2B, 0xDD, 0x01, 0xDD, 0xEC, 0x13, 0xC6, 0x00, 0x00);	// cvtps	+50E87FFE(r2),#2B,-22FF(fp),+0000C613(ap)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_bispsw()
        {
            BuildTest(0xB8, 0xE1, 0xFE, 0x7F, 0xD1, 0x52);	// bispsw	+52D17FFE(r1)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movpsl()
        {
            BuildTest(0xDC);	// movpsl	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_Invalid()
        {
            BuildTest(0xFD);	// Invalid	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtfd()
        {
            BuildTest(0x56);	// cvtfd	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }



        [Test]
        public void VaxRw_adawi()
        {
            BuildTest(0x58);	// adawi	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movw()
        {
            BuildTest(0xB0, 0x8F, 0x00, 0x02, 0xA2, 0x36);	// movw	#0200,+36(r2)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_movp()
        {
            BuildTest(0x34);	// movp	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtpl()
        {
            BuildTest(0x36);	// cvtpl	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_subp4()
        {
            BuildTest(0x22, 0x00, 0x22, 0x00, 0x22);	// subp4	#0000,#22,#0000,#22
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mnegb()
        {
            BuildTest(0x8E, 0x01, 0xC6, 0xDE, 0x00);	// mnegb	#01,+00DE(r6)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_bsbw()
        {
            BuildTest(0x30, 0xE2, 0xFE);	// bsbw	0000A5FF
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_pushaq()
        {
            BuildTest(0x7F, 0xE8, 0x50, 0x0F, 0xDD, 0x50);	// pushaq	+50DD0F50(r8)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtbw()
        {
            BuildTest(0x99, 0xC6, 0xE8, 0x00, 0xE6, 0x1C, 0x03, 0x01, 0x00);	// cvtbw	+00E8(r6),+0001031C(r6)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtlw()
        {
            BuildTest(0xF7, 0x52, 0xE6, 0x24, 0x03, 0x01, 0x00);	// cvtlw	r2,+00010324(r6)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movaw()
        {
            BuildTest(0x3E);	// movaw	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        [Ignore]
        public void VaxRw_bbssi()
        {
            BuildTest(0xE6);	// bbssi	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_bsbb()
        {
            BuildTest(0x10, 0x02);	// bsbb	0000A7A0
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mnegf()
        {
            BuildTest(0x52, 0x3E, 0xE6, 0xFE, 0x00, 0x00, 0x00);	// mnegf	#112,+000000FE(r6)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_tstf()
        {
            BuildTest(0x53, 0xB0, 0x63);	// tstf	+63(r0)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_subw2()
        {
            BuildTest(0xA2, 0x06, 0xB4, 0x62);	// subw2	#0006,+62(r4)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }



        [Test]
        public void VaxRw_movzbw()
        {
            BuildTest(0x9B, 0x8F, 0x80, 0xE6, 0x22, 0x02, 0x01, 0x00);	// movzbw	#80,+00010222(r6)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_remqhi()
        {
            BuildTest(0x5E);	// remqhi	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movaq()
        {
            BuildTest(0x7E, 0x00, 0x95);	// movaq	#00000000,(r5)+
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_Reserved()
        {
            BuildTest(0x57);	// Reserved	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mulf2()
        {
            BuildTest(0x44);	// mulf2	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtdb()
        {
            BuildTest(0x68, 0x00, 0x68);	// cvtdb	#0.5,(r8)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_prober()
        {
            BuildTest(0x0C, 0x00, 0xC2, 0x04, 0x5E, 0x9E);	// prober	#00,+5E04(r2),(sp)+
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_bvc()
        {
            BuildTest(0x1C, 0x00);	// bvc	0000B192
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }



        [Test]
        [Ignore]
        public void VaxRw_bbss()
        {
            BuildTest(0xE2, 0xFC, 0x01, 0x01, 0x00, 0x53, 0xE8, 0x53, 0x0F, 0xDD, 0x53, 0xDF);	// bbss	+53000101(ap),+53DD0F53(r8),0000B1AC
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mull2()
        {
            BuildTest(0xC4, 0xC8, 0x03, 0xFB, 0x02);	// mull2	-04FD(r8),#00000002
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtrfl()
        {
            BuildTest(0x4B);	// cvtrfl	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_insque()
        {
            BuildTest(0x0E, 0xD0, 0x32, 0x50, 0x04);	// insque	+5032(r0),#04
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mcoml()
        {
            BuildTest(0xD2, 0x52, 0x52);	// mcoml	r2,r2
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

  

        [Test]
        public void VaxRw_mull3()
        {
            BuildTest(0xC5, 0x8F, 0x6D, 0x01, 0x00, 0x00, 0x53, 0x52);	// mull3	#0000016D,r3,r2
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_scanc()
        {
            BuildTest(0x2A, 0x05, 0x01, 0x00, 0x52);	// scanc	#0005,#01,#00,r2
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_emul()
        {
            BuildTest(0x7A);	// emul	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_ediv()
        {
            BuildTest(0x7B);	// ediv	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_subd2()
        {
            BuildTest(0x62, 0xC0, 0x02, 0x53, 0xC0, 0x02, 0x52);	// subd2	+5302(r0),+5202(r0)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtfb()
        {
            BuildTest(0x48);	// cvtfb	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_index()
        {
            BuildTest(0x0A, 0x01, 0x00, 0xD4, 0x50, 0x04, 0x9A, 0xE6, 0x18, 0x0A, 0x01, 0x00, 0x52);	// index	#00000001,#00000000,+0450(r4),(r10)+,+00010A18(r6),r2
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_chme()
        {
            BuildTest(0xBD);	// chme	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mulf3()
        {
            BuildTest(0x45);	// mulf3	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_emodf()
        {
            BuildTest(0x54);	// emodf	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtlf()
        {
            BuildTest(0x4E, 0x52, 0x52);	// cvtlf	r2,r2
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtdl()
        {
            BuildTest(0x6A, 0x54, 0x52);	// cvtdl	r4,r2
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_ldpctx()
        {
            BuildTest(0x06);	// ldpctx	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_pushaw()
        {
            BuildTest(0x3F);	// pushaw	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_cmpf()
        {
            BuildTest(0x51, 0xD5, 0x51, 0x12, 0x50);	// cmpf	+1251(r5),r0
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_tstw()
        {
            BuildTest(0xB5, 0xAE, 0x06);	// tstw	+06(sp)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_insv()
        {
            BuildTest(0xF0);	// insv	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

   

        [Test]
        public void VaxRw_rotl()
        {
            BuildTest(0x9C);	// rotl	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_rsb()
        {
            BuildTest(0x05);	// rsb	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_acbf()
        {
            BuildTest(0x4F);	// acbf	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_bvs()
        {
            BuildTest(0x1D, 0x00);	// bvs	000106ED
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

 

        [Test]
        public void VaxRw_mulb3()
        {
            BuildTest(0x85, 0xFE, 0xFF, 0x5A, 0xD4, 0x5B, 0xD4, 0x53, 0xD4, 0x6E);	// mulb3	+5BD45AFF(sp),-2BAD(r4),(sp)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_polyf()
        {
            BuildTest(0x55);	// polyf	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movtc()
        {
            BuildTest(0x2E);	// movtc	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cmpp4()
        {
            BuildTest(0x37);	// cmpp4	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_acbl()
        {
            BuildTest(0xF1);	// acbl	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_sbwc()
        {
            BuildTest(0xD9);	// sbwc	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtwf()
        {
            BuildTest(0x4D);	// cvtwf	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_sobgtr()
        {
            BuildTest(0xF5, 0x00, 0xD9);	// sobgtr	#00000000,000127B5
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cmpp3()
        {
            BuildTest(0x35);	// cmpp3	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_locc()
        {
            BuildTest(0x3A);	// locc	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtld()
        {
            BuildTest(0x6E, 0x00, 0x6E);	// cvtld	#00000000,(sp)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_tstd()
        {
            BuildTest(0x73, 0x00);	// tstd	#0.5
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_crc()
        {
            BuildTest(0x0B, 0xD1, 0x52, 0x50, 0x1A, 0x0B, 0xD6, 0x51, 0x11);	// crc	+5052(r1),#0000001A,#000B,+1151(r6)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_editpc()
        {
            BuildTest(0x38);	// editpc	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_acbw()
        {
            BuildTest(0x3D);	// acbw	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtsp()
        {
            BuildTest(0x09, 0x07, 0x00, 0x00, 0x5A);	// cvtsp	#0007,#00,#0000,r10
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtwb()
        {
            BuildTest(0x33);	// cvtwb	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtbd()
        {
            BuildTest(0x6C, 0x00, 0x00);	// cvtbd	#00,#0.5
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_sobgeq()
        {
            BuildTest(0xF4, 0x00, 0x00);	// sobgeq	#00000000,00019277
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_muld2()
        {
            BuildTest(0x64, 0x01, 0x00);	// muld2	#0.5625,#0000
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

 

        [Test]
        public void VaxRw_cvtpt()
        {
            BuildTest(0x24, 0x00, 0x00, 0x00, 0x22);	// cvtpt	#0000,#00,#00,#0022
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_probew()
        {
            BuildTest(0x0D, 0x00, 0x00, 0x00);	// probew	#00,#0000,#00
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }



        [Test]
        public void VaxRw_subf3()
        {
            BuildTest(0x43);	// subf3	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtfw()
        {
            BuildTest(0x49);	// cvtfw	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_subf2()
        {
            BuildTest(0x42);	// subf2	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtfl()
        {
            BuildTest(0x4A);	// cvtfl	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_matchc()
        {
            BuildTest(0x39);	// matchc	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mulw3()
        {
            BuildTest(0xA5, 0x01, 0x00, 0x00);	// mulw3	#0001,#0000,#0000
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_remqti()
        {
            BuildTest(0x5F);	// remqti	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtdw()
        {
            BuildTest(0x69, 0x70, 0x20);	// cvtdw	-(r0),#0020
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_muld3()
        {
            BuildTest(0x65, 0x00, 0x00, 0x38);	// muld3	#0.5,#0.5,#64
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mulw2()
        {
            BuildTest(0xA4, 0x08, 0x00);	// mulw2	#0008,#0000
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cmpc5()
        {
            BuildTest(0x2D);	// cmpc5	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtwd()
        {
            BuildTest(0x6D, 0x65, 0x6D);	// cvtwd	(r5),(fp)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_polyd()
        {
            BuildTest(0x75, 0x6F, 0x74, 0x65);	// polyd	(pc),-(r4),(r5)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_acbd()
        {
            BuildTest(0x6F, 0x20, 0x25, 0x64, 0x2E);	// acbd	#8,#13,(r4),00001E1B
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mulp()
        {
            BuildTest(0x25, 0x64, 0x25, 0x64, 0x25, 0x73, 0x20);	// mulp	(r4),#25,(r4),#25,-(r3),#20
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cvtrdl()
        {
            BuildTest(0x6B, 0x73, 0x20);	// cvtrdl	-(r3),#00000020
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_movtuc()
        {
            BuildTest(0x2F);	// movtuc	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_subd3()
        {
            BuildTest(0x63, 0x61, 0x72, 0x64);	// subd3	(r1),-(r2),(r4)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mnegd()
        {
            BuildTest(0x72, 0x20, 0x31);	// mnegd	#8,#36
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_cmpc3()
        {
            BuildTest(0x29, 0x5D, 0x20, 0x5B);	// cmpc3	fp,#20,r11
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_insqti()
        {
            BuildTest(0x5D);	// insqti	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }



        [Test]
        public void VaxRw_cvtdf()
        {
            BuildTest(0x76, 0x65, 0x72);	// cvtdf	(r5),-(r2)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }



        [Test]
        public void VaxRw_skpc()
        {
            BuildTest(0x3B);	// skpc	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_acbb()
        {
            BuildTest(0x9D);	// acbb	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_bicpsw()
        {
            BuildTest(0xB9, 0x5D);	// bicpsw	fp
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_subb2()
        {
            BuildTest(0x82, 0x53, 0x01);	// subb2	r3,#01
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }


        [Test]
        public void VaxRw_mcomb()
        {
            BuildTest(0x92);	// mcomb	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_popr()
        {
            BuildTest(0xBA);	// popr	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_spanc()
        {
            BuildTest(0x2B, 0x00, 0x2C, 0x00, 0x2D);	// spanc	#0000,#2C,#00,#2D
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_addb3()
        {
            BuildTest(0x81, 0x01, 0x55, 0xC1, 0x00, 0x01);	// addb3	#01,r5,+0100(r1)
            AssertCode(
                "0|L--|00010000(6): 3 instructions",
                "1|L--|v4 = (byte) r5 + 0x01",
                "2|L--|Mem0[r1 + 256:byte] = v4",
                "3|L--|CVZN = cond(v4)");
        }

        [Test]
        public void VaxRw_subb3()
        {
            BuildTest(0x83, 0x00, 0xA3, 0x00, 0xC3, 0x00, 0xE3);	// subb3	#00,+00(r3),-1D00(r3)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

 
        [Test]
        public void VaxRw_callg()
        {
            BuildTest(0xFA);	// callg	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_ffc()
        {
            BuildTest(0xEB);	// ffc	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_subw3()
        {
            BuildTest(0xA3, 0x96, 0x81, 0xEC, 0xE7, 0x98, 0x00, 0x00);	// subw3	(r6)+,(r1)+,+000098E7(ap)
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mtpr()
        {
            BuildTest(0xDA);	// mtpr	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mfpr()
        {
            BuildTest(0xDB);	// mfpr	
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_mcomw()
        {
            BuildTest(0xB2, 0xA6, 0xA0, 0x20);	// mcomw	-60(r6),#0020
            AssertCode(
                "0|L--|00010000(2): 1 instructions",
                "1|L--|@@@");
        }

        [Test]
        public void VaxRw_xorb2()
        {
            BuildTest(0x8C, 0x02, 0x90);	// xorb2	#02,(r0)+
            AssertCode(
                "0|L--|00010000(3): 6 instructions",
                "1|L--|v3 = Mem0[r0:byte] ^ 0x02",
                "2|L--|Mem0[r0:byte] = v3",
                "3|L--|r0 = r0 + 0x00000001",
                "4|L--|ZN = cond(v3)",
                "5|L--|C = false",
                "6|L--|V = false");
        }

        [Test]
        public void VaxRw_xorb3()
        {
            BuildTest(0x8D, 0x5C, 0x52, 0x63);	// xorb3	ap,r2,(r3)
            AssertCode(
                "0|L--|00010000(4): 5 instructions",
                "1|L--|v5 = (byte) r2 ^ (byte) ap",
                "2|L--|Mem0[r3:byte] = v5",
                "3|L--|ZN = cond(v5)",
                "4|L--|C = false",
                "5|L--|V = false");
        }

        [Test]
        public void VaxRw_xorl2()
        {
            BuildTest(0xCC, 0x8F, 0xFF, 0xFF, 0xFF, 0xFF, 0x53);	// xorl2	#FFFFFFFF,r3
            AssertCode(
                "0|L--|00010000(7): 4 instructions",
                "1|L--|r3 = r3 ^ 0xFFFFFFFF",
                "2|L--|ZN = cond(r3)",
                "3|L--|C = false",
                "4|L--|V = false");
        }


        [Test]
        public void VaxRw_xorl3()
        {
            BuildTest(0xCD, 0xFD, 0xFF, 0x58, 0xD0, 0xEA, 0x27, 0xC6, 0x00, 0x00);	// xorl3	-152FA701(fp),#00000027,+0000(r6)
            AssertCode(
                "0|L--|00010000(10): 5 instructions",
                "1|L--|v4 = 0x00000027 ^ Mem0[fp + 0xEAD058FF:word32]",
                "2|L--|Mem0[r6 + 0:word32] = v4",
                "3|L--|ZN = cond(v4)",
                "4|L--|C = false",
                "5|L--|V = false");
        }

        [Test]
        public void VaxRw_xorw2()
        {
            BuildTest(0xAC, 0x02, 0xB4, 0x03);	// xorw2	#0002,+03(r4)
            AssertCode(
                "0|L--|00010000(4): 5 instructions",
                "1|L--|v3 = Mem0[r4 + 3:word16] ^ 0x0002",
                "2|L--|Mem0[r4 + 3:word16] = v3",
                "3|L--|ZN = cond(v3)",
                "4|L--|C = false",
                "5|L--|V = false");
        }

        [Test]
        public void VaxRw_xorw3()
        {
            BuildTest(0xAD, 0xAC, 0xD0, 0xEC, 0x13, 0xC6, 0x00, 0x00, 0xAD, 0xD8);	// xorw3	-30(ap),+0000C613(ap),-28(fp)
            AssertCode(
                "0|L--|00010000(10): 5 instructions",
                "1|L--|v4 = Mem0[ap + 0x0000C613:word16] ^ Mem0[ap + -48:word16]",
                "2|L--|Mem0[fp + -40:word16] = v4",
                "3|L--|ZN = cond(v4)",
                "4|L--|C = false",
                "5|L--|V = false");
        }
    }
}
