using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace dotNetCrypt.Protections.IntMutator {
    internal class IntMutation:IProtection {
        private static readonly Random rng = new Random(); 
        public static void Execute(ModuleDefMD module) {
            foreach(TypeDef type in module.Types) {
                foreach(MethodDef method in type.Methods) {
                    if(!method.HasBody)
                        continue; 
                    var instrs = method.Body.Instructions; 
                    for(int i = 0; i < instrs.Count; i++) {
                        Instruction instr = instrs[i]; 
                        if(!instr.IsLdcI4())
                            continue;
                        int value = instr.GetLdcI4Value();
                        int key = rng.Next(1000, int.MaxValue);
                        if(value == 0) {
                            instrs[i] = Instruction.Create(OpCodes.Ldc_I4, key);
                            instrs.Insert(i + 1, Instruction.Create(OpCodes.Ldc_I4, key));
                            instrs.Insert(i + 2, Instruction.Create(OpCodes.Xor));
                            i += 2;
                        } else if(value == 1) {
                            instrs[i] = Instruction.Create(OpCodes.Ldc_I4, key);
                            instrs.Insert(i + 1, Instruction.Create(OpCodes.Ldc_I4, key ^ 1));
                            instrs.Insert(i + 2, Instruction.Create(OpCodes.Xor));
                            i += 2;
                        } else if(value == -1) {
                            instrs[i] = Instruction.Create(OpCodes.Ldc_I4, key);
                            instrs.Insert(i + 1, Instruction.Create(OpCodes.Ldc_I4, ~key));
                            instrs.Insert(i + 2, Instruction.Create(OpCodes.Xor));
                            i += 2;
                        } else {
                            int encrypted = value ^ key;
                            instrs[i] = Instruction.Create(OpCodes.Ldc_I4, key);
                            instrs.Insert(i + 1, Instruction.Create(OpCodes.Ldc_I4, encrypted));
                            instrs.Insert(i + 2, Instruction.Create(OpCodes.Xor));
                            i += 2;
                        }
                    }
                }
            }
        }
    }
}
/// seperated becuase its better performance wise and would cause less errors at runtimes
//TODO: add a more strong version of the mutations