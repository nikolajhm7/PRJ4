namespace Client.Libary.DTO
{
    public class ConnectedUserDTO(string username, string connectionId)
    {
        public string Username { get; set; } = username;
        public string ConnectionId { get; set; } = connectionId;

        public override bool Equals(object? obj)
        {
            return obj is ConnectedUserDTO user && ConnectionId == user.ConnectionId;
        }

        public override int GetHashCode()
        {
            return ConnectionId.GetHashCode();
        }
    }
}
