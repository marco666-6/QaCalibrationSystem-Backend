using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class Equipment
    {
        public int Id { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string ControlNo { get; set; } = string.Empty;
        public string? SerialNo { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string Location { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public int PicId { get; set; }
        public string PicCode { get; set; } = string.Empty;
        public string PicName { get; set; } = string.Empty;
        public int CalibIntervalMonths { get; set; }
        public DateTime? LastCalibDate { get; set; }

        public int? LastCalibMonth => LastCalibDate?.Month;
        public int? LastCalibYear => LastCalibDate?.Year;
        public DateTime? NextCalibDate => LastCalibDate?.AddMonths(CalibIntervalMonths);
        public int? NextCalibMonth => NextCalibDate?.Month;
        public int? NextCalibYear => NextCalibDate?.Year;

        public string CalibType { get; set; } = "I";
        public string EquipmentStatus { get; set; } = "A";
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
    }
}