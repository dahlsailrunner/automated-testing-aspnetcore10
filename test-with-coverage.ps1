Remove-Item -Recurse -Force -ErrorAction SilentlyContinue TestResults, coveragereport

dotnet test --coverage --coverage-output-format cobertura 

reportgenerator -reports:TestResults/*.cobertura.xml -targetdir:coveragereport

./coveragereport/index.html

./TestResults/*.html