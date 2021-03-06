/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using FluentValidation;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Model.Validation.Data
{
    public sealed class ProjectDocumentValidator : AbstractValidator<ProjectDocument>
    {
        public ProjectDocumentValidator()
        {
            RuleFor(obj => obj.Name)
                .NotEmpty();

            RuleFor(obj => obj.Type)
                .NotEmpty();

            // RuleFor(obj => obj.Users)
            //     .NotEmpty();

            RuleForEach(obj => obj.Tags).MustBeValidTag();
        }
    }
}
