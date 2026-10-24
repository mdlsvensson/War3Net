# Prints the failed tests of a .trx file, one "class.method: test name" per line, sorted. The fork's workflows compare this with
# .github/known-failures/<project>.txt: a failure that list does not hold fails the run.
param([Parameter(Mandatory = $true)][string]$Trx)

[xml]$run = Get-Content -LiteralPath $Trx -Raw
$ns = [System.Xml.XmlNamespaceManager]::new($run.NameTable)
$ns.AddNamespace('t', 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010')

$methods = @{}
foreach ($definition in $run.SelectNodes('//t:UnitTest', $ns)) {
    $method = $definition.SelectSingleNode('t:TestMethod', $ns)
    $methods[$definition.id] = "$($method.className).$($method.name)"
}

$failed = foreach ($result in $run.SelectNodes('//t:UnitTestResult[@outcome="Failed"]', $ns)) {
    "$($methods[$result.testId]): $($result.testName)"
}

$failed | Sort-Object -Unique -CaseSensitive
