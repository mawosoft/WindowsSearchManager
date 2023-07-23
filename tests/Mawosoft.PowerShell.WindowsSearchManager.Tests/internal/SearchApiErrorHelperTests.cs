// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchApiErrorHelperTests
{
    private class Exception_TheoryData : TheoryData<ExceptionParam>
    {
        private const int E_INVALIDARG = unchecked((int)0x8007000E);
        private const int MSS_E_CATALOGNOTFOUND_notITF = unchecked((int)0x80112103);

        public Exception_TheoryData()
        {
            Add(new ExceptionParam(null!));
            Add(new ExceptionParam(new ExternalException(null, ExceptionParam.MSS_E_CATALOGNOTFOUND))); // not COMException
            Add(new ExceptionParam(new COMException(null, E_INVALIDARG)));
            Add(new ExceptionParam(new COMException(null, MSS_E_CATALOGNOTFOUND_notITF))); // not facility ITF
            Add(new ExceptionParam(new COMException(null, ExceptionParam.MSS_E_CATALOGNOTFOUND))); // SearchApi msg
            Add(new ExceptionParam(new COMException(null, ExceptionParam.OLEDB_BINDER_CUSTOM_ERROR))); // SearchApi msg with inserts
            Add(new ExceptionParam(new COMException(null, ExceptionParam.GTHR_E_SINGLE_THREADED_EMBEDDING))); // SearchApi msg long (290 ch)
        }
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void TryGetCOMExceptionMessage_Succeeds(ExceptionParam exceptionParam)
    {
        Exception exception = exceptionParam.Exception;
        bool success = SearchApiErrorHelper.TryGetCOMExceptionMessage(exception, out string message);
        Assert.Equal(exceptionParam.IsSearchApi, success);
        if (success)
        {
            Assert.NotNull(message);
            Assert.NotEqual(exception.Message, message);
        }
        else
        {
            Assert.Null(message);
        }
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void TryWrapCOMException_Succeeds(ExceptionParam exceptionParam)
    {
        Exception exception = exceptionParam.Exception;
        bool success = SearchApiErrorHelper.TryWrapCOMException(exception, out ErrorRecord errorRecord);
        Assert.Equal(exceptionParam.IsSearchApi, success);
        if (success)
        {
            Assert.NotNull(errorRecord);
            Assert.Same(exception, errorRecord.Exception);
            Assert.NotEqual(exception.Message, errorRecord.ErrorDetails.Message);
        }
        else
        {
            Assert.Null(errorRecord);
        }
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void TrySetErrorDetails_Succeeds(ExceptionParam exceptionParam)
    {
        Exception exception = exceptionParam.Exception;
        if (exception is null) return; // Cannot create ErrorRecord w/o exception
        ErrorRecord errorRecord = new(exception, string.Empty, ErrorCategory.NotSpecified, null);
        bool success = SearchApiErrorHelper.TrySetErrorDetails(errorRecord);
        Assert.Equal(exceptionParam.IsSearchApi, success);
        if (success)
        {
            Assert.NotEqual(exception.Message, errorRecord.ErrorDetails.Message);
        }
        else
        {
            Assert.Null(errorRecord.ErrorDetails);
        }
    }
}
