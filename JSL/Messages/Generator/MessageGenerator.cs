using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JSL.Messages.Generator
{
    public class MessageGenerator
    {
        public bool Generate(IEnumerable<MessageType> messageTypeProvider, string templatePath, string baseImplicitPath, string outputPath)
        {
            foreach (var file in Directory.EnumerateFiles(outputPath))
            {
                File.Delete(file);
            }
            var template = File.ReadAllText(templatePath);
            var messageTypes = new List<string>();
            var id = 1; // 0 is BaseMessage and unused
            foreach (var messageType in messageTypeProvider)
            {
                messageTypes.Add(messageType.Name);
                GenerateType(id++, messageType, template, outputPath);
            }
            GenerateBaseMessageImplicit(baseImplicitPath, messageTypes, outputPath);
            return true;
        }

        private const string BaseMessageToken = "[[MESSAGE_CONSTRUCTORS]]";
        private void GenerateBaseMessageImplicit(string baseImplicitPath, List<string> messageTypes, string outputPath)
        {
            var template = File.ReadAllText(baseImplicitPath);
            stringBuilder.Clear();
            foreach (var messageType in messageTypes)
            {
                stringBuilder.AppendLine($"{messageType}.ClassId => new {messageType}(),");
            }

            template = template.Replace(BaseMessageToken, stringBuilder.ToString());
            
            File.WriteAllText(outputPath + "BaseMessage" + ".cs", template);
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
                return;
            }
            var readMessageTemplate = ReadMessageTypeTemplate
                .Replace(MemberNameToken, memberName)
                .Replace(MemberTypeToken, typeName);
            stringBuilder.AppendLine(readMessageTemplate);
        }
        
        private const string WriterBaseTypeTemplate = "writer.Write(" + MemberNameToken + ");";
        private const string WriterMessageTypeTemplate = MemberNameToken + ".Serialize(writer);"; 
        private void WriteWriterFunc(string typeName, string memberName)
        {
            if (_typeWriteronversion.ContainsKey(typeName))
            {
                var writeBaseTemplate = _typeWriteronversion[typeName].Replace(MemberNameToken, memberName);
                stringBuilder.AppendLine(writeBaseTemplate);
                return;
            }
            var writeMessageTemplate = WriterMessageTypeTemplate
                .Replace(MemberNameToken, memberName)
                .Replace(MemberTypeToken, typeName);
            stringBuilder.AppendLine(writeMessageTemplate);
        }

        private const string WriteAcquireTemplate = MemberNameToken + " = MemoryManager.SerializablePool.Get<" + MemberTypeToken + ">();";
        private void WriteAcquireFunc(string typeName, string memberName)
        {
            if (_typeWriteronversion.ContainsKey(typeName))
            {
                return;
            }
            var writeAcquireTemplate = WriteAcquireTemplate
                .Replace(MemberNameToken, memberName)
                .Replace(MemberTypeToken, typeName);
            stringBuilder.AppendLine(writeAcquireTemplate);
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
        private const string ClassNameToken = "[[CLASS_NAME]]";
        private const string MembersToken = "[[MEMBERS]]";
        private const string MembersReadToken = "[[READ_MEMBERS]]";
        private const string MembersWriteToken = "[[WRITE_MEMBERS]]";
        private const string MembersAcquireToken = "[[ACQUIRE_MEMBERS]]";
        private const string MembersReleaseToken = "[[RELEASE_MEMBERS]]";
        private const string TypeIdToken = "[[TYPE_ID]]";
        private const int StringBufferSize = 2048;
        StringBuilder stringBuilder = new StringBuilder(StringBufferSize);
        private bool GenerateType(int id, MessageType messageType, string messageTemplate, string outputPath)
        {
            messageTemplate = messageTemplate
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
                var memberDeclaration = MemberDefinitionTemplate
                    .Replace(MemberTypeToken, messageData.Type)
                    .Replace(MemberNameToken, messageData.Name);
                stringBuilder.AppendLine(memberDeclaration);
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