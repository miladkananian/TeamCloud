// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { IIdentifiable, ITags, IProperties, AzureResourceGroup, ProjectType, User } from './index'

export interface Project extends IIdentifiable, ITags, IProperties {
    name: string;
    type: ProjectType;
    resourceGroup: AzureResourceGroup;
    users: User[];
}
