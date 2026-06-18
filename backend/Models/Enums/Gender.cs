using System.Text.Json.Serialization;

namespace backend.Model;

public enum Gender
{
    [JsonStringEnumMemberName("Hombre")]      Male,
    [JsonStringEnumMemberName("Mujer")]       Female,
    [JsonStringEnumMemberName("NoEspecifica")] Unspecified
}
