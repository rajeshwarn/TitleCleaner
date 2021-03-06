﻿using System;
using System.Globalization;
using System.IO;
using MediaFileParser.MediaTypes.TvFile;
using MediaFileParser.MediaTypes.TvFile.Tvdb;
using MediaFileParser.ModeManagers;
using MediaFileParser.MediaTypes.MediaFile;

namespace TitleCleanerConsole
{
    public static class NormalMode
    {
        private static bool _conf;
        private static StreamWriter _logWriter;

        public static void Run(bool confirm, string inputDir, string outputDir, bool copyOnly, bool debug)
        {
            if (debug)
            {
                _logWriter = new StreamWriter(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "TitleCleaner", "clean.log"), true);
            }

            _conf = confirm;
            var consoleColor = Console.ForegroundColor;

            var fileManager = new FileManager(_conf);
            fileManager.ConfirmAutomaticMove += FileManagerConfirmAutomaticMove;
            fileManager.OnFileMove += fileManager_OnFileMove;
            fileManager.OnFileMoveFailed += fileManager_OnFileMoveFailed;
            fileManager.OnAccessDenied += fileManager_OnAccessDenied;

            TvFile.TvdbSearchSelectionRequired += TvFileTvdbSearchSelectionRequired;
            
            var mediaFiles = fileManager.GetMediaFileList(inputDir);
            fileManager.MoveFiles(mediaFiles, outputDir, copyOnly);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("----\nComplete");

            if (confirm)
            {
                Console.ReadKey(true);
            }

            Console.ForegroundColor = consoleColor;
        }

        private static void fileManager_OnAccessDenied(FileManager sender, UnauthorizedAccessException e)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write('[');
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Erro");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("]\t");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
        }

        private static uint TvFileTvdbSearchSelectionRequired(TvdbSeries[] seriesSearch, string seriesName)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("----\nWhich TV Series does the series \"" + seriesName + "\" refer to?");
            int num = 0;
            foreach (var series in seriesSearch)
            {
                num++;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("(" + num + "): " + series.SeriesName);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(series.Description + "\n");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[Number/Enter/Ctrl-C]: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            int result;
            string temp;
            while (!int.TryParse((temp = Console.ReadLine()), out result) && temp != "" || result < 0 || result > num)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Invalid Input. ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[Number/Enter/Ctrl-C]: ");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Console.WriteLine();
            return result == 0 ? 0 : seriesSearch[result-1].TvdbId;
        }

        private static bool GetBool(string b, out bool res)
        {
            switch (b.ToLower().Trim())
            {
                case "true":
                case "yes":
                case "y":
                {
                    res = true;
                    return true;
                }
                case "false":
                case "no":
                case "n":
                {
                    res = false;
                    return true;
                }
                default:
                {
                    res = false;
                    return false;
                }
            }
        }

        private static void fileManager_OnFileMoveFailed(MediaFile file, string destination)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write('[');
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Fail");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("]\t");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(file);
        }

        private static void fileManager_OnFileMove(MediaFile file, string destination)
        {
            if (_logWriter != null)
            {
                _logWriter.WriteLine(file.Origional.Replace(",", "") + "," + file.Cleaned.Replace(",", ""));
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write('[');
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Done");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("]\t");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(file);
        }

        private static bool FileManagerConfirmAutomaticMove(MediaFile file, string destination)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Move\t");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(file.ToString("O.E"));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("To\t");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(file.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[Yes/No/Ctrl-C]: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            bool result;
            while (!GetBool(Console.ReadKey().KeyChar.ToString(CultureInfo.InvariantCulture), out result))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Invalid Input. ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[Yes/No/Ctrl-C]: ");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Console.WriteLine();
            Console.WriteLine();
            return result;
        }
    }
}
