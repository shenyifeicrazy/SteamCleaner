﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

#endregion

namespace SteamCleaner.Utilities
{
    internal class SteamUtilities
    {
        public static string FixPath(string dir)
        {
            if (!dir.Contains("SteamApps"))
                dir += Path.Combine("\\SteamApps", "common");
            return dir;
        }

        public static string GetSteamPath()
        {
            var regPath = "";
            var steamPath = "";
            var is64Bit = Environment.Is64BitOperatingSystem;
            if (is64Bit)
            {
                Console.WriteLine("64 Bit operating system detected");
                regPath = "SOFTWARE\\Wow6432Node\\Valve\\Steam";
            }
            else
            {
                Console.WriteLine("32 Bit operating system detected");
                regPath = "SOFTWARE\\Valve\\Steam";
            }

            var key = Registry.LocalMachine.OpenSubKey(regPath);
            if (key != null)
            {
                var o = key.GetValue("InstallPath");
                steamPath = o.ToString();
            }
            key?.Close();
            return steamPath;
        }


        public static List<string> SteamPaths()
        {
            var paths = new List<string> {GetSteamPath()};
            paths.AddRange(GetSecondarySteamInstallPaths());
            return paths;
        }

        public static int CountOccurences(string needle, string haystack)
        {
            return (haystack.Length - haystack.Replace(needle, "").Length)/needle.Length;
        }

        public static List<string> GetSecondarySteamInstallPaths()
        {
            var configPath = GetSteamPath() + "\\config\\config.vdf";
            var data = File.ReadAllText(configPath);
            var numberOfInstallPaths = CountOccurences("BaseInstallFolder", data);
            var dataArray = File.ReadAllLines(configPath);
            var paths = new List<string>();
            for (var i = 0; i < numberOfInstallPaths; i++)
            {
                var slot = i + 1;
                paths.AddRange(from t in dataArray
                    where t.Contains("BaseInstallFolder_" + slot)
                    select t.Trim()
                    into dataString
                    let regex = new Regex("\\\"(.*)\\\"(.*)\\\"", RegexOptions.IgnoreCase)
                    select regex.Match(dataString)
                    into match
                    where match.Success
                    select FixPath(match.Groups[2].Value).Replace("\\\\", "\\"));
            }
            return paths;
        }
    }
}