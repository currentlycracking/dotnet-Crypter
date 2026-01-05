using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dotnetCrypter.Engine.Core;

namespace dotnetCrypter.Engine.Protections.Proxy {
    public static class ExternalImports {
        public static List<MethodDef> ProxyMethods {
            get;
            private set;
        }

        static ExternalImports() {
            ProxyMethods = new List<MethodDef>();
        }

        static Random _rng = new Random();
        private static string GenerateInvisibleName() {
            var chars = new[] { '\u200B', '\u200C', '\u200D', '\u2060', '\u2061', '\u2062', '\u2063', '\u2064' };
            int length = _rng.Next(8, 16);
            var name = new char[length];
            for(int i = 0; i < length; i++) {
                name[i] = chars[_rng.Next(chars.Length)];
            }
            return new string(name);
        }

        public static void Execute(ModuleDef module) {
            ProxyCreator rPHelper = new ProxyCreator();

            foreach(TypeDef type in module.Types.ToArray()) {
                foreach(MethodDef method in type.Methods.ToArray()) {
                    if(ProxyMethods.Contains(method))
                        continue;
                    if(canObfuscate(method)) {
                        foreach(Instruction instruction in method.Body.Instructions.ToArray()) {
                            if(instruction.OpCode == OpCodes.Stfld) {
                                FieldDef targetField = instruction.Operand as FieldDef;

                                if(targetField == null)
                                    continue;
                                CilBody body = new CilBody();

                                body.Instructions.Add(OpCodes.Nop.ToInstruction());
                                body.Instructions.Add(OpCodes.Ldarg_0.ToInstruction());
                                body.Instructions.Add(OpCodes.Ldarg_1.ToInstruction());
                                body.Instructions.Add(OpCodes.Stfld.ToInstruction(targetField));
                                body.Instructions.Add(OpCodes.Ret.ToInstruction());

                                var sig = MethodSig.CreateInstance(module.CorLibTypes.Void, targetField.FieldSig.GetFieldType());
                                sig.HasThis = true;

                                MethodDefUser methodDefUser = new MethodDefUser(GenerateInvisibleName(), sig)
                                {
                                    Body = body,
                                    IsHideBySig = true
                                };

                                ProxyMethods.Add(methodDefUser);
                                method.DeclaringType.Methods.Add(methodDefUser);

                                instruction.Operand = methodDefUser;
                                instruction.OpCode = OpCodes.Call;
                            } else
                            if(instruction.OpCode == OpCodes.Call) {
                                if(instruction.Operand is MemberRef) {
                                    MemberRef methodReference = (MemberRef)instruction.Operand;

                                    if(!methodReference.FullName.Contains("Collections.Generic") && !methodReference.Name.Contains("ToString") && !methodReference.FullName.Contains("Thread::Start")) {
                                        MethodDef methodDef = rPHelper.GenerateMethod(type, methodReference, methodReference.HasThis);

                                        if(methodDef != null) {
                                            ProxyMethods.Add(methodDef);
                                            type.Methods.Add(methodDef);

                                            instruction.Operand = methodDef;
                                            methodDef.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool canObfuscate(MethodDef methodDef) {
            if(!methodDef.HasBody)
                return false;
            if(!methodDef.Body.HasInstructions)
                return false;

            if(methodDef.DeclaringType.IsGlobalModuleType)
                return false;

            return true;
        }
    }
}
