﻿using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Toolkits.Clarify.Support;

namespace Bootstrap.Web.Handlers.api.cases.create
{
    public class post_handler
    {
        private readonly IClarifySession _userSession;

        public post_handler(IClarifySession userSession)
        {
            _userSession = userSession;
        }

        public CreateCaseResult Execute(CreateCaseInputModel model)
        {
            var toolkit = _userSession.CreateSupportToolkit();

            var setup = new CreateCaseSetup(model.SiteId, model.ContactFirstName, model.ContactLastName, model.ContactPhone)
                            {
                                Title = model.Title,
                                Queue = model.Queue,
                                PhoneLogNotes = model.Description,
                                Priority = model.Priority,
                                CaseType = model.CaseType,
                                Severity = model.IssueSeverity
                            };

            var result = toolkit.CreateCase(setup);

            return new CreateCaseResult {Id = result.IDNum};
        }
    }

    public class CreateCaseInputModel : CreateCaseModel {}

    public class CreateCaseResult
    {
        public string Id { get; set; }
    }
}