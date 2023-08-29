using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EquipmentType
    {

        /// <summary>
        /// 20' Dangerous Container 
        /// </summary>
        [EnumMember(Value = "20DG")]
        TwentyDG = 3,

        /// <summary>
        /// 20' Flat Rack 
        /// </summary>
        [EnumMember(Value = "20FR")]
        TwentyFR = 5,

        /// <summary>
        /// 20' GOH Container 
        /// </summary>
        [EnumMember(Value = "20GH")]
        TwentyGH = 7,

        /// <summary>
        /// 20' CONTAINER 
        /// </summary>
        [EnumMember(Value = "20GP")]
        TwentyGP = 10,

        /// <summary>
        /// 20' High Cube
        /// </summary>
        [EnumMember(Value = "20HC")]
        TwentyHC = 11,

        /// <summary>
        /// 20' HT Container
        /// </summary>
        [EnumMember(Value = "20HT")]
        TwentyHT = 12,

        /// <summary>
        /// 20' High Wide
        /// </summary>
        [EnumMember(Value = "20HW")]
        TwentyHW = 13,

        /// <summary>
        /// 20' Reefer Dry
        /// </summary>
        [EnumMember(Value = "20NOR")]
        TwentyNOR = 14,

        /// <summary>
        /// 20' Both Full Side Door Opening Container
        /// </summary>
        [EnumMember(Value = "20OS")]
        TwentyOS = 15,

        /// <summary>
        /// 20' Open Top Container
        /// </summary>
        [EnumMember(Value = "20OT")]
        TwentyOT = 16,

        /// <summary>
        /// 40' CONTAINER 
        /// </summary>
        [EnumMember(Value = "40GP")]
        FourtyGP = 20,

        /// <summary>
        /// 40' High Cube
        /// </summary>
        [EnumMember(Value = "40HC")]
        FourtyHC = 21,

        /// <summary>
        /// 40' HC GOH Container
        /// </summary>
        [EnumMember(Value = "40HG")]
        FourtyHG = 22,

        /// <summary>
        /// 40' HC Reefer Dry Container
        /// </summary>
        [EnumMember(Value = "40HNOR")]
        FourtyHNOR = 23,

        /// <summary>
        /// 40' HC Open Top Container
        /// </summary>
        [EnumMember(Value = "40HO")]
        FourtyHO = 24,

        /// <summary>
        /// 40' HQ DG Container
        /// </summary>
        [EnumMember(Value = "40HQDG")]
        FourtyHQDG = 25,

        /// <summary>
        /// 40' HC Reefer Container
        /// </summary>
        [EnumMember(Value = "40HR")]
        FourtyHR = 26,

        /// <summary>
        /// 40' High Cube Pallet Wide
        /// </summary>
        [EnumMember(Value = "40HW")]
        FourtyHW = 27,

        /// <summary>
        /// 40' Reefer Dry
        /// </summary>
        [EnumMember(Value = "40NOR")]
        FourtyNOR = 28,

        /// <summary>
        /// 40' Open Top Container
        /// </summary>
        [EnumMember(Value = "40OT")]
        FourtyOT = 29,

        /// <summary>
        /// 20' Reefer 
        /// </summary>
        [EnumMember(Value = "20RF")]
        TwentyRF = 30,

        /// <summary>
        /// 20' Tank Container 
        /// </summary>
        [EnumMember(Value = "20TK")]
        TwentyTK = 31,

        /// <summary>
        /// 20' Ventilated Container 
        /// </summary>
        [EnumMember(Value = "20VH")]
        TwentyVH = 32,

        /// <summary>
        /// 40' Dangerous Conatiner 
        /// </summary>
        [EnumMember(Value = "40DG")]
        FourtyDG = 33,

        /// <summary>
        /// 40' High Cube Flat Rack 
        /// </summary>
        [EnumMember(Value = "40FQ")]
        FourtyFQ = 34,

        /// <summary>
        /// 40' Flat Rack 
        /// </summary>
        [EnumMember(Value = "40FR")]
        FourtyFR = 35,

        /// <summary>
        /// 40' GOH Container 
        /// </summary>
        [EnumMember(Value = "40GH")]
        FourtyGH = 36,

        /// <summary>
        /// 40' Plus
        /// </summary>
        [EnumMember(Value = "40PS")]
        FourtyPS = 37,

        /// <summary>
        /// 40' Reefer
        /// </summary>
        [EnumMember(Value = "40RF")]
        FourtyRF = 40,

        /// <summary>
        /// 40' Tank
        /// </summary>
        [EnumMember(Value = "40TK")]
        FourtyTK = 41,

        /// <summary>
        /// 45' GOH
        /// </summary>
        [EnumMember(Value = "45GO")]
        FourtyFiveGO = 51,

        /// <summary>
        /// 45' High Cube
        /// </summary>
        [EnumMember(Value = "45HC")]
        FourtyFiveHC = 52,

        /// <summary>
        /// 45' HC GOH Container
        /// </summary>
        [EnumMember(Value = "45HG")]
        FourtyFiveHG = 54,

        /// <summary>
        /// 45' Hard Top Container
        /// </summary>
        [EnumMember(Value = "45HT")]
        FourtyFiveHT = 55,

        /// <summary>
        /// 45 HC Pallet Wide
        /// </summary>
        [EnumMember(Value = "45HW")]
        FourtyFiveHW = 56,

        /// <summary>
        /// 45' Reefer Container
        /// </summary>
        [EnumMember(Value = "45RF")]
        FourtyFiveRF = 57,

        /// <summary>
        /// 48' HC Container
        /// </summary>
        [EnumMember(Value = "48HC")]
        FourtyEightHC = 58,

        /// <summary>
        /// LCL Shipment
        /// </summary>
        LCL = 60,

        /// <summary>
        /// Truck
        /// </summary>
        [EnumMember(Value = "TRUCK")]
        TRUCK = 70,

        /// <summary>
        /// Air Shipment
        /// </summary>
        [EnumMember(Value = "AIR")]
        AIR = 50,
    }
}
