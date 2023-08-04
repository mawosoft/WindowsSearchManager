// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Helper for handling errors returned from the SearchAPI interfaces.
/// </summary>
internal static class SearchApiErrorHelper
{
    private const string Kernel32 = "kernel32.dll";

    private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
    private const uint LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;

    private const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
    private const uint FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;

    private const int ERROR_INSUFFICIENT_BUFFER = 0x7A;

    // Windows Search dll containg the MESSAGETABLE for all SearchAPI errors.
    // See C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\um\WindowsSearchErrors.h
    private const string TQueryDll = "tquery.dll";

    [DllImport(Kernel32, ExactSpelling = true, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern bool FreeLibrary(IntPtr hModule);

    [DllImport(Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "External name.")]
    private static extern IntPtr LoadLibraryEx([In] string lpLibFileName, IntPtr hFile, uint dwFlags);

    [DllImport(Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId,
                                            [Out] char[] lpBuffer, int nSize, IntPtr arguments);

    // Returns a message for the given HResult, or null if no matching message is found.
    // Any possible message parameters (inserts) are ignored.
    private static string? GetMessageFromHResult(int HResult)
    {
        IntPtr hModule = LoadLibraryEx(TQueryDll, IntPtr.Zero,
                                       LOAD_LIBRARY_AS_DATAFILE | LOAD_LIBRARY_SEARCH_SYSTEM32);
        if (hModule == IntPtr.Zero) return null;

        try
        {
            for (int bufferLength = 256; bufferLength <= 2048; bufferLength *= 2)
            {
                char[] buffer = new char[bufferLength];
                int msgLength = FormatMessage(
                    FORMAT_MESSAGE_FROM_HMODULE | FORMAT_MESSAGE_IGNORE_INSERTS,
                    hModule, unchecked((uint)HResult), 0, buffer, buffer.Length, IntPtr.Zero);
                if (msgLength > 0)
                {
                    return new string(buffer, 0, msgLength);
                }
                if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER) break;
            }
            return null;
        }
        finally
        {
            FreeLibrary(hModule);
        }
    }

    /// <summary>
    /// Try to get a SearchAPI-specific message for given COMException.
    /// Accepts generic Exception for convenience.
    /// </summary>
    public static bool TryGetCOMExceptionMessage(Exception exception, out string message)
    {
        // We cannot annotate in netstandard2.0. The proper 'string? message' would leave the
        // burden of exclamation marks to the caller.
        message = default!;
        if (exception is not COMException comException) return false;
        // Don't bother if the facility code isn't FACILITY_ITF (4).
        if (((comException.HResult >> 16) & 0x1FFF) != 4) return false;
        message = GetMessageFromHResult(comException.HResult)!;
        return message is not null;
    }

    /// <summary>
    /// Try to wrap a COMException in an ErrorRecord with SearchAPI-specific message.
    /// Accepts generic Exception for convenience.
    /// </summary>
    public static bool TryWrapCOMException(Exception exception, out ErrorRecord errorRecord)
    {
        errorRecord = default!;
        if (!TryGetCOMExceptionMessage(exception, out string message)) return false;
        errorRecord = new(exception, string.Empty, ErrorCategory.NotSpecified, null)
        {
            ErrorDetails = new(message)
        };
        return true;
    }

    /// <summary>
    /// Try to set ErrorDetails to a SearchAPI-specific message.
    /// </summary>
    public static bool TrySetErrorDetails(ErrorRecord errorRecord)
    {
        if (!TryGetCOMExceptionMessage(errorRecord.Exception, out string message)) return false;
        errorRecord.ErrorDetails = new(message);
        return true;
    }
}
