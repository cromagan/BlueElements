using System.Runtime.CompilerServices;
using System.Text;

namespace BlueBasics;

internal static class EncodingInitializer {

    [ModuleInitializer]
    internal static void Init() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

}
