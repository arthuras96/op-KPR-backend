using Application.Account.Models;
using Application.General.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Resident.Models
{
    public class PreloadModel
    {
        public List<GenericParamModel> GENDER { get; set; }
        public List<GenericParamModel> TYPERESIDENT { get; set; }
        public List<GenericParamModel> SPONSOR { get; set; }
        public List<UnityModel> UNITY { get; set; }
        public List<VehicleModel> VEHICLE { get; set; }
    }
}
