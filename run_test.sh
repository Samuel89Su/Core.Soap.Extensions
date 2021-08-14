for test in $(find test/ -name "*.csproj")
do
    echo test project ${test} found!
    echo run test ${test}...

    dotnet test ${test} -c:Release --collect:"XPlat Code Coverage" -r:"./test/TestResults" -l:"console;verbosity=quiet"
    echo run test ${test} "done"
done

reportgenerator -reports:"./test/TestResults/*/coverage.cobertura.xml" -targetdir:"./test/TestResults/report" -reporttypes:Html -historydir:"./test/TestResults/histories"