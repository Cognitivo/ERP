﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace entity.Reporting
{
    public class InventoryAnalysis
    {
        public DateTime InventoryDate { get; set; }
        public string UserName { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal SystemQuantity { get; set; }
        public decimal CountedQuantity { get; set; }
        public decimal Difference { get; set; }
        public decimal ItemCost { get; set; }
        public decimal TotalCost { get; set; }
        public string Tag { get; set; }
        public string Comment { get; set; }
        public string Branch { get; set; }
        public string Location { get; set; }
    }
}