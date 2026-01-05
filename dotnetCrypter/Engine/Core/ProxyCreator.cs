using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace dotnetCrypter.Engine.Core {
    internal class ProxyCreator
    {
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
        public MethodDef GenerateMethod(TypeDef declaringType, object targetMethod, bool hasThis = false)
        {
            MemberRef methodReference = (MemberRef)targetMethod;
            MethodDef methodDefinition = new MethodDefUser(GenerateInvisibleName(), MethodSig.CreateStatic((methodReference).ReturnType), MethodAttributes.FamANDAssem | MethodAttributes.Public | MethodAttributes.Static);
            methodDefinition.Body = new CilBody(); 
            if (hasThis)
                methodDefinition.MethodSig.Params.Add(declaringType.Module.Import(declaringType.ToTypeSig())); 
            foreach (TypeSig current in methodReference.MethodSig.Params)
                methodDefinition.MethodSig.Params.Add(current); 
            methodDefinition.Parameters.UpdateParameterTypes(); 
            foreach (var current in methodDefinition.Parameters)
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, current)); 
            methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Call, methodReference)); 
            return methodDefinition;
        }
    }
}
