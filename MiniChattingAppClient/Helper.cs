using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniChattingAppClient
{
    internal static class Helper
    {
        internal static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            return Regex.IsMatch(email, pattern);
        }
        internal static bool IsValidJson(string strInput)
        {
            // Source - https://stackoverflow.com/a/14977915
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal static void ShowWarningMessage(this string txt)
        {
            Console.WriteLine(txt,Console.ForegroundColor = ConsoleColor.DarkYellow);
            Console.ResetColor();
        }
        internal static void ShowGreenText(this string txt)
        {
            Console.WriteLine(txt,Console.ForegroundColor = ConsoleColor.Green);
            Console.ResetColor();
        }
        internal static void ShowRedText(this string txt)
        {
            Console.WriteLine(txt,Console.ForegroundColor = ConsoleColor.Red);
            Console.ResetColor();
        }
        internal static void ShowMsgSender(this string txt)
        {
            var consoleSize = Console.WindowWidth;
            var txtLen = txt.Length;
            var txt2 = txt.PadLeft(consoleSize - txtLen);
            Console.WriteLine(txt2);
        }
    }
}
