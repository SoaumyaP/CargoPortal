using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum QuantityUnit
    {
        /// <summary>
        /// Carton
        /// </summary>
        CT = 10,

        /// <summary>
        /// Pallet
        /// </summary>
        PL = 20,

        /// <summary>
        /// Bag
        /// </summary>
        BG = 30,

        /// <summary>
        /// Box
        /// </summary>
        BX = 40,

        /// <summary>
        /// Piece
        /// </summary>
        PC = 50,

        /// <summary>
        /// Roll
        /// </summary>
        RO = 60,

        /// <summary>
        /// Tube
        /// </summary>
        TB = 70,

        /// <summary>
        /// Package
        /// </summary>
        PK = 80,

        /// <summary>
        /// Bundle
        /// </summary>
        BE = 90,

        /// <summary>
        /// Set
        /// </summary>
        ST = 100,

        /// <summary>
        /// Can
        /// </summary>
        CA = 110,

        /// <summary>
        /// Case
        /// </summary>
        CS = 120,

        /// <summary>
        /// Crate
        /// </summary>
        CR = 130,

        /// <summary>
        /// Cylinder
        /// </summary>
        CY = 140,

        /// <summary>
        /// Drum 
        /// </summary>
        DR = 150,

        /// <summary>
        /// Pipe
        /// </summary>
        PI = 160
    }
}
