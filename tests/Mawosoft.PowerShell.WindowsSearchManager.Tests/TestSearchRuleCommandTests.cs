// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class TestSearchRuleCommandTests : CommandTestBase
{
    private static readonly List<TestSearchRuleInfo> s_testInfos = new()
    {
        new()
        {
            Path = @"x:\foo",
            IsIncluded = true,
            Reason = CLUSION_REASON.CLUSIONREASON_DEFAULT,
            HasChildScope = true,
            HasParentScope = true,
            ParentScopeVersiondId = 111
        },
        new()
        {
            Path = @"x:\bar",
            IsIncluded = true,
            Reason = CLUSION_REASON.CLUSIONREASON_GROUPPOLICY,
            HasChildScope = false,
            HasParentScope = false,
            ParentScopeVersiondId = 0
        },
        new()
        {
            Path = @"x:\foo\bar",
            IsIncluded = false,
            Reason = CLUSION_REASON.CLUSIONREASON_DEFAULT,
            HasChildScope = true,
            HasParentScope = false,
            ParentScopeVersiondId = 0
        },
        new()
        {
            Path = @"x:\bar\foo",
            IsIncluded = false,
            Reason = CLUSION_REASON.CLUSIONREASON_USER,
            HasChildScope = false,
            HasParentScope = true,
            ParentScopeVersiondId = 222
        },
    };

    public static readonly object?[][] Succeeds_TestData =
        new string[]
        {
            $@"Test-SearchRule -Path '{s_testInfos[0].Path}'",
            $@"Test-SearchRule '{s_testInfos[0].Path}'",
            $@"Test-SearchRule -Path '{string.Join("', '", s_testInfos.Select(t => t.Path))}'",
            $@"Test-SearchRule '{string.Join("', '", s_testInfos.Select(t => t.Path))}'",
            $@"@('{string.Join("', '", s_testInfos.Select(t => t.Path))}') | Test-SearchRule",
            $@"@([pscustomobject]@{{ Path = '{string.Join("'}, [pscustomobject]@{ Path = '", s_testInfos.Select(t => t.Path))}' }}) | Test-SearchRule",
        }
        .CrossJoin(new string[][]
        {
            new string[] { "", "IsIncluded" },
            new string[] { "-IsIncluded", "IsIncluded" },
            new string[] { "-HasChildScope", "HasChildScope" },
            new string[] { "-HasParentScope", "HasParentScope" },
            new string[] { "-Detailed", "" },
        })
        .ToArray();

    [Theory]
    [MemberData(nameof(Succeeds_TestData))]
    public void Command_Succeeds(string script, string switchParameter, string? propertyName)
    {
        InterfaceChain.ScopeManager.TestRuleInfos = new(s_testInfos);
        int valueCount = script.IndexOf(',') < 0 ? 1 : s_testInfos.Count;
        script += " " + switchParameter;
        Collection<PSObject> results = InvokeScript(script);
        Assert.False(PowerShell.HadErrors);
        Assert.False(InterfaceChain.HasWriteRecordings());
        if (string.IsNullOrEmpty(propertyName))
        {
            var expected = s_testInfos.Take(valueCount);
            var actual = results.Select(o => (TestSearchRuleInfo)o.BaseObject);
            Assert.Equivalent(expected, actual);
        }
        else
        {
            PropertyInfo pi = typeof(TestSearchRuleInfo).GetProperty(propertyName)!;
            Assert.NotNull(pi);
            var expected = s_testInfos.Take(valueCount).Select(t => (bool)pi.GetValue(t)!);
            var actual = results.Select(o => (bool)o.BaseObject);
            Assert.Equal(expected, actual);
        }
    }

    public static readonly object?[][] HandlesFailures_TestData =
        new string[][]
        {
            new string[] { "-IsIncluded", "IsIncluded" },
            new string[] { "-HasChildScope", "HasChildScope" },
            new string[] { "-HasParentScope", "HasParentScope" },
            new string[] { "-Detailed", "" },
        }
        .CrossJoin(new Exception_TheoryData())
        .ToArray();

    [Theory]
    [MemberData(nameof(HandlesFailures_TestData))]
    public void Command_WithFailures_PartiallySucceeds(string switchParameter, string? propertyName, ExceptionParam exceptionParam)
    {
        InterfaceChain.ScopeManager.TestRuleInfos = new(s_testInfos);
        Assert.InRange(s_testInfos.Count, 4, s_testInfos.Count);
        InterfaceChain.ScopeManager.TestRuleInfos[1] = exceptionParam.Exception;
        InterfaceChain.ScopeManager.TestRuleInfos[2] = exceptionParam.Exception;
        List<TestSearchRuleInfo> expectedInfos = s_testInfos.Take(1).Concat(s_testInfos.Skip(3)).ToList();

        string script = $"@('{string.Join("', '", s_testInfos.Select(t => t.Path))}') | Test-SearchRule {switchParameter}";
        Collection<PSObject> results = InvokeScript(script);
        Assert.True(PowerShell.HadErrors);
        Assert.False(InterfaceChain.HasWriteRecordings());
        if (string.IsNullOrEmpty(propertyName))
        {
            var actual = results.Select(o => (TestSearchRuleInfo)o.BaseObject);
            Assert.Equivalent(expectedInfos, actual);
        }
        else
        {
            PropertyInfo pi = typeof(TestSearchRuleInfo).GetProperty(propertyName)!;
            Assert.NotNull(pi);
            var expected = expectedInfos.Select(t => (bool)pi.GetValue(t)!);
            var actual = results.Select(o => (bool)o.BaseObject);
            Assert.Equal(expected, actual);
        }
        Assert.Collection(PowerShell.Streams.Error,
            e => Assert.Equal($"SystemIndex Path={s_testInfos[1].Path}", e.TargetObject),
            e => Assert.Equal($"SystemIndex Path={s_testInfos[2].Path}", e.TargetObject));
    }
}
