using System;
using System.ComponentModel.DataAnnotations;

namespace Groove.CSFE.Core
{
    public enum OrganizationStatus
    {
        [Display(Name = "label.inactive", Description = "Inactive")]
        Inactive = 0,
        [Display(Name = "label.active", Description = "Active")]
        Active = 1,
        [Display(Name = "label.pending", Description = "Pending")]
        Pending = 2
    }

    [Flags]
    public enum OrganizationType
    {
        [Display(Name = "label.general", Description = "General")]
        General = 1,
        [Display(Name = "label.agent", Description = "Agent")]
        Agent = 2,
        [Display(Name = "label.principal", Description = "Principal")]
        Principal = 4
    }

    public enum AgentType
    {
        [Display(Name = "label.none")]
        None = 1,
        [Display(Name = "label.import")]
        Import = 2,
        [Display(Name = "label.export")]
        Export = 3,
        [Display(Name = "label.both")]
        Both = 4
    }

    public enum ConnectionType
    {
        [Display(Name = "label.active")]
        Active = 1,
        [Display(Name = "label.pending")]
        Pending = 2,
        [Display(Name = "label.inactive")]
        Inactive = 3
    }

    public enum IntegrationStatus
    {
        [Display(Name = "label.succeed")]
        Succeed = 1,
        [Display(Name = "label.failed")]
        Failed = 2
    }

    public enum CurrencyStatus
    {
        Inactive = 0,
        Active = 1
    }

    public enum CarrierStatus
    {
        Inactive = 0,
        Active = 1
    }

    public enum FieldDeserializationStatus
    {
        WasNotPresent,
        HasValue
    }

    public enum VesselStatus
    {
        Inactive = 0,
        Active = 1
    }

    public enum EventCodeStatus
    {
        Inactive = 0,
        Active = 1
    }

    public enum Role
    {
        [Display(Description = "System Admin")]
        SystemAdmin = 1,
        [Display(Description = "CSR")]
        CSR = 2,
        [Display(Description = "Sale")]
        Sale = 3,
        [Display(Description = "Agent")]
        Agent = 4,
        [Display(Description = "Registered User")]
        RegisteredUser = 5,
        [Display(Description = "Guest")]
        Guest = 6,
        [Display(Description = "Pending")]
        Pending = 7,
        [Display(Description = "Principal")]
        Principal = 8,
        [Display(Description = "Shipper")]
        Shipper = 9,
        [Display(Description = "Cruise Agent")]
        CruiseAgent = 10,
        [Display(Description = "Cruise Principal")]
        CruisePrincipal = 11,
        [Display(Description = "Warehouse")]
        Warehouse = 12
    }

    public enum SOFormGenerationFileType
    {
        Pdf = 10,
        Excel = 20
    }

    public enum EventOrderType
    {
        Before = 0,
        After = 1,
    }
}