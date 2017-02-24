﻿using entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace cntrl.Controls
{
    public partial class InventoryFlowDataGrid : UserControl
    {
        public int? ParentID { get; set; }
        public int ProductID { get; set; }

        public InventoryFlowDataGrid(int? InvParentID, int InvProductID)
        {
            InitializeComponent();

            ParentID = InvParentID;
            ProductID = InvProductID;

            using (db db = new db())
            {
                var MovementList = from item in db.item_movement
                                    join loc in db.app_location on item.id_location equals loc.id_location
                                    join b in db.app_branch on loc.id_branch equals b.id_branch
                                    join ip in db.item_product on item.id_item_product equals ip.id_item_product
                                    join i in db.items on ip.id_item equals i.id_item
                                    where item.parent.id_movement == ParentID && item.id_item_product == ProductID
                                    select new ItemMovement
                                    {
                                        MovementID = item.id_movement,
                                        Branch = b.name,
                                        Location = loc.name,
                                        ProductCode = i.code,
                                        ProductName = i.name,
                                        Date = item.trans_date,
                                        ExpiryDate = item.expire_date,
                                        Batch = item.code,
                                        Quantity = item.credit - item.debit,
                                        Cost = item.item_movement_value.Sum(x => x.unit_value),
                                        Comment = item.comment
                                    };

                CollectionViewSource item_movementViewSource = ((CollectionViewSource)(FindResource("item_movementViewSource")));
                item_movementViewSource.Source = MovementList;
            }
        }
    }

    public class ItemMovement
    {
        public long MovementID { get; set; }
        public DateTime Date { get; set; }
        public string Branch { get; set; }
        public string Location { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public string Batch { get; set; }
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }

        public string Comment { get; set; }
    }
}