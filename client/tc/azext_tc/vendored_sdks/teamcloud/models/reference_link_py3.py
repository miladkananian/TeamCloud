# coding=utf-8
# --------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
#
# Code generated by Microsoft (R) AutoRest Code Generator.
# Changes may cause incorrect behavior and will be lost if the code is
# regenerated.
# --------------------------------------------------------------------------

from msrest.serialization import Model


class ReferenceLink(Model):
    """ReferenceLink.

    Variables are only populated by the server, and will be ignored when
    sending a request.

    :param href:
    :type href: str
    :ivar templated:
    :vartype templated: bool
    """

    _validation = {
        'templated': {'readonly': True},
    }

    _attribute_map = {
        'href': {'key': 'href', 'type': 'str'},
        'templated': {'key': 'templated', 'type': 'bool'},
    }

    def __init__(self, *, href: str=None, **kwargs) -> None:
        super(ReferenceLink, self).__init__(**kwargs)
        self.href = href
        self.templated = None
