using DbUp;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
var dbMigrationsDirectory = currentDirectory.Parent!.Parent!.Parent!;

var path = Path.Combine(dbMigrationsDirectory.FullName, "Sql");
var sqlDirectory = Directory.GetDirectories(path);

var migrations = sqlDirectory
    .ToDictionary(directory
        => directory, directory
        => configuration.GetConnectionString(Path.GetFileName(directory)));

foreach (var (filePath, connectionString) in migrations)
{
    if (connectionString is null)
    {
        PrintError($"No connection string for filepath: {filePath}");
        continue;
    }

    if (!Directory.Exists(filePath))
    {
        Console.WriteLine($"Migration file not found: {filePath}");
        return -1;
    }

    EnsureDatabase.For.PostgresqlDatabase(connectionString);

    var upgrader =
        DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsFromFileSystem(filePath)
            .LogToConsole()
            .Build();

    var result = upgrader.PerformUpgrade();

    if (!result.Successful)
    {
        PrintException(result.Error);
#if DEBUG
        Console.ReadLine();
#endif
        return -1;
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Database migration completed successfully!");
    Console.ResetColor();
}

return 0;

void PrintError(string error)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(error);
    Console.ResetColor();
}

void PrintException(Exception error)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(error);
    Console.ResetColor();
}