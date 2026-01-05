using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Protector.Protections {
    internal class ImportProtection {
        public static FieldDefUser CreateField(FieldSig sig) {
            return new FieldDefUser(GenerateInvisibleName(), sig, FieldAttributes.Public | FieldAttributes.Static);
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
            var bridges = new Dictionary<IMethod, MethodDef>();
            var methods = new Dictionary<IMethod, TypeDef>();
            var field = CreateField(new FieldSig(module.ImportAsTypeSig(typeof(object[]))));

            var cctor = module.GlobalType.FindStaticConstructor();
            if(cctor == null) {
                cctor = new MethodDefUser(".cctor",
                    MethodSig.CreateStatic(module.CorLibTypes.Void),
                    MethodImplAttributes.IL | MethodImplAttributes.Managed,
                    MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
                cctor.Body = new CilBody();
                cctor.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
                module.GlobalType.Methods.Add(cctor);
            }

            var randomField = CreateField(new FieldSig(module.CorLibTypes.Int32));
            var initFlagField = CreateField(new FieldSig(module.CorLibTypes.Boolean));
            var counterField = CreateField(new FieldSig(module.CorLibTypes.Int32));
            module.GlobalType.Fields.Add(randomField);
            module.GlobalType.Fields.Add(initFlagField);
            module.GlobalType.Fields.Add(counterField);

            foreach(TypeDef type in module.GetTypes().ToArray()) {
                if(type.IsDelegate)
                    continue;
                if(type.IsGlobalModuleType)
                    continue;
                if(type.Namespace == "Costura")
                    continue;

                foreach(MethodDef method in type.Methods.ToArray()) {
                    if(!method.HasBody)
                        continue;
                    if(!method.Body.HasInstructions)
                        continue;
                    if(method.IsConstructor && method.IsStaticConstructor)
                        continue;

                    ProcessMethodBody(module, method, bridges, methods, field, randomField, initFlagField, counterField);
                }
            }

            module.GlobalType.Fields.Add(field);

            if(methods.Count > 0) {
                InitializeCctor(cctor, field, randomField, initFlagField, counterField, methods);
            }
        }

        private static void ProcessMethodBody(ModuleDef module, MethodDef method,
            Dictionary<IMethod, MethodDef> bridges, Dictionary<IMethod, TypeDef> methods,
            FieldDef field, FieldDef randomField, FieldDef initFlagField, FieldDef counterField) {

            var instrs = method.Body.Instructions;
            var newInstructions = new List<Instruction>();
            var random = new Random();

            for(int i = 0; i < instrs.Count; i++) {
                var instruction = instrs[i];

                if(IsMethodCallInstruction(instruction.OpCode)) {
                    if(instruction.Operand is IMethod idef) {
                        var resolvedMethod = idef.ResolveMethodDef();

                        if(ShouldSkipMethod(idef, resolvedMethod)) {
                            newInstructions.Add(instruction);
                            continue;
                        }

                        if(resolvedMethod != null && resolvedMethod.HasThis && !resolvedMethod.IsStatic && !resolvedMethod.IsConstructor) {
                            ProcessInstanceMethod(module, instruction, idef, bridges, methods, field, newInstructions, random, counterField);
                            continue;
                        }

                        if(bridges.ContainsKey(idef)) {
                            instruction.OpCode = OpCodes.Call;
                            instruction.Operand = bridges[idef];
                            AddObfuscationInstructions(newInstructions, randomField, counterField, random);
                            newInstructions.Add(instruction);
                        } else {
                            var bridge = CreateMethodBridge(module, idef, bridges, methods, field);
                            if(bridge != null) {
                                instruction.OpCode = OpCodes.Call;
                                instruction.Operand = bridge;
                                AddObfuscationInstructions(newInstructions, randomField, counterField, random);
                                newInstructions.Add(instruction);
                            } else {
                                newInstructions.Add(instruction);
                            }
                        }
                    } else if(instruction.Operand is MemberRef memberRef && memberRef.IsMethodRef) {
                        ProcessMemberRef(module, instruction, memberRef, bridges, methods, field, newInstructions, random, counterField);
                    } else {
                        newInstructions.Add(instruction);
                    }
                } else if(instruction.OpCode == OpCodes.Ldftn || instruction.OpCode == OpCodes.Ldvirtftn) {
                    if(instruction.Operand is IMethod idef) {
                        ProcessFunctionPointer(module, instruction, idef, bridges, methods, field, newInstructions);
                    } else {
                        newInstructions.Add(instruction);
                    }
                } else if(instruction.OpCode == OpCodes.Newobj) {
                    if(instruction.Operand is IMethod idef) {
                        ProcessConstructor(module, instruction, idef, bridges, methods, field, newInstructions, random, counterField);
                    } else {
                        newInstructions.Add(instruction);
                    }
                } else {
                    newInstructions.Add(instruction);
                }
            }

            if(newInstructions.Count > 0) {
                method.Body.Instructions.Clear();
                foreach(var instr in newInstructions) {
                    method.Body.Instructions.Add(instr);
                }
                method.Body.OptimizeMacros();
            }
        }

        private static bool IsMethodCallInstruction(OpCode opCode) {
            return opCode == OpCodes.Call || opCode == OpCodes.Callvirt ||
                   opCode == OpCodes.Calli || opCode == OpCodes.Jmp;
        }

        private static bool ShouldSkipMethod(IMethod method, MethodDef resolved) {
            if(resolved == null)
                return true;

            if(resolved.DeclaringType != null) {
                var declaringType = resolved.DeclaringType.FullName;
                if(declaringType.StartsWith("System.") ||
                   declaringType.StartsWith("Microsoft.") ||
                   declaringType.StartsWith("Windows.") ||
                   declaringType.StartsWith("mscorlib"))
                    return true;

                if(resolved.IsSpecialName && (resolved.Name.StartsWith("get_") || resolved.Name.StartsWith("set_")))
                    return true;
            }

            if(resolved.HasGenericParameters)
                return true;

            if(resolved.IsPinvokeImpl)
                return true;

            if(resolved.IsAbstract)
                return true;

            if(resolved.IsVirtual && !resolved.IsFinal)
                return true;

            return false;
        }

        private static void ProcessInstanceMethod(ModuleDef module, Instruction instruction, IMethod idef,
            Dictionary<IMethod, MethodDef> bridges, Dictionary<IMethod, TypeDef> methods,
            FieldDef field, List<Instruction> newInstructions, Random random, FieldDef counterField) {

            if(bridges.ContainsKey(idef)) {
                instruction.OpCode = OpCodes.Call;
                instruction.Operand = bridges[idef];
                AddObfuscationInstructions(newInstructions, null, counterField, random);
                newInstructions.Add(instruction);
            } else {
                var bridge = CreateInstanceMethodBridge(module, idef, bridges, methods, field);
                if(bridge != null) {
                    instruction.OpCode = OpCodes.Call;
                    instruction.Operand = bridge;
                    AddObfuscationInstructions(newInstructions, null, counterField, random);
                    newInstructions.Add(instruction);
                } else {
                    newInstructions.Add(instruction);
                }
            }
        }

        private static void ProcessMemberRef(ModuleDef module, Instruction instruction, MemberRef memberRef,
            Dictionary<IMethod, MethodDef> bridges, Dictionary<IMethod, TypeDef> methods,
            FieldDef field, List<Instruction> newInstructions, Random random, FieldDef counterField) {

            try {
                var resolved = memberRef.ResolveMethod();
                if(resolved != null && !ShouldSkipMethod(resolved, resolved.ResolveMethodDef())) {
                    if(bridges.ContainsKey(resolved)) {
                        instruction.OpCode = OpCodes.Call;
                        instruction.Operand = bridges[resolved];
                        AddObfuscationInstructions(newInstructions, null, counterField, random);
                        newInstructions.Add(instruction);
                    } else {
                        var bridge = CreateMethodBridge(module, resolved, bridges, methods, field);
                        if(bridge != null) {
                            instruction.OpCode = OpCodes.Call;
                            instruction.Operand = bridge;
                            AddObfuscationInstructions(newInstructions, null, counterField, random);
                            newInstructions.Add(instruction);
                        } else {
                            newInstructions.Add(instruction);
                        }
                    }
                } else {
                    newInstructions.Add(instruction);
                }
            } catch {
                newInstructions.Add(instruction);
            }
        }

        private static void ProcessFunctionPointer(ModuleDef module, Instruction instruction, IMethod idef,
            Dictionary<IMethod, MethodDef> bridges, Dictionary<IMethod, TypeDef> methods,
            FieldDef field, List<Instruction> newInstructions) {

            if(!bridges.ContainsKey(idef)) {
                CreateMethodBridge(module, idef, bridges, methods, field);
            }

            if(bridges.ContainsKey(idef)) {
                newInstructions.Add(OpCodes.Ldsfld.ToInstruction(field));
                newInstructions.Add(Instruction.CreateLdcI4(GetMethodIndex(methods, idef)));
                newInstructions.Add(OpCodes.Ldelem_Ref.ToInstruction());
                newInstructions.Add(OpCodes.Ldftn.ToInstruction(bridges[idef]));
            } else {
                newInstructions.Add(instruction);
            }
        }

        private static void ProcessConstructor(ModuleDef module, Instruction instruction, IMethod idef,
            Dictionary<IMethod, MethodDef> bridges, Dictionary<IMethod, TypeDef> methods,
            FieldDef field, List<Instruction> newInstructions, Random random, FieldDef counterField) {

            var resolved = idef.ResolveMethodDef();
            if(resolved != null && ShouldSkipMethod(idef, resolved)) {
                newInstructions.Add(instruction);
                return;
            }

            if(!bridges.ContainsKey(idef)) {
                CreateMethodBridge(module, idef, bridges, methods, field);
            }

            if(bridges.ContainsKey(idef)) {
                instruction.OpCode = OpCodes.Call;
                instruction.Operand = bridges[idef];
                AddObfuscationInstructions(newInstructions, null, counterField, random);
                newInstructions.Add(instruction);
            } else {
                newInstructions.Add(instruction);
            }
        }

        private static MethodDef CreateMethodBridge(ModuleDef module, IMethod idef,
            Dictionary<IMethod, MethodDef> bridges, Dictionary<IMethod, TypeDef> methods,
            FieldDef field) {

            var def = idef.ResolveMethodDef();
            if(def == null)
                return null;

            var sig = CreateProxySignature(module, def);
            var delegateType = CreateDelegateType(module, sig);
            module.Types.Add(delegateType);

            var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
            var bridge = new MethodDefUser(GenerateInvisibleName(), sig, methImplFlags, methFlags);
            bridge.Body = new CilBody();

            bridge.Body.Instructions.Add(OpCodes.Ldsfld.ToInstruction(field));
            bridge.Body.Instructions.Add(Instruction.CreateLdcI4(methods.Count));
            bridge.Body.Instructions.Add(OpCodes.Ldelem_Ref.ToInstruction());
            bridge.Body.Instructions.Add(OpCodes.Castclass.ToInstruction(delegateType));

            for(int paramIndex = 0; paramIndex < bridge.Parameters.Count; paramIndex++) {
                bridge.Body.Instructions.Add(OpCodes.Ldarg.ToInstruction(bridge.Parameters[paramIndex]));
            }

            bridge.Body.Instructions.Add(OpCodes.Callvirt.ToInstruction(delegateType.FindMethod("Invoke")));
            bridge.Body.Instructions.Add(OpCodes.Ret.ToInstruction());

            delegateType.Methods.Add(bridge);
            methods.Add(idef, delegateType);
            bridges.Add(idef, bridge);

            return bridge;
        }

        private static MethodDef CreateInstanceMethodBridge(ModuleDef module, IMethod idef,
            Dictionary<IMethod, MethodDef> bridges, Dictionary<IMethod, TypeDef> methods,
            FieldDef field) {

            var def = idef.ResolveMethodDef();
            if(def == null)
                return null;

            var paramTypes = new List<TypeSig>();
            paramTypes.Add(module.CorLibTypes.Object);

            foreach(var param in def.MethodSig.Params) {
                paramTypes.Add(param.IsClassSig ? module.CorLibTypes.Object : param);
            }

            TypeSig retType = def.MethodSig.RetType;
            if(retType != null && retType.IsClassSig)
                retType = module.CorLibTypes.Object;

            var sig = MethodSig.CreateStatic(retType ?? module.CorLibTypes.Void, paramTypes.ToArray());
            var delegateType = CreateDelegateType(module, sig);
            module.Types.Add(delegateType);

            var bridge = new MethodDefUser(GenerateInvisibleName(), sig,
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot);

            bridge.Body = new CilBody();

            bridge.Body.Instructions.Add(OpCodes.Ldsfld.ToInstruction(field));
            bridge.Body.Instructions.Add(Instruction.CreateLdcI4(methods.Count));
            bridge.Body.Instructions.Add(OpCodes.Ldelem_Ref.ToInstruction());
            bridge.Body.Instructions.Add(OpCodes.Castclass.ToInstruction(delegateType));

            for(int paramIndex = 0; paramIndex < bridge.Parameters.Count; paramIndex++) {
                bridge.Body.Instructions.Add(OpCodes.Ldarg.ToInstruction(bridge.Parameters[paramIndex]));
            }

            bridge.Body.Instructions.Add(OpCodes.Callvirt.ToInstruction(delegateType.FindMethod("Invoke")));
            bridge.Body.Instructions.Add(OpCodes.Ret.ToInstruction());

            delegateType.Methods.Add(bridge);
            methods.Add(idef, delegateType);
            bridges.Add(idef, bridge);

            return bridge;
        }

        private static void AddObfuscationInstructions(List<Instruction> instructions, FieldDef randomField, FieldDef counterField, Random random) {
            int obfuscationType = random.Next(0, 100);

            if(obfuscationType < 40) {
                instructions.Add(OpCodes.Ldsfld.ToInstruction(counterField));
                instructions.Add(Instruction.CreateLdcI4(1));
                instructions.Add(OpCodes.Add.ToInstruction());
                instructions.Add(OpCodes.Stsfld.ToInstruction(counterField));
                instructions.Add(OpCodes.Ldsfld.ToInstruction(counterField));
                instructions.Add(OpCodes.Pop.ToInstruction());
            } else if(obfuscationType < 70 && randomField != null) {
                instructions.Add(OpCodes.Ldsfld.ToInstruction(randomField));
                instructions.Add(Instruction.CreateLdcI4(random.Next(1, 100)));
                instructions.Add(OpCodes.Xor.ToInstruction());
                instructions.Add(OpCodes.Pop.ToInstruction());
            } else if(obfuscationType < 85) {
                int val1 = random.Next(10, 1000);
                int val2 = random.Next(10, 1000);
                instructions.Add(Instruction.CreateLdcI4(val1));
                instructions.Add(Instruction.CreateLdcI4(val2));
                instructions.Add(OpCodes.Mul.ToInstruction());
                instructions.Add(Instruction.CreateLdcI4(val1));
                instructions.Add(OpCodes.Div.ToInstruction());
                instructions.Add(OpCodes.Pop.ToInstruction());
            }
        }

        private static int GetMethodIndex(Dictionary<IMethod, TypeDef> methods, IMethod method) {
            int index = 0;
            foreach(var key in methods.Keys) {
                if(key == method)
                    return index;
                index++;
            }
            return -1;
        }

        private static void InitializeCctor(MethodDef cctor, FieldDef field, FieldDef randomField, FieldDef initFlagField, FieldDef counterField, Dictionary<IMethod, TypeDef> methods) {
            var instructions = new List<Instruction>();
            var currentInstructions = cctor.Body.Instructions.ToList();
            var random = new Random();
            cctor.Body.Instructions.Clear();

            instructions.Add(Instruction.CreateLdcI4(random.Next()));
            instructions.Add(OpCodes.Stsfld.ToInstruction(randomField));

            instructions.Add(Instruction.CreateLdcI4(0));
            instructions.Add(OpCodes.Stsfld.ToInstruction(counterField));

            instructions.Add(OpCodes.Ldsfld.ToInstruction(initFlagField));
            var skipInitLabel = new Instruction(OpCodes.Nop);
            instructions.Add(OpCodes.Brtrue_S.ToInstruction(skipInitLabel));

            instructions.Add(Instruction.CreateLdcI4(methods.Count));
            instructions.Add(OpCodes.Newarr.ToInstruction(cctor.Module.CorLibTypes.Object.ToTypeDefOrRef()));
            instructions.Add(OpCodes.Stsfld.ToInstruction(field));

            var methodList = methods.ToList();
            for(int i = 0; i < methodList.Count; i++) {
                var entry = methodList[i];

                instructions.Add(OpCodes.Ldsfld.ToInstruction(field));
                instructions.Add(Instruction.CreateLdcI4(i));

                if(entry.Key.IsMethodDef && entry.Key.ResolveMethodDef()?.IsStatic == true) {
                    instructions.Add(OpCodes.Ldnull.ToInstruction());
                } else {
                    instructions.Add(OpCodes.Ldnull.ToInstruction());
                }

                instructions.Add(OpCodes.Ldftn.ToInstruction(entry.Key));
                instructions.Add(OpCodes.Newobj.ToInstruction(entry.Value.FindMethod(".ctor")));
                instructions.Add(OpCodes.Stelem_Ref.ToInstruction());

                if(random.Next(0, 100) < 30) {
                    instructions.Add(Instruction.CreateLdcI4(random.Next(1, 100)));
                    instructions.Add(Instruction.CreateLdcI4(random.Next(1, 100)));
                    instructions.Add(OpCodes.Add.ToInstruction());
                    instructions.Add(OpCodes.Pop.ToInstruction());
                }
            }

            instructions.Add(OpCodes.Ldc_I4_1.ToInstruction());
            instructions.Add(OpCodes.Stsfld.ToInstruction(initFlagField));

            instructions.Add(skipInitLabel);

            foreach(var instr in instructions)
                cctor.Body.Instructions.Add(instr);
            foreach(var instr in currentInstructions)
                cctor.Body.Instructions.Add(instr);
        }

        public static TypeDef CreateDelegateType(ModuleDef module, MethodSig sig) {
            var ret = new TypeDefUser(GenerateInvisibleName(), module.CorLibTypes.GetTypeRef("System", "MulticastDelegate"));
            ret.Attributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoLayout | TypeAttributes.AnsiClass;
            ret.BaseType = module.CorLibTypes.GetTypeRef("System", "MulticastDelegate");

            var ctorSig = MethodSig.CreateInstance(module.CorLibTypes.Void, module.CorLibTypes.Object, module.CorLibTypes.IntPtr);
            var ctor = new MethodDefUser(".ctor", ctorSig,
                MethodImplAttributes.Runtime,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            ret.Methods.Add(ctor);

            var invoke = new MethodDefUser("Invoke", sig.Clone());
            invoke.MethodSig.HasThis = true;
            invoke.Attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;
            invoke.ImplAttributes = MethodImplAttributes.Runtime;
            ret.Methods.Add(invoke);

            var beginInvokeSig = MethodSig.CreateInstance(module.CorLibTypes.Void, module.CorLibTypes.Object.ToValueTypeSig(), module.CorLibTypes.IntPtr.ToValueTypeSig());
            var beginInvoke = new MethodDefUser("BeginInvoke", beginInvokeSig,
                MethodImplAttributes.Runtime,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot);
            ret.Methods.Add(beginInvoke);

            var endInvokeSig = MethodSig.CreateInstance(module.CorLibTypes.Void, module.CorLibTypes.Object.ToValueTypeSig());
            var endInvoke = new MethodDefUser("EndInvoke", endInvokeSig,
                MethodImplAttributes.Runtime,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot);
            ret.Methods.Add(endInvoke);

            return ret;
        }

        public static MethodSig CreateProxySignature(ModuleDef module, IMethod method) {
            var paramTypes = new List<TypeSig>();

            if(method.MethodSig.HasThis && !method.MethodSig.ExplicitThis) {
                paramTypes.Add(module.CorLibTypes.Object);
            }

            foreach(var param in method.MethodSig.Params) {
                if(param.IsClassSig)
                    paramTypes.Add(module.CorLibTypes.Object);
                else
                    paramTypes.Add(param);
            }

            TypeSig retType = method.MethodSig.RetType;
            if(retType != null && retType.IsClassSig)
                retType = module.CorLibTypes.Object;

            return MethodSig.CreateStatic(retType ?? module.CorLibTypes.Void, paramTypes.ToArray());
        }
    }
}