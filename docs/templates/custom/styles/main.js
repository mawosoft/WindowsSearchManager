// Copyright (c) 2023-2024 Matthias Wolf, Mawosoft.

// Very simple language definition to highlight the syntax block of a PowerShell cmdlet.

hljs.registerLanguage('psmeta', function (hljs) {
    const CMDLETS = {
        scope: 'built_in',
        begin: /[A-Z]+-[A-Z]+/,
    };
    const PARAMETERS = {
        scope: 'literal',
        begin: /-[A-Z]+/,
    };
    return {
        case_insensitive: true,
        disableAutodetect: true,
        contains: [CMDLETS, PARAMETERS]
    }
});
