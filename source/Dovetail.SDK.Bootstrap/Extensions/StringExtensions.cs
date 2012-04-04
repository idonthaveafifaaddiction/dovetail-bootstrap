﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Extensions
{
	public static class StringExtensions
	{
        private static readonly Regex findLineBreaks = new Regex(@"\s*(\n\r|\n)\s*");
        private static readonly Regex findLine = new Regex(@"^(.*)$", RegexOptions.Multiline);
        private static readonly Regex urlFinderRegEx = new Regex(@"(?<link>(?<protocol>ftp|http|https|mailto|file|webcal):(?:(?:[A-Za-z0-9$_.+!*(),;/?:@&~=-])|%[A-Fa-f0-9]{2}){2,}(?:#(?:[a-zA-Z0-9][a-zA-Z0-9$_.+!*(),;/?:@&~=%-]*))?(?:[A-Za-z0-9$_+!*();/?:~-]))");
        public static string ToHtml(this String toHtml)
		{
			if (toHtml.IsEmpty()) return String.Empty;

			var result = findLineBreaks.Replace(toHtml, "\n");

            result = findLine.Replace(result, "<p>$1</p>");
            result = urlFinderRegEx.Replace(result, @"<a href=""${link}"">${link}</a>");

			return result;
		}

        private static readonly Random _random = new Random(((int)DateTime.Now.Ticks));

        public static string GetRandomString(int size)
        {
            var builder = new StringBuilder(size + 10);
            for (var i = 0; i < size; i++)
            {
                var character = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65)));
                builder.Append(character);
            }
            return builder.ToString().ToLower();
        }
	}
}