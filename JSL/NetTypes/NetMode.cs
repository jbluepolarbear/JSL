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