//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BotManager
{
    using System;
    using System.Collections.Generic;
    
    public partial class Trip
    {
        public long TripId { get; set; }
        public long DriverRouteId { get; set; }
        public System.DateTime TCreateTime { get; set; }
        public System.DateTime TStartTime { get; set; }
        public short TState { get; set; }
        public short TEmptySeat { get; set; }
    }
}