using System.Text.Json.Serialization;

namespace MusicCatalog.Models.Recenzije
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipZahteva
    {
        IZMENA,
        BRISANJE
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusZahteva
    {
        NA_CEKANJU,
        PRIHVACEN,
        ODBIJEN
    }
}