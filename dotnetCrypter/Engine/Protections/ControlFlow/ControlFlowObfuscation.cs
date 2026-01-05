using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dotNetCrypt.Protections.ControlFlow {
    internal class ControlFlow {
        static readonly Random R = new Random();

        public static void Execute(ModuleDefMD md) { 
            foreach(var t in md.Types) {
                if(t == md.GlobalType) continue;
                foreach(var m in t.Methods) {
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(!m.HasBody || m.Body == null) continue;
                    if(m.IsAbstract || m.IsConstructor) continue;
                    if(m.Name.StartsWith("get_") || m.Name.StartsWith("set_")) continue;
                    if(m.Body.Instructions.Count < 1) continue;
                    Protect(m);
                }
            }  
        }

        static void Protect(MethodDef m) {
            var body = m.Body;
            body.SimplifyMacros(m.Parameters);
            body.SimplifyBranches(); 
            var blocks = BlockParser.ParseMethod(m);
            if(blocks == null || blocks.Count <= 1) return; 
            var state = new Local(m.Module.CorLibTypes.Int32);
            body.Variables.Add(state);
            body.InitLocals = true; 
            var shuffled = blocks.Where(b => b != null && b.Instructions.Count > 0).OrderBy(_ => R.Next()).ToList();
            if(shuffled.Count <= 1) return; 
            var originalAll = body.Instructions.ToList();
            body.Instructions.Clear(); 
            body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            body.Instructions.Add(Instruction.Create(OpCodes.Stloc, state)); 
            var blockLabels = new Instruction[shuffled.Count];
            for(int i = 0; i < blockLabels.Length; i++)
                blockLabels[i] = Instruction.Create(OpCodes.Nop); 
            var loopStart = Instruction.Create(OpCodes.Nop); 
            body.Instructions.Add(loopStart); 
            body.Instructions.Add(Instruction.Create(OpCodes.Ldloc, state)); 
            var switchEnd = Instruction.Create(OpCodes.Nop);
            body.Instructions.Add(Instruction.Create(OpCodes.Switch, blockLabels)); 
            body.Instructions.Add(Instruction.Create(OpCodes.Br, loopStart)); 
            for(int i = 0; i < shuffled.Count; i++) {
                var block = shuffled[i]; 
                body.Instructions.Add(blockLabels[i]); 
                foreach(var ins in block.Instructions) {
                    body.Instructions.Add(ins);
                } 
                int nextState;
                if(i == shuffled.Count - 1) { 
                    var lastIns = block.Instructions.LastOrDefault();
                    if(lastIns != null && (lastIns.OpCode == OpCodes.Ret || lastIns.OpCode == OpCodes.Throw)) { 
                        continue;
                    } else { 
                        nextState = 0;
                    }
                } else { 
                    nextState = i + 1;
                } 
                body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, nextState));
                body.Instructions.Add(Instruction.Create(OpCodes.Stloc, state));
                body.Instructions.Add(Instruction.Create(OpCodes.Br, loopStart)); 
                var continueLabel = Instruction.Create(OpCodes.Nop);
                body.Instructions.Add(continueLabel);
            }

            RemapExceptionHandlersNoClone(body, originalAll);
        }

        static void RemapExceptionHandlersNoClone(CilBody body, IList<Instruction> original) {
            if(body.ExceptionHandlers.Count == 0) return;
            var indexMap = new Dictionary<Instruction, int>();
            for(int i = 0; i < original.Count; i++) indexMap[original[i]] = i;
            var current = body.Instructions;
            Instruction FindNearest(Instruction orig) {
                if(orig == null) return null;
                if(!indexMap.TryGetValue(orig, out var idx)) return current.FirstOrDefault();
                if(idx < current.Count) return current[idx];
                return current.LastOrDefault();
            }
            foreach(var eh in body.ExceptionHandlers.ToList()) {
                eh.TryStart = FindNearest(eh.TryStart);
                eh.TryEnd = FindNearest(eh.TryEnd);
                eh.HandlerStart = FindNearest(eh.HandlerStart);
                eh.HandlerEnd = FindNearest(eh.HandlerEnd);
                if(eh.FilterStart != null) eh.FilterStart = FindNearest(eh.FilterStart);
            }
        }
    }
}