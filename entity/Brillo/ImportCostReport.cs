﻿using entity.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace entity.Brillo
{
    public class ImportCostReport
    {
        public List<Impex_ItemDetail> Impex_ItemDetailLIST = new List<Impex_ItemDetail>();
        public List<CostDetail> CostDetailLIST = new List<CostDetail>();
        private ImpexDB ImpexDB = new ImpexDB();

        public void GetExpensesForAllIncoterm(impex impex)
        {
            Impex_ItemDetailLIST.Clear();
            CostDetailLIST.Clear();
            List<impex_incoterm> impex_incotermList = ImpexDB.impex_incoterm.ToList();
            List<Impex_Products> Impex_ProductsLIST = new List<Impex_Products>();
            purchase_invoice PurchaseInvoice = impex.impex_expense.FirstOrDefault().purchase_invoice;

            foreach (impex_incoterm Incoterm in impex_incotermList)
            {
                List<impex_incoterm_detail> IncotermDetail = ImpexDB.impex_incoterm_detail.Where(i => i.id_incoterm == Incoterm.id_incoterm && i.buyer == true).ToList();
                decimal totalExpense = 0;

                if (PurchaseInvoice != null)
                {
                    decimal GrandTotal = PurchaseInvoice.purchase_invoice_detail.Where(z => z.item != null && z.item.item_product != null).Sum(y => y.SubTotal);
                    foreach (purchase_invoice_detail _purchase_invoice_detail in PurchaseInvoice.purchase_invoice_detail.Where(x => x.item != null && x.item.item_product != null))
                    {
                        foreach (impex_incoterm_detail item in IncotermDetail)
                        {
                            impex_expense _impex_expense = ImpexDB.impex_expense.Where(x => x.id_incoterm_condition == item.id_incoterm_condition && x.id_purchase_invoice == PurchaseInvoice.id_purchase_invoice).FirstOrDefault();
                            if (_impex_expense != null)
                            {
                                CostDetail CostDetail = new Class.CostDetail();
                                if (impex.fx_rate > 0)
                                {
                                    if (impex.Currencyfx != null)
                                    {
                                        if (impex.Currencyfx.is_reverse)
                                        {
                                            CostDetail.Costfx = (decimal)_impex_expense.value / impex.fx_rate;
                                        }
                                        else
                                        {
                                            CostDetail.Costfx = (decimal)_impex_expense.value * impex.fx_rate;
                                        }
                                    }
                                }

                                CostDetail.Cost = (decimal)_impex_expense.value;
                                CostDetail.CostName = _impex_expense.impex_incoterm_condition.name;

                                CostDetailLIST.Add(CostDetail);
                                totalExpense += (decimal)_impex_expense.value;
                            }
                            else
                            {
                                totalExpense += 0;
                            }
                        }

                        Impex_ItemDetail ImpexImportDetails = new Impex_ItemDetail();
                        ImpexImportDetails.number = _purchase_invoice_detail.purchase_invoice.number;
                        ImpexImportDetails.id_item = (int)_purchase_invoice_detail.id_item;
                        ImpexImportDetails.item_code = ImpexDB.items.Where(a => a.id_item == _purchase_invoice_detail.id_item).FirstOrDefault().code;
                        ImpexImportDetails.item = ImpexDB.items.Where(a => a.id_item == _purchase_invoice_detail.id_item).FirstOrDefault().name;
                        ImpexImportDetails.quantity = _purchase_invoice_detail.quantity;
                        ImpexImportDetails.unit_cost = _purchase_invoice_detail.unit_cost;
                        ImpexImportDetails.sub_total = _purchase_invoice_detail.SubTotal;
                        ImpexImportDetails.id_invoice = _purchase_invoice_detail.id_purchase_invoice;
                        ImpexImportDetails.id_invoice_detail = _purchase_invoice_detail.id_purchase_invoice_detail;

                        if (totalExpense > 0)
                        {
                            ImpexImportDetails.unit_Importcost = Math.Round(((_purchase_invoice_detail.SubTotal / GrandTotal) * totalExpense) / _purchase_invoice_detail.quantity, 2);
                            ImpexImportDetails.prorated_cost = _purchase_invoice_detail.unit_cost + ImpexImportDetails.unit_Importcost;
                        }

                        decimal SubTotal = (_purchase_invoice_detail.quantity * ImpexImportDetails.prorated_cost);
                        ImpexImportDetails.sub_total = Math.Round(SubTotal, 2);
                        ImpexImportDetails.incoterm = Incoterm.name;
                        if (impex.fx_rate > 0)
                        {
                            if (impex.Currencyfx != null)
                            {
                                if (impex.Currencyfx.is_reverse)
                                {
                                    ImpexImportDetails.unit_costfx = ImpexImportDetails.unit_costfx / impex.fx_rate;
                                    ImpexImportDetails.sub_totalfx = ImpexImportDetails.sub_totalfx / impex.fx_rate;
                                    ImpexImportDetails.unit_Importcostfx = ImpexImportDetails.unit_Importcostfx / impex.fx_rate;
                                    ImpexImportDetails.prorated_costfx = ImpexImportDetails.prorated_costfx / impex.fx_rate;
                                }
                                else
                                {
                                    ImpexImportDetails.unit_costfx = ImpexImportDetails.unit_costfx * impex.fx_rate;
                                    ImpexImportDetails.sub_totalfx = ImpexImportDetails.sub_totalfx * impex.fx_rate;
                                    ImpexImportDetails.unit_Importcostfx = ImpexImportDetails.unit_Importcostfx * impex.fx_rate;
                                    ImpexImportDetails.prorated_costfx = ImpexImportDetails.prorated_costfx * impex.fx_rate;
                                }
                            }
                        }
                        Impex_ItemDetailLIST.Add(ImpexImportDetails);
                    }
                }
            }
        }
    }
}