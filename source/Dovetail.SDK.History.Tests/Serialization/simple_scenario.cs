﻿using System.Diagnostics;
using System.Linq;
using Dovetail.SDK.History.Instructions;
using Dovetail.SDK.ModelMap.Instructions;
using NUnit.Framework;

namespace Dovetail.SDK.History.Tests.Serialization
{
	[TestFixture]
	public class simple_scenario
	{
		private HistoryMapParsingScenario theScenario;

		[SetUp]
		public void SetUp()
		{
			theScenario = HistoryMapParsingScenario.Create(_ =>
			{
				_.UseFile("simple.history.config");
			});
		}

		[Test]
		public void verify_instructions()
		{
			VerifyInstructions.Assert(theScenario.Instructions, _ =>
			{
				_.Verify<BeginModelMap>(__ => __.Name.ShouldEqual(WorkflowObject.KeyFor("case")));

				_.SkipDefaults();

				_.Verify<BeginActEntry>(__ =>
				{
					__.Code.ShouldEqual(3400);
					__.IsVerbose.ShouldBeFalse();
				});

				_.Get<BeginProperty>().Key.ShouldEqual("note");
				_.Is<EndProperty>();

				_.Get<BeginRelation>().RelationName.ShouldEqual("act_entry2email_log");
				_.Get<BeginProperty>().Key.ShouldEqual("sender");
				_.Is<EndProperty>();
				_.Get<BeginProperty>().Key.ShouldEqual("recipient");
				_.Is<EndProperty>();
				_.Get<BeginProperty>().Key.ShouldEqual("body");
				_.Is<EndProperty>();
				_.Is<EndRelation>();

				_.Is<EndActEntry>();

				_.Verify<BeginActEntry>(__ =>
				{
					__.Code.ShouldEqual(8900);
					__.IsVerbose.ShouldBeTrue();
				});

				_.Verify<BeginWhen>(__ => __.IsChild.ShouldBeFalse());
				_.Get<BeginRelation>().RelationName.ShouldEqual("act_entry2doc_inst");
				_.Get<BeginProperty>().Key.ShouldEqual("id");
				_.Is<EndProperty>();
				_.Get<BeginProperty>().Key.ShouldEqual("title");
				_.Is<EndProperty>();
				_.Is<EndRelation>();
				_.Is<EndWhen>();

				_.Verify<BeginWhen>(__ => __.IsChild.ShouldBeTrue());
				_.Get<BeginProperty>().Key.ShouldEqual("childProperty");
				_.Is<EndProperty>();
				_.Is<EndWhen>();

				_.Is<EndActEntry>();

				_.Is<EndModelMap>();
			});
			//theScenario.WhatDoIHave();
			//var container = TestContainer.getContainer();
			//var builder = container.With(theScenario.Cache).GetInstance<HistoryBuilder>();
			//var data = builder.GetAll(new HistoryRequest
			//{
			//	ShowAllActivities = false,
			//	WorkflowObject = new WorkflowObject
			//	{
			//		Id = "2",
			//		Type = "case",
			//		IsChild = false,
			//	}
			//});

			//Debug.WriteLine(data.Select(_ => _.ToValues()).ToJSON());
		}

		[TearDown]
		public void TearDown()
		{
			theScenario.CleanUp();
		}
	}
}