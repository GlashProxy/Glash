﻿using System.Text.Json.Serialization;

namespace Glash.Client.Protocol.QpNotices;

[JsonSerializable(typeof(AgentLoginStatusChanged))]
internal partial class ClientNoticesSerializerContext : JsonSerializerContext { }