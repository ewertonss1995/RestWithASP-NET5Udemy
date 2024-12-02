using System.Text.Json.Serialization;

namespace RestWithASPNETUdemy.Data.VO
{
    public class PersonVO
    {
        // Para alterar o nome de um atributo no JSON 
        [JsonPropertyName("code")]
        public long Id { get; set; }
        [JsonPropertyName("name")]
        public string FirstName { get; set; }
        [JsonPropertyName("last_name")]
        public string LastName { get; set; }
        [JsonIgnore] // Para ignorar um atributo no JSON 
        public string Address { get; set; }
        [JsonPropertyName("sex")]
        public string Gender { get; set; }
    }
}