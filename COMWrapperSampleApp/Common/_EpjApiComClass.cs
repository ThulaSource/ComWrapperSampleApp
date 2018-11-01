using System.Runtime.InteropServices;

namespace COMWrapperSampleApp.Common
{
    [Guid("D4A30B5E-C787-41EE-8E45-3723A3FD545A")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface _EpjApiComClass
    {
        string StartPasient(string parameter);
    }
}
 
