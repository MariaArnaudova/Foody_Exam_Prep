using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace FoodyApiTests.Models
{
    public class ApiResponseDTO
    {

        [JsonPropertyName("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("foodId")]
        public string? FoodId { get; set; }

    }
}
