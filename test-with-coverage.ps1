Remove-Item -Recurse -Force -ErrorAction SilentlyContinue TestResults, coveragereport

dotnet test --coverage --coverage-output-format cobertura --coverage-settings testconfig.json

reportgenerator -reports:TestResults/*.cobertura.xml -targetdir:coveragereport

./coveragereport/index.html

./TestResults/*.html