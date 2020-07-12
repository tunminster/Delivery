using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using Microsoft.AspNetCore.Http;

namespace Delivery.Api.Models.Dto
{
    [DataContract]
    public class ProductDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string ProductImage { get; set; }
        public string ProductImageUrl { get; set; }
        public string UnitPrice { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        
    }
}
