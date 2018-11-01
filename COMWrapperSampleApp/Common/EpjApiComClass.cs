using COMWrapperSampleApp.Logic;
using COMWrapperSampleApp.StructureMap;
using FM.Common.DataModel.EpjApi;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using Xml.Schema.Linq;

namespace COMWrapperSampleApp.Common
{
    [Guid("51B16CC1-BE7E-4185-B863-6BAE77207B90")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ProgId("eResept.Forskrivningsmodul.1")]
    [ComVisible(true)]
    [Serializable]
    public class EpjApiComClass : _EpjApiComClass
    {
        public static void Register()
        {
            var services = new RegistrationServices();
            services.RegisterTypeForComClients(typeof(EpjApiComClass), RegistrationClassContext.LocalServer, RegistrationConnectionType.MultipleUse);
        }

        public static void Install(Module module)
        {
            var services = new RegistrationServices();
            var ass = Assembly.GetExecutingAssembly();
            services.RegisterAssembly(ass, AssemblyRegistrationFlags.SetCodeBase);
            var t = typeof(EpjApiComClass);
            Registry.ClassesRoot.DeleteSubKeyTree("CLSID\\{" + t.GUID + "}\\InprocServer32");
            services.RegisterTypeForComClients(t, RegistrationClassContext.LocalServer, RegistrationConnectionType.MultipleUse);

            var progId = GetProgId(t);
            var guid = GetGuid(t);

            SetRegistryKey(Registry.ClassesRoot, @"CLSID\{" + guid + @"}\LocalServer32", module.FullyQualifiedName);
            SetRegistryKey(Registry.ClassesRoot, @"CLSID\{" + guid + @"}\ProgId", progId);
            SetRegistryKey(Registry.ClassesRoot, @"CLSID\" + progId, guid.ToString());
        }

        private static RegistryKey SetRegistryKey(RegistryKey root, string subkey, string value)
        {
            var key = root.CreateSubKey(subkey);
            if (key == null)
            {
                throw new Exception(string.Format("Could not create registry subkey '{0}' under root '{1}'", subkey, root.Name));
            }
            key.SetValue(null, value);
            return key;
        }

        private static string GetProgId(Type t)
        {
            AttributeCollection attributes = TypeDescriptor.GetAttributes(t);
            var progIdAttr = attributes[typeof(ProgIdAttribute)] as ProgIdAttribute;
            return progIdAttr != null ? progIdAttr.Value : t.FullName;
        }

        private static Guid GetGuid(Type t)
        {
            AttributeCollection attributes = TypeDescriptor.GetAttributes(t);
            var guidAttr = attributes[typeof(GuidAttribute)] as GuidAttribute;
            return guidAttr != null ? new Guid(guidAttr.Value) : Guid.Empty;
        }
        
        public string StartPasient(string parameter)
        {
            return RunOperation<StartPasient>(parameter, Api.StartPasient);
        }

        private string RunOperation<T>(string parameter, Func<T, IEpjApiSvar> action) where T : XTypedElement, new()
        {
            var parameterDeserialization = Serializer.DeserializeAndValidate<T>(parameter);
            if (!parameterDeserialization.HasError)
            {
                var result = (XTypedElement)action(parameterDeserialization.Result);
                return Serializer.Serialize(result, parameterDeserialization.Namespace);
            }

            return parameterDeserialization.ErrorMessage;
        }

        private IApiOperations Api => Bootstrapper.GetInstance<IApiOperations>();

        private IEpjApiSerializer Serializer => Bootstrapper.GetInstance<IEpjApiSerializer>();
    }
} 
