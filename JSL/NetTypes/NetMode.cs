// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace JSL.NetTypes
{
    public enum NetMode
    {
        None,
        Local,
        Server,
        Host, // This would be through a lan or relay service
        Client,
        HostRelay, // This would be through a lan or relay service
        ClientRelay
    }
}