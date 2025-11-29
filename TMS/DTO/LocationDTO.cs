using System;

namespace TMS.DTO
{
    public class LocationDTO
    {
        public int Id { get; set; }          // INT IDENTITY
        public string Name { get; set; }     // City/Location name

        public override string ToString() => Name; // For debugging or binding
    }
}
