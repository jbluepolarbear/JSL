using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JSL.CodeGen.Messages.Generator;
using JSL.CodeGen.Messages.Templates;
using JSL.NetTypes;
using JSL.Utility;

namespace JSL.Messages.Generator
{
    public class MessageGenerator
    {
        public bool Generate(MessageGeneratorConfig config)
        {
            foreach (var file in Directory.EnumerateFiles(config.OutputPath))
            {
                File.Delete(file);
            }
            var template = MessageTemplate.Template;
            var messageTypes = new List<string>();
            var id = 1; // 0 is BaseMessage and unused
            foreach (var messageType in config.MessageTypes)
            {
                messageTypes.Add(messageType.Name);
                GenerateType(id++, messageType, config.OutputNamespace, template, config.OutputPath);
            }
            GenerateBaseMessageImplicit(MessageFactoryTemplate.Tempalte, config.OutputNamespace, messageTypes, config.OutputPath);
            return true;
        }

        private List<Type> _recycleableTypes;
        public bool TypeIsRecycleable(string typeName, out string fullTypeName)
        {
            if (_recycleableTypes == null)
            {
                _recycleableTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(_ => _.GetTypes())
                    .Where(_ => _.BaseType != null && typeof(INetRecyclable).IsAssignableFrom(_.BaseType)).ToList();
            }

            if (typeName.Contains("NetList"))
            {
                var innerTypeName = typeName.Substring(8, typeName.Length - 9);
                var foundType = _recycleableTypes.FirstOrDefault(_ => _.Name.Contains(innerTypeName));
                fullTypeName = foundType != null ? "JSL.NetTypes.NetList<" + foundType.FullName + ">" : null;
            }
            else
            {
                var foundType = _recycleableTypes.FirstOrDefault(_ => _.Name.Contains(typeName));
                fullTypeName = foundType?.FullName;
            }
            return fullTypeName != null;
        }

        private const string BaseMessageToken = "[[MESSAGE_CONSTRUCTORS]]";
        private void GenerateBaseMessageImplicit(string template, string namespaceName, List<string> messageTypes, string outputPath)
        {
            stringBuilder.Clear();
            foreach (var messageType in messageTypes)
            {
                stringBuilder.AppendLine($"{messageType}.ClassId => new {messageType}(),");
            }

            template = template.Replace(NamespaceToken, namespaceName);
            template = template.Replace(BaseMessageToken, stringBuilder.ToString());
            
            File.WriteAllText(outputPath + "MessageFactory" + ".cs", template);
        }

        private Dictionary<string, string> _typeReaderonversion = new Dictionary<string, string>
        {
            {"byte", "reader.ReadByte()"},
            {"short", "reader.ReadInt16()"},
            {"int", "reader.ReadInt32()"},
            {"long", "reader.ReadInt64()"},
            {"ushort", "reader.ReadUInt16()"},
            {"uint", "reader.ReadUInt32()"},
            {"ulong", "reader.ReadUInt64()"},
            {"float", "reader.ReadSingle()"},
            {"double", "reader.ReadDouble()"},
        };
        
        private Dictionary<string, string> _typeWriteronversion = new Dictionary<string, string>
        {
            {"byte", $"writer.Write((byte) {MemberNameToken});"},
            {"short", $"writer.Write((short) {MemberNameToken});"},
            {"int", $"writer.Write((int) {MemberNameToken});"},
            {"long", $"writer.Write((long) {MemberNameToken});"},
            {"ushort", $"writer.Write((ushort) {MemberNameToken});"},
            {"uint", $"writer.Write((uint) {MemberNameToken});"},
            {"ulong", $"writer.Write((ulong) {MemberNameToken});"},
            {"float", $"writer.Write((float) {MemberNameToken});"},
            {"double", $"writer.Write((double) {MemberNameToken});"}
        };

        private const string ReaderToken = "[[READER]]";
        private const string ReadBaseTypeTemplate = MemberNameToken + " = " + ReaderToken + ";";
        private const string ReadMessageTypeTemplate = MemberNameToken + ".Deserialize(reader);";
        private void WriteReaderFunc(string typeName, string memberName)
        {
            if (_typeReaderonversion.ContainsKey(typeName))
            {
                var readBaseTemplate = ReadBaseTypeTemplate
                    .Replace(MemberNameToken, memberName)
                    .Replace(ReaderToken, _typeReaderonversion[typeName]);
                stringBuilder.AppendLine(readBaseTemplate);
            }
            else if (TypeIsRecycleable(typeName, out var fullTypeName))
            {
                var readMessageTemplate = ReadMessageTypeTemplate
                    .Replace(MemberNameToken, memberName)
                    .Replace(MemberTypeToken, fullTypeName);
                stringBuilder.AppendLine(readMessageTemplate);
            }
            else
            {
                throw new ArgumentException($"Type: {typeName} not found.");
            }
        }
        
        private const string WriterBaseTypeTemplate = "writer.Write(" + MemberNameToken + ");";
        private const string WriterMessageTypeTemplate = MemberNameToken + ".Serialize(writer);"; 
        private void WriteWriterFunc(string typeName, string memberName)
        {
            if (_typeWriteronversion.ContainsKey(typeName))
            {
                var writeBaseTemplate = _typeWriteronversion[typeName].Replace(MemberNameToken, memberName);
                stringBuilder.AppendLine(writeBaseTemplate);
            }
            else if (TypeIsRecycleable(typeName, out var fullTypeName))
            {
                var writeMessageTemplate = WriterMessageTypeTemplate
                    .Replace(MemberNameToken, memberName)
                    .Replace(MemberTypeToken, fullTypeName);
                stringBuilder.AppendLine(writeMessageTemplate);
            }
            else
            {
                throw new ArgumentException($"Type: {typeName} not found.");
            }
        }

        private const string WriteAcquireTemplate = MemberNameToken + " = MemoryManager.RecyclablePool.Get<" + MemberTypeToken + ">();";
        private void WriteAcquireFunc(string typeName, string memberName)
        {
            if (_typeWriteronversion.ContainsKey(typeName))
            {
                return;
            }

            if (TypeIsRecycleable(typeName, out var fullTypeName))
            {
                var writeAcquireTemplate = WriteAcquireTemplate
                    .Replace(MemberNameToken, memberName)
                    .Replace(MemberTypeToken, fullTypeName);
                stringBuilder.AppendLine(writeAcquireTemplate);
            }
            else
            {
                throw new ArgumentException($"Type: {typeName} not found.");
            }
        }

        private const string WriteReleaseTemplate = MemberNameToken + ".Dispose();";
        private const string WriteReleaseClearTemplate = MemberNameToken + " = null;";
        private void WriteReleaseFunc(string typeName, string memberName)
        {
            if (_typeWriteronversion.ContainsKey(typeName))
            {
                return;
            }

            var writeReleaseTemplate = WriteReleaseTemplate
                .Replace(MemberNameToken, memberName);
            stringBuilder.AppendLine(writeReleaseTemplate);
            var writeReleaseClearTemplate = WriteReleaseClearTemplate
                .Replace(MemberNameToken, memberName);
            stringBuilder.AppendLine(writeReleaseClearTemplate);
        }
        
        private const string BaseClassToken = "[[BASE_CLASS]]";
        private const string MemberTypeToken = "[[MEMBER_TYPE]]";
        private const string MemberNameToken = "[[MEMBER_NAME]]";
        private const string MemberDefinitionTemplate = "public " + MemberTypeToken + " " + MemberNameToken + ";";
        private const string NamespaceToken = "[[NAMESPACE]]";
        private const string ClassNameToken = "[[CLASS_NAME]]";
        private const string MembersToken = "[[MEMBERS]]";
        private const string MembersReadToken = "[[READ_MEMBERS]]";
        private const string MembersWriteToken = "[[WRITE_MEMBERS]]";
        private const string MembersAcquireToken = "[[ACQUIRE_MEMBERS]]";
        private const string MembersReleaseToken = "[[RELEASE_MEMBERS]]";
        private const string TypeIdToken = "[[TYPE_ID]]";
        private const int StringBufferSize = 2048;
        StringBuilder stringBuilder = new StringBuilder(StringBufferSize);
        private bool GenerateType(int id, MessageType messageType, string namespaceName, string messageTemplate, string outputPath)
        {
            messageTemplate = messageTemplate
                .Replace(NamespaceToken, namespaceName)
                .Replace(ClassNameToken, messageType.Name)
                .Replace(TypeIdToken, id.ToString());

            if (string.IsNullOrEmpty(messageType.Base))
            {
                messageTemplate = messageTemplate.Replace(BaseClassToken, "BaseMessage");
            }
            else
            {
                messageTemplate = messageTemplate.Replace(BaseClassToken, messageType.Base);
            }
            stringBuilder.Clear();
            // Add all the members to the class
            foreach (var messageData in messageType.Data)
            {

                if (TypeIsRecycleable(messageData.Type, out var fullTypeName))
                {
                    var memberDeclaration = MemberDefinitionTemplate
                        .Replace(MemberTypeToken, fullTypeName)
                        .Replace(MemberNameToken, messageData.Name);
                    stringBuilder.AppendLine(memberDeclaration);
                }
                else if (_typeWriteronversion.ContainsKey(messageData.Type))
                {
                    var memberDeclaration = MemberDefinitionTemplate
                        .Replace(MemberTypeToken, messageData.Type)
                        .Replace(MemberNameToken, messageData.Name);
                    stringBuilder.AppendLine(memberDeclaration);
                }
                else
                {
                    throw new ArgumentException($"Type: {messageData.Type} not found.");
                }
            }

            messageTemplate = messageTemplate.Replace(MembersToken, stringBuilder.ToString());
            stringBuilder.Clear();
            
            // Add all the readers
            foreach (var messageData in messageType.Data)
            {
                WriteReaderFunc(messageData.Type, messageData.Name);
            }

            messageTemplate = messageTemplate.Replace(MembersReadToken, stringBuilder.ToString());
            stringBuilder.Clear();
            
            // Add all the writers
            foreach (var messageData in messageType.Data)
            {
                WriteWriterFunc(messageData.Type, messageData.Name);
            }

            messageTemplate = messageTemplate.Replace(MembersWriteToken, stringBuilder.ToString());
            stringBuilder.Clear();
            
            // Add all acquire
            foreach (var messageData in messageType.Data)
            {
                WriteAcquireFunc(messageData.Type, messageData.Name);
            }

            messageTemplate = messageTemplate.Replace(MembersAcquireToken, stringBuilder.ToString());
            stringBuilder.Clear();
            
            // Add all release 
            foreach (var messageData in messageType.Data)
            {
                WriteReleaseFunc(messageData.Type, messageData.Name);
            }

            messageTemplate = messageTemplate.Replace(MembersReleaseToken, stringBuilder.ToString());
            
            File.WriteAllText(outputPath + messageType.Name + ".cs", messageTemplate);

            return true;
        }
        
    }
}