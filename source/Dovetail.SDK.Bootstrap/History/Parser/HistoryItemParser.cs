using System;
using System.Collections.Generic;
using FubuCore;
using Sprache;

namespace Dovetail.SDK.Bootstrap.History.Parser
{
	public interface IHistoryItemParser
	{
		EmailLog ParseEmailLog(string input);
		IEnumerable<Content> ParseContent(string input);
	}

	public class HistoryItemParser : IHistoryItemParser
	{
		private readonly HistoryParsers _historyParser;
		private readonly ILogger _logger;

		public HistoryItemParser(HistoryParsers historyParser, ILogger logger)
		{
			_historyParser = historyParser;
			_logger = logger;
		}

		public IEnumerable<Content> ParseContent(string input)
		{
			try
			{
				return (IEnumerable<Content>) _historyParser.Content.Many().End().Parse(input);
			}
			catch (Exception e)
			{
				_logger.LogError("Could not parse content. Contents:\n\n{0}".ToFormat(input), e);
				return fakeContent(input);
			}
		}

		public EmailLog ParseEmailLog(string input)
		{
			try
			{
				return _historyParser.LogEmail.Parse(input);
			}
			catch (Exception e)
			{
				_logger.LogError("Could not parse email log. Contents:\n\n{0}".ToFormat(input), e);
				return fakeEmailLog(input);
			}
		}

		private static IEnumerable<Content> fakeContent(string input)
		{
			return new[] {new Content {Text = input}};
		}

		private static EmailLog fakeEmailLog(string input)
		{
			return new EmailLog
			{
				Header = new EmailHeader { Headers = new EmailHeaderItem[0] },
				Items = new[] {new Content {Text = input}}
			};
		}
	}
}