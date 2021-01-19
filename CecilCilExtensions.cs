using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace OpusPatcher {
    // copied from https://github.com/gtw123/ShenzhenMod/blob/74b03be6a991b27a9020d393d12412712a9b1ed3/ShenzhenMod/CecilCilExtensions.cs
    public static class CecilCilExtensions {

        public static bool Matches(this Instruction instruction, OpCode opCode, object operand) {
            return instruction.OpCode == opCode && Object.Equals(instruction.Operand, operand);
        }

        public static Instruction FindNext(this Instruction instruction, OpCode opCode) {
            var instr = instruction;
            while(instr != null) {
                if(instr.OpCode == opCode) {
                    return instr;
                }

                instr = instr.Next;
            }

            throw new Exception($"Cannot find instruction with OpCode \"{opCode}\" anywhere after instruction \"{instruction}\"");
        }

        public static void Set(this Instruction instruction, OpCode opCode, object operand) {
            instruction.OpCode = opCode;
            instruction.Operand = operand;
        }

        public static IEnumerable<Instruction> RemoveRange(this ILProcessor il, Instruction start, Instruction end) {
            var removed = new List<Instruction>();
            var current = start;
            while(current != end) {
                var next = current.Next;
                il.Remove(current);
                removed.Add(current);
                current = next;
            }

            return removed;
        }

        public static void InsertRangeBefore(this ILProcessor il, Instruction target, IEnumerable<Instruction> instructions) {
            foreach(var instr in instructions) {
                il.InsertBefore(target, instr);
            }
        }

        public static void InsertBefore(this ILProcessor il, Instruction target, params Instruction[] instructions) {
            InsertRangeBefore(il, target, instructions);
        }
    }
}
