/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IUsersRepository
    {
        Task<User> GetAsync(Guid id);

        IAsyncEnumerable<User> ListAsync();

        IAsyncEnumerable<User> ListAsync(Guid projectId);

        IAsyncEnumerable<User> ListOwnersAsync(Guid projectId);

        IAsyncEnumerable<User> ListAdminsAsync();

        Task<User> AddAsync(User user);

        Task<User> SetAsync(User user);

        Task<User> RemoveAsync(User user);

        Task RemoveProjectMembershipsAsync(Guid projectId);

        Task<User> RemoveProjectMembershipAsync(User user, Guid projectId);

        Task<User> AddProjectMembershipAsync(User user, ProjectMembership membership);

        Task<User> AddProjectMembershipAsync(User user, Guid projectId, ProjectUserRole role, IDictionary<string, string> properties);

        Task<User> SetTeamCloudInfoAsync(User user);
    }
}