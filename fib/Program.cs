
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

var bundleCommand = new Command("bundle", "Bundle code files to a single file");
var FileNameAndPathOutput = new Option<FileInfo>("--output", "File path and name");
var FileNameAndPathLanguage = new Option<string>("--language", "Programming language to include use all for all languages");
var IncludeSourceOption = new Option<bool>("--note", "Include source code reference as comment in the bundle");
var SortOption = new Option<string>("--sort", "Sort code files alphabetically by 'name' or 'type'. Default is 'name'");
var RemoveEmptyLinesOption = new Option<bool>("--remove-empty-lines", "Remove empty lines from code files before bundling.");
var AuthorOption = new Option<string>("--author", "Author name to be included in the bundle file header");

FileNameAndPathOutput.AddAlias("-o");
FileNameAndPathLanguage.AddAlias("-l");
IncludeSourceOption.AddAlias("-n");
SortOption.AddAlias("-s");
RemoveEmptyLinesOption.AddAlias("-r");
AuthorOption.AddAlias("-a");

bundleCommand.AddOption(FileNameAndPathOutput);
bundleCommand.AddOption(FileNameAndPathLanguage);
bundleCommand.AddOption(IncludeSourceOption);
bundleCommand.AddOption(SortOption);
bundleCommand.AddOption(RemoveEmptyLinesOption);
bundleCommand.AddOption(AuthorOption);

var createRspCommand = new Command("create-rsp", "Generate a response file with the current command and options.");

createRspCommand.SetHandler(() =>
{
    Console.Write("Enter value for language: ");
    var languageValue = Console.ReadLine();
    Console.Write("Enter value for output: ");
    var outputValue = Console.ReadLine();
    Console.Write("Enter value for note (true/false): ");
    var noteValue = Console.ReadLine();
    while (!(noteValue == "true" || noteValue == "false"))
    {
        Console.Write("Enter again value for note (true/false): ");
        noteValue = Console.ReadLine();
    }
    bool.TryParse(noteValue, out bool note);
    Console.Write("Enter value for sort: ");
    var sortValue = Console.ReadLine();
    Console.Write("Enter value for remove-empty-lines (true/false): ");
    var removeEmptyLinesValue = Console.ReadLine();
    while (!(noteValue == "true" || noteValue == "false"))
    {
        Console.Write("Enter again value for note (true/false): ");
        noteValue = Console.ReadLine();
    }
    bool.TryParse(removeEmptyLinesValue, out bool removeEmptyLines);
    Console.Write("Enter value for author: ");
    var authorValue = Console.ReadLine();
    string rspContent = $"--language {languageValue}" +
                        $" --output {outputValue}" +
                        $" --note {note}" +
                        $" --sort {sortValue}" +
                        $" --remove-empty-lines {removeEmptyLines}" +
                        $" --author {authorValue}";
    File.WriteAllText("response.rsp", rspContent);
    Console.WriteLine("Response file 'response.rsp' created successfully.");
});

bundleCommand.SetHandler((output, language, note, sort, removeEmptyLines, author) =>
    {
        string[] files;
        if (output.Exists)
        {
            Console.WriteLine("Output file already exist. Please choose a differente name.");
            return;
        }
        if (language.ToLower() == "all")
        {
            files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories);
            files = files.Where(f => !f.Contains("bin") && !f.Contains("debug")).ToArray();
        }
        else
        {
            files = Directory.GetFiles(Directory.GetCurrentDirectory(), $"*.{ConvertLanguage(language)}", SearchOption.AllDirectories).ToArray();

        }
        if (files.Length == 0)
        {
            Console.WriteLine("No files to bundle");
            return;
        }
        try
        {
            using (var outputFile = System.IO.File.CreateText(output.FullName))
            {
                if (note)
                {
                    outputFile.WriteLine($"// Source code reference: {Environment.NewLine}// {GetSourceCodeReference()}{Environment.NewLine}");
                    Console.WriteLine("Source code reference added to the bundle");
                }
                if (!string.IsNullOrWhiteSpace(author))
                {
                    outputFile.WriteLine($"// Author: {author}{Environment.NewLine}");
                }
                foreach (var file in files)
                {
                    string fileContent = File.ReadAllText(file);
                    if (removeEmptyLines)
                    {
                        fileContent = RemoveEmptyLines(fileContent);
                    }
                    outputFile.WriteLine($"// File: {file}");
                    outputFile.WriteLine(fileContent);
                    outputFile.WriteLine();
                }
            }
            Console.WriteLine($"Files bundled successfully: {output.FullName}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine("file path invalid");
        }
        if (files.Length > 1)
        {
            if (sort?.ToLower() == "alphabetical")
            {
                files = files.OrderBy(f => Path.GetExtension(f)).ThenBy(f => Path.GetFileName(f)).ToArray();
            }
            else
            {
                files = files.OrderBy(f => Path.GetFileName(f)).ToArray();
            }
        }
    }, FileNameAndPathOutput, FileNameAndPathLanguage, IncludeSourceOption, SortOption, RemoveEmptyLinesOption, AuthorOption);

var rootCommand = new RootCommand("root command for file bundle CLI");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);
rootCommand.InvokeAsync(args);

string GetSourceCodeReference()
{
    var sourceFilePath = typeof(Program).Assembly.Location;
    return $"File: {sourceFilePath}";
}

string RemoveEmptyLines(string content)
{
    return string.Join(Environment.NewLine, content.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToArray());
}

string ConvertLanguage(string language)
{
    switch (language)
    {
        case "csharp":
            return "cs";
            break;
        case "cpp":
            return "cpp";
            break;
        case "html":
            return "html";
            break;
        case "asembler":
            return "asn";
            break;
        case "sql":
            return "sql";
            break;
        case "css":
            return "css";
            break;
        case "javascript":
            return "js";
            break;
        default:
            return language;
    }
}




















