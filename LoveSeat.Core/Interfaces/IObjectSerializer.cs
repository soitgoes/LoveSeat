namespace LoveSeat.Core.Interfaces
{
    public interface IObjectSerializer
    {
        T Deserialize<T>(string json) where T : class;
        string Serialize<T>(T obj) where T : class;
        Document<T> DeserializeToDoc<T>(string json) where T : class;
    }
}
