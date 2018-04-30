﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace entity.API.DebeHaber
{
    public enum InvoiceTypes { Purchase = 1, PurchaseReturn = 3, Sales = 4, SalesReturn = 5 }

    public enum AccountTypes { AccountPayable = 1, AccountReceivable = 2, Sales = 3 }

    public enum BusineesCenter { RevenueByService = 1, Asset_Inventory = 2, FixedAsset = 3 }

    public class Invoice
    {
        public InvoiceTypes Type { get; set; }
        public string CustomerTaxID { get; set; }
        public string CustomerName { get; set; }
        public string SupplierTaxID { get; set; }
        public string SupplierName { get; set; }
        public string Date { get; set; }
        public string Code { get; set; }
        public string Number { get; set; }
        public string Comment { get; set; }
        public string CodeExpiry { get; set; }
        public int PaymentCondition { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }
        //public string AccountName { get; set; }
        public ICollection<InvoiceDetail> Details { get; set; }

        public void LoadSales(sales_invoice data)
        {
            Type = InvoiceTypes.Sales;
            CustomerName = data.contact.name;
            CustomerTaxID = data.contact.gov_code;
            SupplierName = data.app_company.name;
            SupplierTaxID = data.app_company.gov_code;
            Date = data.trans_date.Date.ToString("yyyy-MM-dd");
            Code = data.code;
            CodeExpiry = data.app_document_range != null ? data.app_document_range.expire_date != null ? Convert.ToDateTime(data.app_document_range.expire_date).Date.ToString("yyyy-MM-dd") : null : null;
            PaymentCondition = data.app_contract.app_contract_detail.Max(x => x.interval);
            CurrencyCode = data.app_currencyfx.app_currency.code;
            CurrencyRate = data.app_currencyfx.buy_value;
            Number = data.number;
            Comment = data.comment;
           
            Details = new List<InvoiceDetail>();
            foreach (sales_invoice_detail sales_invoice_detail in data.sales_invoice_detail)
            {
                foreach (var VatDetail in sales_invoice_detail.app_vat_group.app_vat_group_details)
                {
                    BusineesCenter DetailType = BusineesCenter.RevenueByService;
                    string Name = "Service";
                    if (sales_invoice_detail.item.id_item_type == item.item_type.FixedAssets)
                    {
                        DetailType = BusineesCenter.FixedAsset;
                        Name = "Fixed Asset";
                    }
                    else if (sales_invoice_detail.item.id_item_type == item.item_type.Product
                        || sales_invoice_detail.item.id_item_type == item.item_type.RawMaterial
                        || sales_invoice_detail.item.id_item_type == item.item_type.Supplies)
                    {
                        DetailType = BusineesCenter.Asset_Inventory;
                        Name = "Product";
                    }

                    InvoiceDetail Detail = Details.Where(x => x.VATPercentage == VatDetail.app_vat.coefficient && x.Type == DetailType).FirstOrDefault() != null ?
                        Details.Where(x => x.VATPercentage == VatDetail.app_vat.coefficient && x.Type == DetailType).FirstOrDefault() :
                        new InvoiceDetail();

                    Detail.Type = DetailType;
                    Detail.Cost = sales_invoice_detail.unit_cost;
                    Detail.Value = (sales_invoice_detail.SubTotal_Vat) * (VatDetail.percentage);
                    Detail.VATPercentage = Convert.ToInt32(VatDetail.app_vat.coefficient * 100);
                    Detail.Name = Name;
                    Details.Add(Detail);
                }
            }
        }

        public void LoadPurchase(purchase_invoice data)
        {
            Type = InvoiceTypes.Purchase;
            SupplierName = data.contact.name;
            SupplierTaxID = data.contact.gov_code;
            CustomerName = data.app_company.name;
            CustomerTaxID = data.app_company.gov_code;
            Date = data.trans_date.Date.ToString("yyyy-MM-dd");
            Code = data.code;
            CodeExpiry = data.app_document_range != null ? data.app_document_range.expire_date != null ? Convert.ToDateTime(data.app_document_range.expire_date).Date.ToString("yyyy-MM-dd") : null : null;
            PaymentCondition = data.app_contract.app_contract_detail.Max(x => x.interval);
            CurrencyCode = data.app_currencyfx.app_currency.code;
            CurrencyRate = data.app_currencyfx.buy_value;
            Number = data.number;
            Comment = data.comment;

            Details = new List<InvoiceDetail>();
            foreach (purchase_invoice_detail purchase_invoice_detail in data.purchase_invoice_detail)
            {
                foreach (var VatDetail in purchase_invoice_detail.app_vat_group.app_vat_group_details)
                {
                    BusineesCenter DetailType = BusineesCenter.RevenueByService;
                    string Name = purchase_invoice_detail.app_cost_center.name;

                    if (purchase_invoice_detail.item != null)
                    {
                        if (purchase_invoice_detail.item.id_item_type == item.item_type.FixedAssets)
                        {
                            DetailType = BusineesCenter.FixedAsset;
                        }
                        else if (purchase_invoice_detail.item.id_item_type == item.item_type.Product
                            || purchase_invoice_detail.item.id_item_type == item.item_type.RawMaterial
                            || purchase_invoice_detail.item.id_item_type == item.item_type.Supplies)
                        {
                            DetailType = BusineesCenter.Asset_Inventory;
                        }
                    }


                    InvoiceDetail Detail = Details.Where(x => x.VATPercentage == VatDetail.app_vat.coefficient && x.Type == DetailType).FirstOrDefault() != null ?
                        Details.Where(x => x.VATPercentage == VatDetail.app_vat.coefficient && x.Type == DetailType).FirstOrDefault() :
                        new InvoiceDetail();

                    Detail.Type = DetailType;
                    Detail.Cost = purchase_invoice_detail.unit_cost;
                    Detail.Value = (purchase_invoice_detail.SubTotal_Vat) * (VatDetail.percentage);
                    Detail.VATPercentage = Convert.ToInt32(purchase_invoice_detail.app_vat_group.app_vat_group_details.Sum(x => x.app_vat.coefficient) * 100);
                    Detail.Name = Name;
                    Details.Add(Detail);
                }
            }
        }

        public void LoadSalesReturn(sales_return data)
        {
            Type = InvoiceTypes.SalesReturn;
            CustomerName = data.contact.name;
            CustomerTaxID = data.contact.gov_code;
            SupplierName = data.app_company.name;
            SupplierTaxID = data.app_company.gov_code;
            Date = data.trans_date.Date.ToString("yyyy-MM-dd");
            Code = data.code;
            CodeExpiry = data.app_document_range != null ? data.app_document_range.expire_date != null ? Convert.ToDateTime(data.app_document_range.expire_date).Date.ToString("yyyy-MM-dd") : null : null;
            PaymentCondition = data.app_contract.app_contract_detail.Max(x => x.interval);
            CurrencyCode = data.app_currencyfx.app_currency.code;
            CurrencyRate = data.app_currencyfx.buy_value;
            Number = data.number;
            Comment = data.comment;

            Details = new List<InvoiceDetail>();
            foreach (sales_return_detail sales_return_detail in data.sales_return_detail)
            {
                foreach (var VatDetail in sales_return_detail.app_vat_group.app_vat_group_details)
                {
                    BusineesCenter DetailType = BusineesCenter.RevenueByService;
                    string Name = "Service";
                    if (sales_return_detail.item.id_item_type == item.item_type.FixedAssets)
                    {
                        DetailType = BusineesCenter.FixedAsset;
                        Name = "Fixedasset";
                    }
                    else if (sales_return_detail.item.id_item_type == item.item_type.Product
                        || sales_return_detail.item.id_item_type == item.item_type.RawMaterial
                        || sales_return_detail.item.id_item_type == item.item_type.Supplies)
                    {
                        DetailType = BusineesCenter.Asset_Inventory;
                        Name = "Product";
                    }

                    InvoiceDetail Detail = Details.Where(x => x.VATPercentage == VatDetail.app_vat.coefficient && x.Type == DetailType).FirstOrDefault() != null ?
                        Details.Where(x => x.VATPercentage == VatDetail.app_vat.coefficient && x.Type == DetailType).FirstOrDefault() :
                        new InvoiceDetail();

                    Detail.Type = DetailType;
                    Detail.Cost = sales_return_detail.unit_cost;
                    Detail.Value = (sales_return_detail.SubTotal_Vat) * (VatDetail.percentage);
                    Detail.VATPercentage = Convert.ToInt32(sales_return_detail.app_vat_group.app_vat_group_details.Sum(x => x.app_vat.coefficient) * 100);
                    Detail.Name = Name;
                    Details.Add(Detail);
                }
            }
        }

        public void LoadPurchaseReturn(purchase_return data)
        {
            Type = InvoiceTypes.PurchaseReturn;
            SupplierName = data.contact.name;
            SupplierTaxID = data.contact.gov_code;
            CustomerName = data.app_company.name;
            CustomerTaxID = data.app_company.gov_code;
            Date = data.trans_date.Date.ToString("yyyy-MM-dd");
            Code = data.code;
            CodeExpiry = data.app_document_range != null ? data.app_document_range.expire_date != null ? Convert.ToDateTime(data.app_document_range.expire_date).Date.ToString("yyyy-MM-dd") : null : null;
            PaymentCondition = data.app_contract.app_contract_detail.Max(x => x.interval);
            CurrencyCode = data.app_currencyfx.app_currency.code;
            CurrencyRate = data.app_currencyfx.buy_value;
            Number = data.number;
            Comment = data.comment;

            Details = new List<InvoiceDetail>();
            foreach (purchase_return_detail purchase_return_detail in data.purchase_return_detail)
            {
                foreach (var VatDetail in purchase_return_detail.app_vat_group.app_vat_group_details)
                {
                    BusineesCenter DetailType = BusineesCenter.RevenueByService;
                    string Name = purchase_return_detail.app_cost_center.name;

                    if (purchase_return_detail.item != null)
                    {
                        if (purchase_return_detail.item.id_item_type == item.item_type.FixedAssets)
                        {
                            DetailType = BusineesCenter.FixedAsset;
                        }
                        else if (purchase_return_detail.item.id_item_type == item.item_type.Product
                            || purchase_return_detail.item.id_item_type == item.item_type.RawMaterial
                            || purchase_return_detail.item.id_item_type == item.item_type.Supplies)
                        {
                            DetailType = BusineesCenter.Asset_Inventory;
                        }
                    }

                    InvoiceDetail Detail = Details.Where(x => x.VATPercentage == VatDetail.app_vat.coefficient && x.Type == DetailType).FirstOrDefault() != null ?
                        Details.Where(x => x.VATPercentage == VatDetail.app_vat.coefficient && x.Type == DetailType).FirstOrDefault() :
                        new InvoiceDetail();

                    Detail.Type = DetailType;
                    Detail.Cost = purchase_return_detail.unit_cost;
                    Detail.Value = (purchase_return_detail.SubTotal_Vat) * (VatDetail.percentage);
                    Detail.VATPercentage = Convert.ToInt32(purchase_return_detail.app_vat_group.app_vat_group_details.Sum(x => x.app_vat.coefficient) * 100);
                    Detail.Name = Name;
                    Details.Add(Detail);
                }
            }
        }
    }

    public class InvoiceDetail
    {
        public BusineesCenter Type { get; set; }
        public Int32 VATPercentage { get; set; }
        public decimal Value { get; set; }
        public decimal Cost { get; set; }
        public string Name { get; set; }
    }

    public class AccountMovements
    {
        public AccountTypes Type { get; set; }
        public string CustomerTaxID { get; set; }
        public string CustomerName { get; set; }
        public string SupplierTaxID { get; set; }
        public string SupplierName { get; set; }
        public string AccountName { get; set; }
        public string Number { get; set; }
        public string Date { get; set; }
        public string ReferenceInvoice { get; set; }
        public int? ReferenceInvoiceID { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Comment { get; set; }

        public void LoadPaymentsMade(app_account_detail data)
        {
            Type = AccountTypes.AccountPayable;
            CustomerName = data.app_company.name;
            CustomerTaxID = data.app_company.gov_code;
            SupplierName = data.payment_detail.payment.contact.name;
            SupplierTaxID = data.payment_detail.payment.contact.gov_code;
           
            Number = data.payment_detail.payment_schedual.First().purchase_invoice.number;
            Comment = data.payment_detail.comment;
            AccountName = data.app_account.name;
            Date = data.trans_date.Date.ToString("yyyy-MM-dd");
            CurrencyCode = CurrentSession.Currencies.Where(x => x.id_currency == data.app_currencyfx.id_currency).FirstOrDefault().code;
            CurrencyRate = data.app_currencyfx.buy_value > 0 ? data.app_currencyfx.buy_value : data.app_currencyfx.sell_value;
            Debit = data.payment_detail.value;
            Credit = 0;

            //ReferenceInvoice = data.payment_detail.payment_schedual.First().purchase_invoice.number;
            ReferenceInvoiceID = data.payment_detail.payment_schedual.First().id_purchase_invoice;
        }

        public void LoadPaymentsRecieved(app_account_detail data)
        {
            Type = AccountTypes.AccountReceivable;
            CustomerName = data.payment_detail.payment.contact.name;
            CustomerTaxID = data.payment_detail.payment.contact.gov_code;
            SupplierName = data.app_company.name;
            SupplierTaxID = data.app_company.gov_code;
            Number = data.payment_detail.payment_schedual.First().sales_invoice.number;
            Comment = data.payment_detail.comment;
            AccountName = data.app_account.name;
            Date = data.trans_date.Date.ToString("yyyy-MM-dd"); ;
            CurrencyCode = CurrentSession.Currencies.Where(x => x.id_currency == data.app_currencyfx.id_currency).FirstOrDefault().code;
            CurrencyRate = data.app_currencyfx.buy_value > 0 ? data.app_currencyfx.buy_value : data.app_currencyfx.sell_value;
            Debit = 0;
            Credit = data.payment_detail.value;

            //ReferenceInvoice = data.payment_detail.payment_schedual.First().sales_invoice.number;
            ReferenceInvoiceID = data.payment_detail.payment_schedual.First().id_sales_invoice;
        }

        //Make another API for MoneyTransfers
        public void LoadTransfers(app_account_detail data)
        {
            AccountName = data.app_account.name;
            Date = data.trans_date.Date.ToString("yyyy-MM-dd"); ;
            CurrencyCode = CurrentSession.Currencies.Where(x => x.id_currency == data.app_currencyfx.id_currency).FirstOrDefault().code;
            CurrencyRate = data.app_currencyfx.buy_value > 0 ? data.app_currencyfx.buy_value : data.app_currencyfx.sell_value;
            Debit = data.debit;
            Credit = data.credit;
        }
    }

    public class Production
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }

        //Input Data
        public BusineesCenter InputType { get; set; }
        public decimal InputCost { get; set; }

        //Output Data --> can be null
        public BusineesCenter? OutputType { get; set; }
        public decimal? OutputValue { get; set; }

        public void Load(production_execution_detail data)
        {
            Name = data.production_order_detail.production_order.name;
            Date = data.trans_date;

            if (data.item.id_item_type == item.item_type.Product ||
                data.item.id_item_type == item.item_type.RawMaterial ||
                data.item.id_item_type == item.item_type.Supplies)
            {
                InputType = BusineesCenter.Asset_Inventory;
            }
            else
            {
                InputType = BusineesCenter.RevenueByService;
            }

            InputCost = (data.unit_cost * data.quantity);

            item.item_type type = data.production_order_detail.item.id_item_type;

            if (type == item.item_type.Product ||
                type == item.item_type.RawMaterial ||
                type == item.item_type.Supplies)
            {
                OutputType = BusineesCenter.Asset_Inventory;
            }
        }
    }
}