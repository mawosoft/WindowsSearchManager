﻿// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchApiErrorHelperTests
{
    private class Exception_TheoryData : TheoryData<ExceptionParam, bool>
    {
        private const uint MSS_E_CATALOGNOTFOUND = 0x80042103;
        private const uint OLEDB_BINDER_CUSTOM_ERROR = 0x80042500;
        private const uint GTHR_E_SINGLE_THREADED_EMBEDDING = 0x80040DA5;
        private const uint E_INVALIDARG = 0x8007000E;
        private const uint MSS_E_CATALOGNOTFOUND_notITF = 0x80112103;

        public Exception_TheoryData()
        {
            Add(new ExceptionParam(null!), false);
            Add(new ExceptionParam(new ExternalException(null, unchecked((int)MSS_E_CATALOGNOTFOUND)), "not COMException"), false);
            Add(new ExceptionParam(new COMException(null, unchecked((int)E_INVALIDARG)), "E_INVALIDARG"), false);
            Add(new ExceptionParam(new COMException(null, unchecked((int)MSS_E_CATALOGNOTFOUND_notITF)), "not facility ITF"), false);
            Add(new ExceptionParam(new COMException(null, unchecked((int)MSS_E_CATALOGNOTFOUND)), "existing msg"), true);
            Add(new ExceptionParam(new COMException(null, unchecked((int)OLEDB_BINDER_CUSTOM_ERROR)), "existing msg with inserts"), true);
            Add(new ExceptionParam(new COMException(null, unchecked((int)GTHR_E_SINGLE_THREADED_EMBEDDING)), "existing msg long (290 ch)"), true);
        }
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void TryGetCOMExceptionMessage_Succeeds(ExceptionParam exceptionParam, bool shouldSucceed)
    {
        Exception exception = exceptionParam.Value;
        bool success = SearchApiErrorHelper.TryGetCOMExceptionMessage(exception, out string message);
        Assert.Equal(shouldSucceed, success);
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
    public void TryWrapCOMException_Succeeds(ExceptionParam exceptionParam, bool shouldSucceed)
    {
        Exception exception = exceptionParam.Value;
        bool success = SearchApiErrorHelper.TryWrapCOMException(exception, out ErrorRecord errorRecord);
        Assert.Equal(shouldSucceed, success);
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
    public void TrySetErrorDetails_Succeeds(ExceptionParam exceptionParam, bool shouldSucceed)
    {
        Exception exception = exceptionParam.Value;
        if (exception == null) return; // Cannot create ErrorRecord w/o exception
        ErrorRecord errorRecord = new(exception, string.Empty, ErrorCategory.NotSpecified, null);
        bool success = SearchApiErrorHelper.TrySetErrorDetails(errorRecord);
        Assert.Equal(shouldSucceed, success);
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
