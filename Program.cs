// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using System.CommandLine;

namespace DotNetCmdSample;
class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("sample DotNet CLI for System.CommandLine");

        //新增第一個Command
        var backUpCommand = new Command("backUp", "備份資料");

        //製作選項
        var filePathOption = new Option<string>(
            aliases: new[] { "--orgfolderName", "--oldfolder" },
            description: "欲備份的目錄");
        var newFilePathOption = new Option<string>(
            aliases: new[] { "--newfolderName", "--newfolder" },
            description: "備份目錄名稱");

        var config = new ConfigurationBuilder()
            .AddJsonFile($"jsconfig1.json", optional: false)
            .Build();

        backUpCommand.AddOption(filePathOption);
        backUpCommand.AddOption(newFilePathOption);
        //將選項和引數指派給命令
        backUpCommand.SetHandler((orgFilePath, newFilePath) =>
        {
            CopyDir(orgFilePath, newFilePath, config.GetSection("ignore").Get<string[]>());
        }, filePathOption, newFilePathOption);

        rootCommand.AddCommand(backUpCommand);

        //新增第二個Command
        var readFileCommand = new Command("readFile", "讀取檔案");

        //製作選項
        var readFilePathOption = new Option<FileInfo?>(
            name: "--file",
            description: "The file to read and display on the console."){ IsRequired = true};

        readFileCommand.AddOption(readFilePathOption);
        readFileCommand.SetHandler((file) =>
        {
            ReadFile(file!);
        }, readFilePathOption);

        rootCommand.AddCommand(readFileCommand);

        return await rootCommand.InvokeAsync(args);
    }

    static void CopyDir(string orgDir, string newDir, string[]? ignorDirList)
    {
        if (string.IsNullOrEmpty(orgDir))
        {
            Console.WriteLine($"來源檔案目錄不能為空");
            return;
        }

        var dir = new DirectoryInfo(orgDir);
        if (ignorDirList?.Length > 0)
        {
            var res = ignorDirList.Contains(dir.Name);
            if (res)
            {
                Console.WriteLine($"忽略路徑: {dir.FullName}");
                return;
            }
        }

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"找不到目錄路徑: {dir.FullName}");

        Console.WriteLine($"來源路徑: {orgDir}");

        if (!string.IsNullOrEmpty(newDir))
        {
            var newPath = Path.Combine(@"D:\Backup", DateTime.Now.ToString("yyyyMMdd"), newDir);
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(newPath, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            if (dir.GetDirectories().Length > 0)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    string newDestinationDir = Path.Combine(newPath, subDir.Name);
                    CopyDir(subDir.FullName, newDestinationDir, ignorDirList);
                }
            }
        }
    }

    static void ReadFile(FileInfo file)
    {
        File.ReadLines(file.FullName).ToList()
            .ForEach(line => Console.WriteLine(line));
    }
}