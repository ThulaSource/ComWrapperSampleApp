using System;
using System.Linq;
using System.Xml.Linq;
using FM.Common.DataModel.EpjApi;
using FM.Common.DataModel.EpjApiDocumentConverters;
using FM.Common.Framework;
using Xml.Schema.Linq;

namespace COMWrapperSampleApp.Logic
{
    public class EpjApiDeserializationResult<T> where T : XTypedElement
    {
        public T Result { get; set; }

        public XNamespace Namespace { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        public bool UnsupportedNamespace { get; set; }
    }

    public interface IEpjApiSerializer
    {
        /// <summary>
        /// Tries to parse, validate and convert the incoming EPJ API document to the current version of the EPJ API schema
        /// </summary>
        /// <typeparam name="T">The type of EPJ API document</typeparam>
        /// <param name="xml">the incoming EPJ API document</param>
        /// <returns>A Tuple where the first item is the parsed and validated input document, converted to the current EPJ API schema version, 
        /// and the second item is the namespace of the incoming document (used when converting the svar before returning it)</returns>
        EpjApiDeserializationResult<T> DeserializeAndValidate<T>(string xml) where T : XTypedElement, new();

        string Serialize<T>(T @object, XNamespace @namespace) where T : XTypedElement;
    }

    public class EpjApiSerializer : IEpjApiSerializer
    {
        private readonly IXDocumentValidator documentValidator;
        private readonly IEpjApiRootDocumentConverter documentConverter;

        public EpjApiSerializer(IXDocumentValidator documentValidator, IEpjApiRootDocumentConverter documentConverter)
        {
            this.documentValidator = documentValidator;
            this.documentConverter = documentConverter;
        }
        
        public EpjApiDeserializationResult<T> DeserializeAndValidate<T>(string xml) where T : XTypedElement, new()
        {
            try
            {
                var doc = XDocument.Parse(xml);
                var rootElement = doc.Root;
                var originalNamespace = rootElement?.GetDefaultNamespace();
                var originalNamespaceName = originalNamespace?.NamespaceName;
                // RENO-10268: if the namespace is not supported, reply on the oldest supported namespace.
                if (rootElement != null && !EpjApiConverterNameSpace.IsNamespaceSupported(originalNamespaceName))
                {
                    return new EpjApiDeserializationResult<T>
                    {
                        HasError = true,
                        ErrorMessage = $"Internal error: the namespace '{originalNamespaceName}' is not supported",
                        UnsupportedNamespace = true
                    };
                }

                // lookup the XName for the specified type
                ILinqToXsdTypeManager tm = LinqToXsdTypeManager.Instance;
                var name = (from el in tm.GlobalElementDictionary where el.Value == typeof(T) select el.Key).FirstOrDefault();
                if (name == null)
                {
                    return new EpjApiDeserializationResult<T>
                    {
                        HasError = true,
                        ErrorMessage = $"Internal error: could not find XName definition for type '{typeof(T).FullName}'",
                    };
                }
                
                // make sure that the xml is of the correct type)
                if (rootElement == null || rootElement.Name.LocalName != name.LocalName)
                {
                    return new EpjApiDeserializationResult<T>
                    {
                        HasError = true,
                        ErrorMessage = $"XML root element is not correct: should be '{name}' but is '{rootElement?.Name.LocalName ?? "null"}'",
                    };
                }

                // validate the xml
                var tuple = documentValidator.IsValid(doc);
                var valid = tuple.Item1;
                var validationMessages = tuple.Item2;
                if (!valid)
                {
                    return new EpjApiDeserializationResult<T>
                    {
                        HasError = true,
                        ErrorMessage = $"XML did not validate: {validationMessages}",
                    };
                }

                // convert to the latest EPJ API namespace if this is an old version we are receiving
                var convertedDocument = documentConverter.ConvertDocument(rootElement);

                // Store the original namespace 
                convertedDocument.Add(new XAttribute("OriginalNamespace", originalNamespaceName));
                var result = new T();
                result.Untyped = convertedDocument;
                return new EpjApiDeserializationResult<T>
                {
                    Namespace = originalNamespace,
                    Result = result
                };
            }
            catch (Exception e)
            {
                return new EpjApiDeserializationResult<T>
                {
                    HasError = true,
                    ErrorMessage = $"Could not parse XML: {e.Message}",
                };
            }
        }

        public string Serialize<T>(T @object, XNamespace @namespace) where T : XTypedElement
        {
            var result = documentConverter.ConvertSvar(@object.Untyped, @namespace);
            return result.ToString();
        }
    }
}