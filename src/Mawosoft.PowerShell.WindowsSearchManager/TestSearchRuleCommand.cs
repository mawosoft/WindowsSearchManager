// Copyright (c) Matthias Wolf, Mawosoft.

namespace Mawosoft.PowerShell.WindowsSearchManager;

/// <summary>
/// Tests specified paths against the search rules of a search catalog.
/// </summary>
[Cmdlet(VerbsDiagnostic.Test, Nouns.SearchRule, DefaultParameterSetName = IncludedParameterSet,
    ConfirmImpact = ConfirmImpact.None)]
[OutputType(typeof(bool), typeof(TestSearchRuleInfo))]
public sealed class TestSearchRuleCommand : SearchApiCommandBase
{
    private const string IncludedParameterSet = "IncludedParameterSet";
    private const string ChildScopeParameterSet = "ChildScopeParameterSet";
    private const string ParentScopeParameterSet = "ParentScopeParameterSet";
    private const string DetailedParameterSet = "DetailedParameterSet";

    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty()]
    public string[]? Path { get; set; }

    [Parameter(Mandatory = false, ParameterSetName = IncludedParameterSet)]
    public SwitchParameter IsIncluded { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = ChildScopeParameterSet)]
    public SwitchParameter HasChildScope { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = ParentScopeParameterSet)]
    public SwitchParameter HasParentScope { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = DetailedParameterSet)]
    public SwitchParameter Detailed { get; set; }

    [Parameter]
    [ValidateNotNullOrEmpty()]
    public string Catalog { get; set; } = DefaultCatalogName;

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Continue after WriteError.")]
    protected override void ProcessRecord()
    {
        if (!(Path?.Length > 0)) return;

        ISearchCrawlScopeManager scope = GetCrawlScopeManager(Catalog);

        foreach (string path in Path)
        {
            try
            {
                switch (ParameterSetName)
                {
                    case IncludedParameterSet:
                        WriteObject(scope.IncludedInCrawlScope(path) != 0);
                        break;
                    case ChildScopeParameterSet:
                        WriteObject(scope.HasChildScopeRule(path) != 0);
                        break;
                    case ParentScopeParameterSet:
                        WriteObject(scope.HasParentScopeRule(path) != 0);
                        break;
                    default:
                        scope.IncludedInCrawlScopeEx(path, out int isIncluded, out CLUSION_REASON reason);
                        TestSearchRuleInfo info = new()
                        {
                            Path = path,
                            IsIncluded = isIncluded != 0,
                            Reason = reason,
                            HasChildScope = scope.HasChildScopeRule(path) != 0,
                            HasParentScope = scope.HasParentScopeRule(path) != 0,
                            ParentScopeVersiondId = scope.GetParentScopeVersionId(path)
                        };
                        WriteObject(info);
                        break;
                }
            }
            catch (Exception ex)
            {
                string target = $"{Catalog} {nameof(Path)}={path}";
                ErrorRecord rec = new(ex, string.Empty, ErrorCategory.NotSpecified, target);
                SearchApiErrorHelper.TrySetErrorDetails(rec);
                WriteError(rec);
            }
        }
    }
}
