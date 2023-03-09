// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class TestSearchRuleInfoTests
{
    [Fact]
    public void ctor_Defaults()
    {
        TestSearchRuleInfo info = new();
        Assert.Equal(6, info.GetType().GetProperties().Length);
        Assert.Null(info.Path);
        Assert.False(info.IsIncluded);
        Assert.Equal(CLUSION_REASON.CLUSIONREASON_UNKNOWNSCOPE, info.Reason);
        Assert.False(info.HasChildScope);
        Assert.False(info.HasParentScope);
        Assert.Equal(0, info.ParentScopeVersiondId);
    }

    [Fact]
    public void Clone_Succeeds()
    {
        TestSearchRuleInfo info = new()
        {
            Path = "foo",
            IsIncluded = true,
            Reason = CLUSION_REASON.CLUSIONREASON_GROUPPOLICY,
            HasParentScope = true,
            ParentScopeVersiondId = 111
        };
        TestSearchRuleInfo clone = (TestSearchRuleInfo)info.Clone();
        Assert.Equal(info, clone, ShallowFieldComparer.Instance);
    }
}
