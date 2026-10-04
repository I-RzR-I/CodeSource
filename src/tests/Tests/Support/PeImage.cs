using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using RzR.Core.CodeSource;

namespace Tests.Support
{
    internal static class PeImage
    {
        private static readonly string CodeSourceTypeName = typeof(CodeSourceAttribute).FullName;

        internal static byte[] Read(Assembly assembly)
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
                throw new InvalidOperationException($"'{assembly.FullName}' was not loaded from a file, so it has no image to read.");

            return File.ReadAllBytes(assembly.Location);
        }

        internal static List<string> AssemblyReferenceNames(byte[] image)
            => WithMetadata(image, metadata => metadata.AssemblyReferences
                .Select(handle => metadata.GetString(metadata.GetAssemblyReference(handle).Name))
                .ToList());

        internal static List<string> CodeSourceUrls(byte[] image)
            => WithMetadata(image, metadata => Decode(metadata, metadata.CustomAttributes, CodeSourceTypeName)
                .Select(SourceUrl)
                .ToList());

        internal static List<CustomAttributeValue<string>> CustomAttributeValues(byte[] image, Type declaringType, Type attributeType)
            => WithMetadata(image, metadata => Decode(metadata, FindType(metadata, declaringType).GetCustomAttributes(), attributeType.FullName)
                .ToList());

        internal static TypeAttributes TypeFlags(byte[] image, Type declaringType)
            => WithMetadata(image, metadata => FindType(metadata, declaringType).Attributes);

        internal static (FieldAttributes Attributes, object Value) FieldConstant(byte[] image, Type declaringType, string fieldName)
            => WithMetadata(image, metadata =>
            {
                var field = Single(
                    FindType(metadata, declaringType).GetFields()
                        .Select(metadata.GetFieldDefinition)
                        .Where(definition => metadata.GetString(definition.Name) == fieldName),
                    $"field '{fieldName}' on '{declaringType.FullName}'");
                var constantHandle = field.GetDefaultValue();
                if (constantHandle.IsNil)
                    return (field.Attributes, (object)null);

                var constant = metadata.GetConstant(constantHandle);

                return (field.Attributes, metadata.GetBlobReader(constant.Value).ReadConstant(constant.TypeCode));
            });

        private static T WithMetadata<T>(byte[] image, Func<MetadataReader, T> read)
        {
            using var peReader = new PEReader(new MemoryStream(image, false));

            return read(peReader.GetMetadataReader());
        }

        private static IEnumerable<CustomAttributeValue<string>> Decode(MetadataReader metadata, IEnumerable<CustomAttributeHandle> handles, string attributeTypeName)
            => handles
                .Select(metadata.GetCustomAttribute)
                .Where(attribute => AttributeTypeName(metadata, attribute) == attributeTypeName)
                .Select(attribute => attribute.DecodeValue(TypeNameProvider.Instance));

        private static TypeDefinition FindType(MetadataReader metadata, Type declaringType)
            => metadata.GetTypeDefinition(Single(
                metadata.TypeDefinitions.Where(handle => FullName(metadata, handle) == declaringType.FullName),
                $"TypeDef '{declaringType.FullName}'"));

        private static T Single<T>(IEnumerable<T> candidates, string description)
        {
            var matches = candidates.ToList();
            if (matches.Count != 1)
                throw new InvalidOperationException($"Expected one {description} in the image, found {matches.Count}.");

            return matches[0];
        }

        private static string SourceUrl(CustomAttributeValue<string> value)
            => value.FixedArguments.Length > 0
                ? (string)value.FixedArguments[0].Value
                : value.NamedArguments
                    .Where(argument => argument.Name == nameof(CodeSourceAttribute.SourceUrl))
                    .Select(argument => (string)argument.Value)
                    .FirstOrDefault();

        private static string AttributeTypeName(MetadataReader metadata, CustomAttribute attribute)
        {
            var owner = attribute.Constructor.Kind switch
            {
                HandleKind.MethodDefinition => (EntityHandle)metadata.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).GetDeclaringType(),
                HandleKind.MemberReference => metadata.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent,
                _ => default
            };

            return owner.Kind switch
            {
                HandleKind.TypeDefinition => FullName(metadata, (TypeDefinitionHandle)owner),
                HandleKind.TypeReference => FullName(metadata, (TypeReferenceHandle)owner),
                _ => null
            };
        }

        private static string FullName(MetadataReader metadata, TypeDefinitionHandle handle)
        {
            var definition = metadata.GetTypeDefinition(handle);
            var declaringType = definition.GetDeclaringType();

            return declaringType.IsNil
                ? Qualify(metadata.GetString(definition.Namespace), metadata.GetString(definition.Name))
                : FullName(metadata, declaringType) + "+" + metadata.GetString(definition.Name);
        }

        private static string FullName(MetadataReader metadata, TypeReferenceHandle handle)
        {
            var reference = metadata.GetTypeReference(handle);

            return reference.ResolutionScope.Kind == HandleKind.TypeReference
                ? FullName(metadata, (TypeReferenceHandle)reference.ResolutionScope) + "+" + metadata.GetString(reference.Name)
                : Qualify(metadata.GetString(reference.Namespace), metadata.GetString(reference.Name));
        }

        private static string Qualify(string typeNamespace, string typeName)
            => typeNamespace.Length == 0 ? typeName : typeNamespace + "." + typeName;

        private sealed class TypeNameProvider : ICustomAttributeTypeProvider<string>
        {
            private const string SystemTypeName = "System.Type";

            internal static readonly TypeNameProvider Instance = new TypeNameProvider();

            public string GetPrimitiveType(PrimitiveTypeCode typeCode)
                => "System." + typeCode;

            public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
                => FullName(reader, handle);

            public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
                => FullName(reader, handle);

            public string GetSZArrayType(string elementType)
                => elementType + "[]";

            public string GetSystemType()
                => SystemTypeName;

            public bool IsSystemType(string type)
                => type == SystemTypeName;

            public string GetTypeFromSerializedName(string name)
                => name;

            public PrimitiveTypeCode GetUnderlyingEnumType(string type)
            {
                var enumType = Type.GetType(type, false);
                if (enumType == null || !enumType.IsEnum)
                    throw new NotSupportedException($"The underlying type of the enum '{type}' cannot be resolved from the test runtime.");

                return (PrimitiveTypeCode)Enum.Parse(typeof(PrimitiveTypeCode), Enum.GetUnderlyingType(enumType).Name);
            }
        }
    }
}
