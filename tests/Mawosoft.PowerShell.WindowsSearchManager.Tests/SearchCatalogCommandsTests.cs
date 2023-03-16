// Copyright (c) 2023 Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager.Tests;

public class SearchCatalogCommandsTests : CommandTestBase
{
    private class MockSearchManagerWithMultipleCatalogs : MockSearchManager2
    {
        internal List<ISearchCatalogManager> CatalogManagers { get; set; }

        public MockSearchManagerWithMultipleCatalogs() : base()
        {
            CatalogManagers = new()
            {
                new MockCatalogManager(),
                new MockCatalogManager()
                {
                    Name = "FooCatalog",
                    Status = _CatalogStatus.CATALOG_STATUS_INCREMENTAL_CRAWL,
                    PausedReason = _CatalogPausedReason.CATALOG_PAUSED_REASON_NONE
                },
                new MockCatalogManager()
                {
                    Name = "BarCatalog",
                    NumberOfItemsInternal = 2222,
                    DiacriticSensitivity = 1
                }
            };
        }

        public override ISearchCatalogManager GetCatalog(string pszCatalog)
        {
            if (CatalogManagerException != null) throw CatalogManagerException;
            for (int i = 0; i < CatalogManagers.Count; i++)
            {
                if (pszCatalog == CatalogManagers[i].Name) return CatalogManagers[i];
            }
            throw new COMException(null, unchecked((int)0x80042103)); // MSS_E_CATALOGNOTFOUND
        }
    }

    private class MockCatalogManagerWithGetSetException : MockCatalogManager
    {
        internal Exception? GetException { get; set; }
        internal Exception? SetException { get; set; }

        public override uint ConnectTimeout
        {
            get => GetException == null ? base.ConnectTimeout : throw GetException;
            set
            {
                if (SetException != null) throw SetException;
                base.ConnectTimeout = value;
            }
        }
        public override uint DataTimeout
        {
            get => GetException == null ? base.DataTimeout : throw GetException;
            set
            {
                if (SetException != null) throw SetException;
                base.DataTimeout = value;
            }
        }
        public override int DiacriticSensitivity
        {
            get => GetException == null ? base.DiacriticSensitivity : throw GetException;
            set
            {
                if (SetException != null) throw SetException;
                base.DiacriticSensitivity = value;
            }
        }

    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("FooCatalog", false)]
    [InlineData("BarCatalog", true)]
    public void GetSearchCatalog_Succeeds(string? catalogName, bool positional)
    {
        MockSearchManagerWithMultipleCatalogs searchManager = new();
        InterfaceChain.WithSearchManager(searchManager);
        List<ISearchCatalogManager> expected = searchManager.CatalogManagers;
        InterfaceChain.Factory.SearchRegistryProvider.CatalogNames = searchManager.CatalogManagers.ConvertAll(c => c.Name);
        string script = "Get-SearchCatalog ";
        if (catalogName != null)
        {
            if (!positional) script += "-Catalog ";
            script += $"'{catalogName}' ";
            expected = expected.FindAll(c => c.Name == catalogName);
        }
        PowerShell.AddScript(script);
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.All(results, (item, i) => Assert.Equal(expected[i], item.BaseObject, SearchCatalogInfoToMockComparer.Instance));
        Assert.Equal(expected.Count, results.Count);
        Assert.False(PowerShell.HadErrors);
    }

    [Fact]
    public void GetSearchCatalog_WithFailures_Succeeds()
    {
        MockSearchManagerWithMultipleCatalogs searchManager = new();
        InterfaceChain.WithSearchManager(searchManager);
        List<ISearchCatalogManager> expected = new(searchManager.CatalogManagers);
        searchManager.CatalogManagers.Insert(1, new MockCatalogManagerWithGetSetException()
        {
            Name = "BadCatalog1",
            GetException = new Exception()
        });
        InterfaceChain.Factory.SearchRegistryProvider.CatalogNames = searchManager.CatalogManagers.ConvertAll(c => c.Name);
        InterfaceChain.Factory.SearchRegistryProvider.CatalogNames.Insert(2, "NotFound2");
        PowerShell.AddScript("Get-SearchCatalog");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.All(results, (item, i) => Assert.Equal(expected[i], item.BaseObject, SearchCatalogInfoToMockComparer.Instance));
        Assert.Equal(expected.Count, results.Count);
        Assert.True(PowerShell.HadErrors);
        PSDataCollection<ErrorRecord> errors = PowerShell.Streams.Error;
        Assert.Collection(errors,
            e => Assert.Equal("BadCatalog1", e.TargetObject),
            e => Assert.Equal("NotFound2", e.TargetObject)
            );
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void GetSearchCatalog_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.WithCatalogManager(new MockCatalogManagerWithGetSetException()
        {
            GetException = exceptionParam.Value.Exception,
            SetException = exceptionParam.Value.Exception
        });
        PowerShell.AddScript("Get-SearchCatalog");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }

    [Theory]
    [InlineData("-Catalog ")]
    [InlineData("-Catalog '' ")]
    [InlineData("$null ")]
    [InlineData("'' ")]
    public void GetSearchCatalog_ParameterValidation_Succeeds(string arguments)
    {
        AssertParameterValidation("Get-SearchCatalog " + arguments);
    }

    private class SetSearchCatalog_TheoryData : TheoryData<string, SearchCatalogInfo>
    {
        public SetSearchCatalog_TheoryData()
        {
            Add("-DiacriticSensitivity ",
                new SearchCatalogInfo(new MockCatalogManager())
                {
                    DiacriticSensitivity = true
                });
            Add("-ConnectTimeout 111 -DataTimeout 333 -Catalog FooCatalog ",
                new SearchCatalogInfo(new MockCatalogManager())
                {
                    Catalog = "FooCatalog",
                    ConnectTimeout = 111,
                    DataTimeout = 333
                });
        }
    }

    [Theory]
    [ClassData(typeof(SetSearchCatalog_TheoryData))]
    public void SetSearchCatalog_Succeeds(string arguments, SearchCatalogInfo expectedInfo)
    {
        MockSearchManagerWithMultipleCatalogs searchManager = new()
        {
            CatalogManagers = new()
            {
                new MockCatalogManager(),
                new MockCatalogManager() { Name = "FooCatalog"}
            }
        };
        InterfaceChain.WithSearchManager(searchManager);
        ISearchCatalogManager? catalogManager = searchManager.CatalogManagers.Find(c => c.Name == expectedInfo.Catalog);
        if (catalogManager == null)
        {
            catalogManager = new MockCatalogManager() { Name = expectedInfo.Catalog! };
            searchManager.CatalogManagers.Add(catalogManager);
        }
        PowerShell.AddScript("Set-SearchCatalog " + arguments);
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal(expectedInfo, catalogManager, SearchCatalogInfoToMockComparer.Instance);
    }

    [Theory]
    [ClassData(typeof(Exception_TheoryData))]
    public void SetSearchCatalog_HandlesFailures(ExceptionParam exceptionParam)
    {
        InterfaceChain.WithCatalogManager(new MockCatalogManagerWithGetSetException()
        {
            SetException = exceptionParam.Value.Exception
        });
        PowerShell.AddScript("Set-SearchCatalog -DiacriticSensitivity ");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        AssertSingleErrorRecord(exceptionParam);
    }


    [Fact]
    public void SetSearchCatalog_ConfirmImpact_Medium()
    {
        AssertConfirmImpact(typeof(SetSearchCatalogCommand), ConfirmImpact.Medium);
    }

    [Theory]
    [InlineData("-Catalog ")]
    [InlineData("-Catalog '' ")]
    [InlineData("-ConnectTimeout -1 ")]
    [InlineData("-DataTimeout -1 ")]
    public void SetSearchCatalog_ParameterValidation_Succeeds(string arguments)
    {
        AssertParameterValidation("Set-SearchCatalog " + arguments);
    }

    [Theory]
    [InlineData("-ConnectTimeout 100 ")]
    [InlineData("-DataTimeout 100 ")]
    [InlineData("-DiacriticSensitivity ")]
    [InlineData("-ConnectTimeout 100 -DataTimeout 100 -DiacriticSensitivity ")]
    public void SetSearchCatalog_WhatIf_Succeeds(string arguments)
    {
        SearchCatalogInfo expectedInfo = new(InterfaceChain.CatalogManager);
        PowerShell.AddScript("Set-SearchCatalog " + arguments + " -WhatIf ");
        Collection<PSObject> results = PowerShell.Invoke();
        Assert.Empty(results);
        Assert.False(PowerShell.HadErrors);
        Assert.Equal(expectedInfo, InterfaceChain.CatalogManager, SearchCatalogInfoToMockComparer.Instance);
    }
}
