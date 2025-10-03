#:sdk Cake.Sdk@CakeSdkVersion

Task("TrivialTask")
    .Does(() =>
{
    Information("Hello, World!");
});

RunTarget("TrivialTask"); 