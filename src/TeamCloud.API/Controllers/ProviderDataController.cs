/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/providers/{providerId:providerId}/data")]
    [Produces("application/json")]
    public class ProviderDataController : ApiController
    {
        readonly Orchestrator orchestrator;
        readonly IProviderRepository providersRepository;
        readonly IProviderDataRepository providerDataRepository;

        public ProviderDataController(Orchestrator orchestrator, IProviderRepository providersRepository, IProviderDataRepository providerDataRepository)
        {
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
            this.providerDataRepository = providerDataRepository ?? throw new ArgumentNullException(nameof(providerDataRepository));
        }

        public string ProviderId
            => RouteData.Values.GetValueOrDefault(nameof(ProviderId), StringComparison.OrdinalIgnoreCase)?.ToString();


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProviderDataRead)]
        [SwaggerOperation(OperationId = "GetProviderData", Summary = "Gets all ProviderData for a Provider.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all ProviderData", typeof(DataResult<List<ProviderData>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided providerId was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var provider = await providersRepository
                .GetAsync(ProviderId)
                .ConfigureAwait(false);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found in this TeamCloud Instance")
                    .ToActionResult();

            var data = await providerDataRepository
                .ListAsync(provider.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            var returnData = data.Select(d => d.PopulateExternalModel()).ToList();

            return DataResult<List<ProviderData>>
                .Ok(returnData)
                .ToActionResult();
        }


        [HttpGet("{providerDataId:guid}")]
        [Authorize(Policy = AuthPolicies.ProviderDataRead)]
        [SwaggerOperation(OperationId = "GetProviderDataById", Summary = "Gets the ProviderData by ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a ProviderData", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ProviderData with the provided ID was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get(string providerDataId)
        {
            var provider = await providersRepository
                .GetAsync(ProviderId)
                .ConfigureAwait(false);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found in this TeamCloud Instance")
                    .ToActionResult();

            var providerData = await providerDataRepository
                .GetAsync(providerDataId)
                .ConfigureAwait(false);

            if (providerData is null)
                return ErrorResult
                    .NotFound($"A Provider Data item with the ID '{providerDataId}' could not be found")
                    .ToActionResult();


            var returnData = providerData.PopulateExternalModel();

            return DataResult<ProviderData>
                .Ok(returnData)
                .ToActionResult();
        }


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProviderDataWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProviderData", Summary = "Creates a new ProviderData item")]
        [SwaggerResponse(StatusCodes.Status201Created, "The new ProviderData was created.", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] ProviderData providerData)
        {
            if (providerData is null)
                throw new ArgumentNullException(nameof(providerData));

            var validation = new ProviderDataValidator().Validate(providerData);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            var provider = await providersRepository
                .GetAsync(ProviderId)
                .ConfigureAwait(false);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found in this TeamCloud Instance")
                    .ToActionResult();

            var newProviderData = new ProviderDataDocument
            {
                ProviderId = provider.Id,
                Scope = ProviderDataScope.System

            }.PopulateFromExternalModel(providerData);

            var addResult = await orchestrator
                .AddAsync(newProviderData)
                .ConfigureAwait(false);

            var baseUrl = HttpContext.GetApplicationBaseUrl();
            var location = new Uri(baseUrl, $"api/providers/{provider.Id}/data/{addResult.Id}").ToString();

            var returnAddResult = addResult.PopulateExternalModel();

            return DataResult<ProviderData>
                .Created(returnAddResult, location)
                .ToActionResult();
        }


        [HttpPut("{providerDataId:guid}")]
        [Authorize(Policy = AuthPolicies.ProviderDataWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProviderData", Summary = "Updates an existing ProviderData.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The ProviderData was updated.", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromRoute] string providerDataId, [FromBody] ProviderData providerData)
        {
            if (providerData is null)
                throw new ArgumentNullException(nameof(providerData));

            if (string.IsNullOrWhiteSpace(providerDataId))
                return ErrorResult
                    .BadRequest($"The identifier '{providerDataId}' provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var validation = new ProviderDataValidator().Validate(providerData);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (!providerDataId.Equals(providerData.Id, StringComparison.OrdinalIgnoreCase))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"ProviderData's id does match the identifier provided in the path." })
                    .ToActionResult();

            var provider = await providersRepository
                .GetAsync(ProviderId)
                .ConfigureAwait(false);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found in this TeamCloud Instance")
                    .ToActionResult();

            var oldProviderData = await providerDataRepository
                .GetAsync(providerData.Id)
                .ConfigureAwait(false);

            if (oldProviderData is null)
                return ErrorResult
                    .NotFound($"The Provider Data '{providerData.Id}' could not be found..")
                    .ToActionResult();

            var newProviderData = new ProviderDataDocument
            {
                ProviderId = provider.Id,
                Scope = ProviderDataScope.System

            }.PopulateFromExternalModel(providerData);

            var updateResult = await orchestrator
                .UpdateAsync(newProviderData)
                .ConfigureAwait(false);

            var returnUpdateResult = updateResult.PopulateExternalModel();

            return DataResult<ProviderData>
                .Ok(returnUpdateResult)
                .ToActionResult();
        }


        [HttpDelete("{providerDataId:guid}")]
        [Authorize(Policy = AuthPolicies.ProviderDataWrite)]
        [SwaggerOperation(OperationId = "DeleteProviderData", Summary = "Deletes a ProviderData.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The ProviderData was deleted.", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ProviderData with the providerDataId provided was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete([FromRoute] string providerDataId)
        {
            var provider = await providersRepository
                .GetAsync(ProviderId)
                .ConfigureAwait(false);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found in this TeamCloud Instance")
                    .ToActionResult();

            var existingProviderData = await providerDataRepository
                .GetAsync(providerDataId)
                .ConfigureAwait(false);

            if (existingProviderData is null)
                return ErrorResult
                    .NotFound($"A Provider Data item with the ID '{providerDataId}' could not be found")
                    .ToActionResult();

            _ = await orchestrator
                .DeleteAsync(existingProviderData)
                .ConfigureAwait(false);

            return DataResult<ProviderData>
                .NoContent()
                .ToActionResult();
        }
    }
}
