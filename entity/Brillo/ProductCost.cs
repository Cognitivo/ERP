﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace entity.Brillo
{
    public class ProductCost
    {
        public void calc_EntireCost()
        {
            using (db db = new db())
            {
                foreach (item_product item_product in db.item_product.Where(x => x.id_company == CurrentSession.Id_Company && x.item.is_active))
                {
                    int itemproductId = item_product.id_item_product;
                    item_movement ItemMovement = db.item_movement.Where(x => x.credit > 0 && x.id_item_product == itemproductId).OrderByDescending(x => x.trans_date).Take(1).FirstOrDefault();
                    if (ItemMovement != null)
                    {
                        item_product.item.unit_cost = ItemMovement.item_movement_value.Sum(x => x.unit_value);
                    }
                }
                db.SaveChanges();

            }

        }
        public void calc_SingleCost( item_product item_product)
        {
            using (db db = new db())
            {
                int itemproductId = item_product.id_item_product;
                item_movement ItemMovement = db.item_movement.Where(x => x.credit > 0 && x.id_item_product == itemproductId).OrderByDescending(x => x.trans_date).Take(1).FirstOrDefault();
                if (ItemMovement != null)
                {
                    item_product.item.unit_cost = ItemMovement.item_movement_value.Sum(x => x.unit_value);
                }
               
            }




        }
    }

}