﻿namespace entity.Brillo.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Printing;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Data.Entity;



    public class Reciept
    {

        public void Document_Print(int RangeID, object obj)
        {
            string PrinterName;
            string Content = "";

            using (db db = new db())
            {
                app_document_range app_document_range = db.app_document_range.Find(RangeID);
                app_document app_document = app_document_range.app_document;

                PrinterName = app_document_range.printer_name;

                if (app_document.id_application == App.Names.Movement)
                {
                    item_transfer item_transfer = (item_transfer)obj;
                    Content = ItemMovement(item_transfer);
                    Print(Content, app_document, PrinterName);
                }
                else if (app_document.id_application == App.Names.SalesReturn)
                {
                    sales_return sales_return = (sales_return)obj;
                    Content = SalesReturn(sales_return);
                    Print(Content, app_document, PrinterName);
                }
                else if (app_document.id_application == App.Names.SalesInvoice || app_document.id_application == App.Names.PointOfSale)
                {
                    sales_invoice sales_invoice = (sales_invoice)obj;
                    Content = SalesInvoice(sales_invoice);
                    Print(Content, app_document, PrinterName);
                }
                else if (app_document.id_application == App.Names.Restaurant)
                {
                    sales_invoice sales_invoice = (sales_invoice)obj;
                    Content = Restaurant(sales_invoice);
                    Print(Content, app_document, PrinterName);
                }
                else if (app_document.id_application == App.Names.PaymentUtility)
                {
                    payment payment = (payment)obj;
                    Content = Payment(payment);
                    Print(Content, app_document, PrinterName);
                }
            }
        }

        private void Print(string Content, app_document app_document, string PrinterName)
        {
            entity.Properties.Settings Setting = new Properties.Settings();
            if (Content != "")
            {
                if (app_document != null && PrinterName != string.Empty)
                {
                    if (app_document.style_reciept == true)
                    {
                        PrintDialog pd = new PrintDialog();

                        FlowDocument document = new FlowDocument(new Paragraph(new Run(Content)));
                        document.Name = "CognitivoERP_PrintJob";
                        document.FontFamily = new FontFamily(Setting.Reciept_FontName);
                        document.FontSize = Setting.Reciept_FontSize;
                        document.FontStretch = FontStretches.Normal;
                        document.FontWeight = FontWeights.Normal;

                        document.PagePadding = new Thickness(20);

                        document.PageHeight = double.NaN;
                        document.PageWidth = double.NaN;
                        //document.

                        //Specify minimum page sizes. Origintally 283, but was too small.
                        document.MinPageWidth = Setting.Reciept_MinWidth;
                        //Specify maximum page sizes.
                        document.MaxPageWidth = Setting.Reciept_MaxWidth;

                        IDocumentPaginatorSource idpSource = document;
                        try
                        {
                            pd.PrintQueue = new PrintQueue(new PrintServer(), PrinterName);
                            pd.PrintDocument(idpSource.DocumentPaginator, Content);
                        }
                        catch
                        { MessageBox.Show("Output (Reciept Printer) not Found Error", "Error 101"); }
                    }
                }
            }
        }

        public string ItemMovement(item_transfer i)
        {
            string Header = string.Empty;
            string Detail = string.Empty;
            string Footer = string.Empty;

            string CompanyName = string.Empty;

            using (db db = new db())
            {
                CompanyName = db.app_company.Where(x => x.id_company == i.id_company).FirstOrDefault().name;
            }

            string TransNumber = i.number;
            DateTime TransDate = i.trans_date;
            string BranchName = string.Empty;
            if (i.app_location_origin != null)
            {
                if (i.app_location_origin.app_branch != null)
                {
                    BranchName = i.app_location_origin.app_branch.name;

                }
            }

            string UserGiven = string.Empty;
            if (i.user_given != null)
            {
                UserGiven = i.user_given.name_full;
            }

            string DepartmentName = string.Empty;
            if (i.app_department != null)
            {
                DepartmentName = i.app_department.name;
            }
            string ProjectName = string.Empty;
            string ProjectCode = string.Empty;
            if (i.project != null)
            {
                ProjectName = i.project.name;
                ProjectCode = i.project.code;
            }

            Header =
                CompanyName + "\n"
                + "Registro de PMD. Transaccion: " + TransNumber + "\n"
                + "Fecha y Hora: " + TransDate.ToString() + "\n"
                + "Local Expendido: " + BranchName + "\n"
                + "\n"
                + "Entrega: " + UserGiven + "\n"
                + "Sector: " + DepartmentName + "\n"
                + "Project: " + ProjectCode + " - " + ProjectName + "\n"
                + "-------------------------------"
                + "\n"
                + "Cantiad, Codigo, Description" + "\n";

            foreach (item_transfer_detail d in i.item_transfer_detail)
            {
                if (d.project_task != null)
                {
                    if (d.project_task.parent != null)
                    {
                        Detail += "ACTIV. : " + d.project_task.parent.item_description + "\n";
                    }

                    foreach (project_task project_task in d.project_task.child)
                    {
                        string ItemName = string.Empty;
                        string ItemCode = string.Empty;

                        if (d.project_task.items != null)
                        {
                            ItemName = d.project_task.items.name;
                            ItemCode = d.project_task.code;
                        }

                        decimal? Qty = d.project_task.quantity_est;
                        string TaskName = d.project_task.item_description;
                        string TaskCode = d.project_task.items.code;

                        Detail = Detail +
                            ""
                            + "-------------------------------" + "\n"
                            + Qty.ToString() + ItemCode + "\n"
                            + ItemName;

                        if (d.project_task.project_task_dimension.Count() > 0)
                        {
                            Detail = Detail + "Dimension, Value, Measurement" + "\n";
                        }
                        foreach (project_task_dimension project_task_dimension in d.project_task.project_task_dimension)
                        {
                            decimal value = project_task_dimension.value;
                            string name = project_task_dimension.app_dimension != null ? project_task_dimension.app_dimension.name : "";
                            string measurement = project_task_dimension.app_measurement != null ? project_task_dimension.app_measurement.name : "";
                            string dimension = "-------------------------------" + "\n"
                           + name + "\t" + value + "\t" + measurement + "\n";
                            Detail = Detail + dimension + "\n";
                        }

                        //}
                    }
                }
                else
                {
                    if (d.item_product != null)
                    {
                        if (d.item_product.item != null)
                        {
                            Detail += "ACTIV. : " + d.item_product.item.description + "\n";
                            string ItemName = string.Empty;
                            string ItemCode = string.Empty;

                            if (d.item_product.item != null)
                            {
                                ItemName = d.item_product.item.name;
                                ItemCode = d.item_product.item.code;
                            }

                            decimal? Qty = d.quantity_destination;

                            Detail = Detail
                                + "-------------------------------" + "\n"
                            + Qty.ToString() + "\t" + ItemCode + "\n"
                            + ItemName;

                            if (d.item_transfer_dimension.Count() > 0)
                            {
                                Detail = Detail +
                               "-------------------------------" + "\n"
                             + "Dimension, Value, Measurement" + "\n";
                            }
                            foreach (item_transfer_dimension item_transfer_dimension in d.item_transfer_dimension)
                            {
                                decimal value = item_transfer_dimension.value;
                                string name = item_transfer_dimension.app_dimension != null ? item_transfer_dimension.app_dimension.name : "";
                                string dimension = "\n"
                                + name + "\t" + value + "\n";
                                Detail = Detail + dimension + "\n";
                            }
                        }
                    }
                }
            }

            Footer = "-------------------------------\n";
            if (i.employee != null)
            {
                Footer += "RETIRADO: " + i.employee.name + "\n";
            }
            if (i.user_given != null)
            {
                Footer += "APRORADO: " + i.user_given.name_full + "\n";
            }

            Footer += "-------------------------------";

            string Text = Header + Detail + Footer;
            return Text;
        }

        public string SalesReturn(sales_return sales_return)
        {
            string Header = string.Empty;
            string Detail = string.Empty;
            string Footer = string.Empty;
            string CompanyName = string.Empty;
            app_company app_company = null;
            if (sales_return.app_company != null)
            {
                CompanyName = sales_return.app_company.name;
            }
            else
            {
                using (db db = new db())
                {
                    if (db.app_company.Where(x => x.id_company == sales_return.id_company).FirstOrDefault() != null)
                    {
                        app_company = db.app_company.Where(x => x.id_company == sales_return.id_company).FirstOrDefault();
                        CompanyName = app_company.name;
                    }
                }
            }
            string UserGiven = "";
            if (sales_return.security_user != null)
            {
                UserGiven = sales_return.security_user.name;
            }
            else
            {
                using (db db = new db())
                {
                    security_user security_user = db.security_user.Where(x => x.id_user == sales_return.id_user).FirstOrDefault();
                    if (security_user != null)
                    {
                        UserGiven = security_user.name;
                    }
                }
            }

            string TransNumber = sales_return.number;
            DateTime TransDate = sales_return.trans_date;
            string BranchName = sales_return.app_branch.name;

            Header =
                CompanyName + "\n"
                + "RUC:" + app_company.gov_code + "\n"
                + app_company.address + "\n"
                + "***" + app_company.alias + "***" + "\n"
                + "Timbrado: " + sales_return.app_document_range.code + " Vto: " + sales_return.app_document_range.expire_date
                + "\n"
                + "-------------------------------- \n"
                + "Descripcion, Cantiad, Precio" + "\n"
                + "--------------------------------" + "\n"
                + "\n";

            foreach (sales_return_detail d in sales_return.sales_return_detail)
            {
                string ItemName = d.item.name;
                string ItemCode = d.item.code;
                decimal? Qty = d.quantity;
                string TaskName = d.item_description;
                decimal? UnitPrice_Vat = Math.Round(d.UnitPrice_Vat, 2);

                Detail = Detail
                    + ItemName + "\n"
                    + Qty.ToString() + "\t" + ItemCode + "\t" + UnitPrice_Vat + "\n";
            }

            Footer = "--------------------------------" + "\n";
            Footer += "Total " + sales_return.app_currencyfx.app_currency.name + ": " + sales_return.GrandTotal + "\n";
            Footer += "Fecha & Hora: " + sales_return.trans_date + "\n";
            Footer += "Numero de Factura: " + sales_return.number + "\n";
            Footer += "-------------------------------" + "\n";

            if (sales_return != null)
            {
                List<sales_return_detail> sales_return_detail = sales_return.sales_return_detail.ToList();
                if (sales_return_detail.Count > 0)
                {
                    using (db db = new db())
                    {
                        var listvat = sales_return_detail
                          .Join(db.app_vat_group_details, ad => ad.id_vat_group, cfx => cfx.id_vat_group
                              , (ad, cfx) => new { name = cfx.app_vat.name, value = ad.unit_price * (cfx.app_vat.coefficient * cfx.percentage), id_vat = cfx.app_vat.id_vat, ad })
                              .GroupBy(a => new { a.name, a.id_vat, a.ad })
                      .Select(g => new
                      {
                          vatname = g.Key.ad.app_vat_group.name,
                          id_vat = g.Key.id_vat,
                          name = g.Key.name,
                          value = g.Sum(a => a.value * a.ad.quantity)
                      }).ToList();

                        var VAtList = listvat.GroupBy(x => x.id_vat).Select(g => new
                        {
                            vatname = g.Max(y => y.vatname),
                            id_vat = g.Max(y => y.id_vat),
                            name = g.Max(y => y.name),
                            value = g.Sum(a => a.value)
                        }).ToList();

                        foreach (dynamic item in VAtList)
                        {
                            Footer += item.vatname + "   : " + Math.Round(item.value, 2) + "\n";
                        }
                        Footer += "Total IVA: " + sales_return.app_currencyfx.app_currency.name + " " + Math.Round(VAtList.Sum(x => x.value), 2) + "\n";
                    }
                }
            }

            Footer += "------------------------------- \n";
            Footer += "Cliente   : " + sales_return.contact.name + "\n";
            Footer += "Documento : " + sales_return.contact.gov_code + "\n";
            Footer += "Condicion : " + sales_return.app_condition.name + "\n";
            Footer += "------------------------------- \n";
            Footer += "Sucursal    : " + sales_return.app_branch.name + " Terminal: " + sales_return.app_terminal.name + "\n";
            Footer += "Cajero/a    : " + UserGiven + "/n";
            Footer += "/n";
            Footer += "Factura impresa utilizando Cognitivo ERP /n" +
                      "-- http://www.cognitivo.in --";

            string Text = Header + Detail + Footer;
            return Text;
        }

        public string SalesInvoice(sales_invoice sales_invoice)
        {
            string Header = string.Empty;
            string Detail = string.Empty;
            string Footer = string.Empty;
            string BranchName = string.Empty;
            string TerminalName = string.Empty;
            string BranchAddress = string.Empty;
            app_company app_company = null;

            if (sales_invoice.app_company != null)
            {
                app_company = sales_invoice.app_company;
            }
            else
            {
                using (db db = new db())
                {
                    if (db.app_company.Find(sales_invoice.id_company) != null)
                    {
                        app_company = db.app_company.Find(sales_invoice.id_company);
                    }
                }
            }

            app_branch app_branch = CurrentSession.Branches.Where(x => x.id_branch == sales_invoice.id_branch).FirstOrDefault();
            if (app_branch != null)
            {
                BranchName = app_branch.name;
                BranchAddress = app_branch.address;
            }

            string UserGiven = "";
            if (sales_invoice.security_user != null)
            {
                UserGiven = sales_invoice.security_user.name;
            }
            else
            {
                using (db db = new db())
                {
                    security_user security_user = db.security_user.Find(sales_invoice.id_user);
                    if (security_user != null)
                    {
                        UserGiven = security_user.name;
                    }
                }
            }

            string ContractName = "";
            app_contract app_contract = CurrentSession.Contracts.Where(x => x.id_contract == sales_invoice.id_contract).FirstOrDefault();
            if (app_contract != null)
            {
                ContractName = app_contract.name;
            }

            string ConditionName = "";
            app_condition app_condition = CurrentSession.Conditions.Where(x => x.id_condition == sales_invoice.id_condition).FirstOrDefault();
            if (app_condition != null)
            {
                ConditionName = app_condition.name;
            }
            app_terminal app_terminal = CurrentSession.Terminals.Where(x => x.id_terminal == sales_invoice.id_terminal).FirstOrDefault();
            if (app_terminal != null)
            {
                TerminalName = app_terminal.name;
            }

            string CurrencyName = "";
            if (sales_invoice.app_currencyfx != null)
            {
                if (sales_invoice.app_currencyfx.app_currency != null)
                {
                    CurrencyName = sales_invoice.app_currencyfx.app_currency.name;
                }
            }
            else
            {
                using (db db = new db())
                {
                    app_currencyfx app_currencyfx = db.app_currencyfx.Find(sales_invoice.id_currencyfx);
                    if (app_currencyfx != null)
                    {
                        CurrencyName = app_currencyfx.app_currency.name;
                    }
                }
            }

            string TransNumber = sales_invoice.number;
            DateTime TransDate = sales_invoice.trans_date;

            Header =
                app_company.name + "\n"
                + "RUC:" + app_company.gov_code + "\n"
                + app_company.address + "\n"
                + "***" + app_company.alias + "***" + "\n"
                + "Timbrado    : " + sales_invoice.app_document_range.code + "\n"
                + "Vencimiento : " + String.Format("{0:dd-MM-yyyy}", sales_invoice.app_document_range.expire_date) + "\n"
                + "-------------------------------- \n"
                + "Descripcion, Cantiad, Precio" + "\n"
                + "--------------------------------" + "\n"
                + "\n";

            foreach (sales_invoice_detail d in sales_invoice.sales_invoice_detail)
            {
                string ItemName = d.item_description;
                string ItemCode = d.item.code;
                decimal? Qty = Math.Round(d.quantity, 2);
                string TaskName = d.item_description;

                Detail = Detail + (string.IsNullOrEmpty(Detail) ? "\n" : "")
                    + ItemName + "\n"
                    + Qty.ToString() + "\t" + ItemCode + "\t" + Math.Round((d.SubTotal_Vat), 2) + "\n";
            }

            decimal DiscountTotal = sales_invoice.sales_invoice_detail.Sum(x => x.Discount_SubTotal_Vat);

            Footer = "--------------------------------" + "\n";
            Footer += "Total Bruto       : " + Math.Round((sales_invoice.GrandTotal + DiscountTotal), 2) + "\n";
            Footer += "Total Descuento   : -" + Math.Round(sales_invoice.sales_invoice_detail.Sum(x => x.Discount_SubTotal_Vat), 2) + "\n";
            Footer += "Total " + CurrencyName + " : " + Math.Round(sales_invoice.GrandTotal, 2) + "\n";
            Footer += "Total Cambio       : " + Math.Round((sales_invoice.TotalChanged), 2) + "\n";
            Footer += "Fecha & Hora      : " + sales_invoice.trans_date + "\n";
            Footer += "Numero de Factura : " + sales_invoice.number + "\n";
            Footer += "-------------------------------" + "\n";

            if (sales_invoice != null)
            {
                List<sales_invoice_detail> sales_invoice_detail = sales_invoice.sales_invoice_detail.ToList();
                if (sales_invoice_detail.Count > 0)
                {
                    using (db db = new db())
                    {
                        var listvat = sales_invoice_detail
                          .Join(db.app_vat_group_details, ad => ad.id_vat_group, cfx => cfx.id_vat_group
                              , (ad, cfx) => new { name = cfx.app_vat.name, value = ad.unit_price * (cfx.app_vat.coefficient * cfx.percentage), id_vat = cfx.app_vat.id_vat, ad })
                              .GroupBy(a => new { a.name, a.id_vat, a.ad })
                      .Select(g => new
                      {
                          vatname = g.Key.ad.app_vat_group != null ? g.Key.ad.app_vat_group.name : "",
                          id_vat = g.Key.id_vat,
                          name = g.Key.name,
                          value = g.Sum(a => a.value * a.ad.quantity)
                      }).ToList();

                        var VAtList = listvat.GroupBy(x => x.id_vat).Select(g => new
                        {
                            vatname = g.Max(y => y.vatname),
                            id_vat = g.Max(y => y.id_vat),
                            name = g.Max(y => y.name),
                            value = g.Sum(a => a.value)
                        }).ToList();

                        foreach (dynamic item in VAtList)
                        {
                            Footer += item.vatname + "   : " + Math.Round(item.value, 2) + "\n";
                        }

                        Footer += "Total IVA : " + CurrencyName + " " + Math.Round(VAtList.Sum(x => x.value), 2) + "\n";
                    }
                }
            }

            Footer += "------------------------------- \n";
            Footer += "Cliente    : " + sales_invoice.contact.name + "\n";
            Footer += "Documento  : " + sales_invoice.contact.gov_code + "\n";
            Footer += "Condicion  : " + ConditionName + "\n";
            Footer += "------------------------------- \n";
            Footer += "Sucursal   : " + BranchName + "\n";
            Footer += "Sucursal Address  : " + BranchAddress + "\n";
            Footer += "Terminal   : " + TerminalName;

            if (sales_invoice.id_sales_rep > 0)
            {
                string SalesRep_Name = "";
                int RepID = (int)sales_invoice.id_sales_rep;
                sales_rep sales_rep = CurrentSession.SalesReps.Where(x => x.id_sales_rep == RepID).FirstOrDefault();
                if (sales_rep != null)
                {
                    SalesRep_Name = sales_rep.name;
                }

                Footer += "\n";
                Footer += "Vendedor/a : " + SalesRep_Name;
            }
            Footer += "\n";
            Footer += "Cajero/a : " + UserGiven;

            string Text = Header + Detail + Footer;
            return Text;
        }

        public string Restaurant(sales_invoice sales_invoice)
        {
            string Header = string.Empty;
            string Detail = string.Empty;
            string Footer = string.Empty;
            string BranchName = string.Empty;
            string TerminalName = string.Empty;
            string BranchAddress = string.Empty;
            app_company app_company = null;

            if (sales_invoice.app_company != null)
            {
                app_company = sales_invoice.app_company;
            }
            else
            {
                using (db db = new db())
                {
                    if (db.app_company.Find(sales_invoice.id_company) != null)
                    {
                        app_company = db.app_company.Find(sales_invoice.id_company);
                    }
                }
            }

            app_branch app_branch = CurrentSession.Branches.Where(x => x.id_branch == sales_invoice.id_branch).FirstOrDefault();
            if (app_branch != null)
            {
                BranchName = app_branch.name;
                BranchAddress = app_branch.address;
            }

            string UserGiven = "";
            if (sales_invoice.security_user != null)
            {
                UserGiven = sales_invoice.security_user.name;
            }
            else
            {
                using (db db = new db())
                {
                    security_user security_user = db.security_user.Find(sales_invoice.id_user);
                    if (security_user != null)
                    {
                        UserGiven = security_user.name;
                    }
                }
            }

            string ContractName = "";
            app_contract app_contract = CurrentSession.Contracts.Where(x => x.id_contract == sales_invoice.id_contract).FirstOrDefault();
            if (app_contract != null)
            {
                ContractName = app_contract.name;
            }

            string ConditionName = "";
            app_condition app_condition = CurrentSession.Conditions.Where(x => x.id_condition == sales_invoice.id_condition).FirstOrDefault();
            if (app_condition != null)
            {
                ConditionName = app_condition.name;
            }
            app_terminal app_terminal = CurrentSession.Terminals.Where(x => x.id_terminal == sales_invoice.id_terminal).FirstOrDefault();
            if (app_terminal != null)
            {
                TerminalName = app_terminal.name;
            }

            string CurrencyName = "";
            if (sales_invoice.app_currencyfx != null)
            {
                if (sales_invoice.app_currencyfx.app_currency != null)
                {
                    CurrencyName = sales_invoice.app_currencyfx.app_currency.name;
                }
            }
            else
            {
                using (db db = new db())
                {
                    app_currencyfx app_currencyfx = db.app_currencyfx.Find(sales_invoice.id_currencyfx);
                    if (app_currencyfx != null)
                    {
                        CurrencyName = app_currencyfx.app_currency.name;
                    }
                }
            }

            string TransNumber = sales_invoice.number;
            DateTime TransDate = sales_invoice.trans_date;

            Header =
                app_company.name + "\n"
                + "RUC:" + app_company.gov_code + "\n"
                + app_company.address + "\n"
                + "***" + app_company.alias + "***" + "\n"
                + "-------------------------------- \n"
                + "Descripcion, Cantiad" + "\n"
                + "--------------------------------" + "\n"
                + "\n";

            foreach (sales_invoice_detail d in sales_invoice.sales_invoice_detail)
            {
                string ItemName = d.item.name;
                string ItemCode = d.item.code;
                decimal? Qty = Math.Round(d.quantity, 2);
                string TaskName = d.item_description;

                Detail = Detail + (string.IsNullOrEmpty(Detail) ? "\n" : "")
                    + ItemName + "\n"
                    + Qty.ToString() + "\t" + ItemCode + "\t" + "\n";
            }

            decimal DiscountTotal = sales_invoice.sales_invoice_detail.Sum(x => x.Discount_SubTotal_Vat);

            Footer = "--------------------------------" + "\n";
            Footer += "Fecha & Hora      : " + DateTime.Now + "\n";
            Footer += "-------------------------------" + "\n";




            Footer += "------------------------------- \n";
            Footer += "Location   : " + sales_invoice.Location.name + "\n";
            Footer += "------------------------------- \n \n";
            Footer += Localize.StringText("Client") + " : ................... \n \n";
            Footer += Localize.StringText("GovernmentID") + " : ................... \n";

            if (sales_invoice.id_sales_rep > 0)
            {
                string SalesRep_Name = "";
                int RepID = (int)sales_invoice.id_sales_rep;
                sales_rep sales_rep = CurrentSession.SalesReps.Where(x => x.id_sales_rep == RepID).FirstOrDefault();
                if (sales_rep != null)
                {
                    SalesRep_Name = sales_rep.name;
                }

                Footer += "\n";
                Footer += Localize.StringText("Waiter") + ": " + SalesRep_Name;
            }

            Footer += "\n";
            Footer += "Cajero/a : " + UserGiven;

            string Text = Header + Detail + Footer;
            return Text;
        }

        public string Payment(payment payment)
        {
            string Header = string.Empty;
            string Detail = string.Empty;
            string Footer = string.Empty;

            string CompanyName = string.Empty;

            app_company app_company = null;

            if (payment.app_company != null)
            {
                CompanyName = payment.app_company.name;
                app_company = payment.app_company;
            }
            else
            {
                using (db db = new db())
                {
                    if (db.app_company.Where(x => x.id_company == payment.id_company).FirstOrDefault() != null)
                    {
                        app_company = db.app_company.Where(x => x.id_company == payment.id_company).FirstOrDefault();
                        CompanyName = app_company.name;
                    }
                }
            }

            string UserName = "";

            if (payment.security_user != null)
            {
                UserName = payment.security_user.name;
            }
            else
            {
                using (db db = new db())
                {
                    if (db.security_user.Where(x => x.id_user == payment.id_user).FirstOrDefault() != null)
                    {
                        security_user security_user = db.security_user.Where(x => x.id_user == payment.id_user).FirstOrDefault();
                        UserName = security_user.name;
                    }
                }
            }

            string TransNumber = payment.number;
            DateTime TransDate = payment.trans_date;

            Header =
                CompanyName + "\n"
                + "R.U.C.   :" + app_company.gov_code + "\n"
                + app_company.address + "\n"
                + "***" + app_company.alias + "***" + "\n"
                + "Timbrado : " + TransNumber + " Vto: " + payment.app_document_range.expire_date + "\n"
                + "Fecha    : " + payment.trans_date
                + "\n"
                + "-------------------------------- \n"
                + "Cuenta, Valor, Moneda" + "\n"
                + "--------------------------------" + "\n"
                + "\n";

            string InvoiceNumber = string.Empty;
            string CustomerName = string.Empty;
            string gov_code = string.Empty;
            foreach (payment_detail d in payment.payment_detail)
            {
                string AccountName = string.Empty;

                if (d.app_account == null)
                {
                    using (db db = new db())
                    {
                        app_account app_account = db.app_account.Where(x => x.id_account == d.id_account).FirstOrDefault();
                        AccountName = app_account.name;
                    }
                }
                else
                {
                    AccountName = d.app_account.name;
                }

                string currency = string.Empty;
                if (d.app_currencyfx == null)
                {
                    using (db db = new db())
                    {
                        currency = db.app_currencyfx.Where(x => x.id_currencyfx == d.id_currencyfx).FirstOrDefault().app_currency.name;
                    }
                }
                else
                {
                    currency = d.app_currencyfx.app_currency.name;
                }

                decimal? value = d.value;

                Detail = Detail
                    + AccountName + "\n"
                    + value.ToString() + "\t" + currency + "\n";

                if (InvoiceNumber == string.Empty)
                {
                    InvoiceNumber = d.payment_schedual.FirstOrDefault().sales_invoice.number;
                    CustomerName = d.payment_schedual.FirstOrDefault().contact.name;
                    gov_code = d.payment_schedual.FirstOrDefault().contact.name;
                }
            }

            Footer += "Factura  : " + InvoiceNumber + "\n";
            Footer += "Client  : " + CustomerName + "-" + gov_code + "\n";
            Footer += "--------------------------------" + "\n";

            string Text = Header + Detail + Footer;
            return Text;
        }

        public void ZReport(app_account_session app_account_session)
        {
            string Header = string.Empty;
            string Detail = string.Empty;
            string Footer = string.Empty;
            string CompanyName = string.Empty;
            string BranchName = string.Empty;

            app_company app_company = null;

            if (app_account_session.app_company != null)
            {
                CompanyName = app_account_session.app_company.name;
                app_company = app_account_session.app_company;
            }
            else
            {
                using (db db = new db())
                {
                    if (db.app_company.Find(app_account_session.id_company) != null)
                    {
                        app_company = db.app_company.Find(app_account_session.id_company);
                        CompanyName = app_company.name;
                    }
                }
            }

            string UserName = "";

            if (app_account_session.security_user != null)
            {
                UserName = app_account_session.security_user.name;
            }
            else
            {
                using (db db = new db())
                {
                    if (db.security_user.Find(app_account_session.id_user) != null)
                    {
                        security_user security_user = db.security_user.Find(app_account_session.id_user);
                        UserName = security_user.name;
                    }
                }
            }

            app_branch app_branch = CurrentSession.Branches.Where(x => x.id_branch == CurrentSession.Id_Branch).FirstOrDefault();
            if (app_branch != null)
            {
                BranchName = app_branch.name;
            }

            string SessionID = app_account_session.id_session.ToString();
            DateTime OpenDate = app_account_session.op_date;
            DateTime CloseDate = DateTime.Now;

            if (app_account_session.cl_date != null)
            {
                CloseDate = (DateTime)app_account_session.cl_date;
            }

            Header =
                "*** Z Report ***" + "\n"
                + CompanyName + "\t" + BranchName + "\n"
                + "R.U.C.   :" + app_company.gov_code + "\n"
                + app_company.address + "\n"
                + "***" + app_company.alias + "***" + "\n"
                + "Apertura : " + OpenDate + "\n"
                + "Cierre   : " + CloseDate
                + "\n"
                + "\n-------------------------------------" + "\n";

            string CustomerName = string.Empty;


            List<app_currency> CurrencyList = app_account_session.app_account_detail.GroupBy(x => x.app_currencyfx.id_currency).Select(x => x.FirstOrDefault().app_currencyfx.app_currency).ToList();

            foreach (app_currency Currency in CurrencyList)
            {
                decimal SumOpening = app_account_session.app_account_detail
                    .Where(x => x.tran_type == app_account_detail.tran_types.Open && x.app_currencyfx.id_currency == Currency.id_currency).Sum(x => x.credit);
                decimal SumMovement = app_account_session.app_account_detail
                    .Where(x => x.tran_type == app_account_detail.tran_types.Transaction && x.app_currencyfx.id_currency == Currency.id_currency).Sum(x => x.credit);
                decimal SumOptrans = app_account_session.app_account_detail
                    .Where(x => (x.tran_type == app_account_detail.tran_types.Open || x.tran_type == app_account_detail.tran_types.Transaction)
                    && x.app_currencyfx.id_currency == Currency.id_currency).Sum(x => x.credit);
                decimal Sumclosing = app_account_session.app_account_detail
                    .Where(x => x.tran_type == app_account_detail.tran_types.Close && x.app_currencyfx.id_currency == Currency.id_currency).Sum(x => x.debit);

                Detail += "Moneda             : " + Currency.name + "\n";
                Detail += "Total de Ventas    : " + Math.Round(SumMovement, 2) + "\n";
                Detail += "--------------------------------" + "\n";

                Detail += "Balance de Apertura: " + Math.Round(SumOpening, 2) + "\n";

                var listvat = app_account_session.app_account_detail.Where(x => x.tran_type == app_account_detail.tran_types.Transaction && x.app_currencyfx.id_currency == Currency.id_currency)
                         .GroupBy(a => new { a.id_payment_type, a.id_currencyfx })
                     .Select(g => new
                     {
                         Currencyname = g.Max(x => x.app_currencyfx).app_currency.name,
                         paymentname = g.Max(x => x.payment_type).name,
                         id_currencyfx = g.Key.id_currencyfx,
                         id_payment_type = g.Key.id_payment_type,
                         value = g.Sum(a => a.credit)
                     }).ToList().OrderBy(x => x.id_currencyfx);


                foreach (dynamic item in listvat)
                {
                    Detail += item.paymentname + " : \t" + Math.Round(item.value, 2) + "\n";
                }

                Detail += "Cierre acorde Sistema : " + Math.Round(SumOptrans, 2) + "\n";
                Detail += "Cierre acorde Usuario : " + Math.Round(Sumclosing, 2) + "\n";
                if (SumOptrans != Sumclosing)
                {
                    Detail += "** Diferencia         : " + (Math.Round(Sumclosing, 2) - Math.Round(SumOptrans, 2));
                }
                Detail += "\n--------------------------------" + "\n";
                Detail += "\n--------------------------------" + "\n";
            }

            using (db db = new db())
            {
                decimal amount = 0M;
                int[] id_schedual = new int[10];
                int index = 0;

                foreach (app_account_detail account_detail in db.app_account_detail.Where(x => x.id_session == app_account_session.id_session && x.tran_type == app_account_detail.tran_types.Transaction).ToList())
                {
                    foreach (payment_schedual payment_schedual in account_detail.payment_detail.payment_schedual.ToList())
                    {
                        if (!id_schedual.Contains(payment_schedual.parent.id_payment_schedual))
                        {
                            if (payment_schedual.parent != null)
                            {
                                id_schedual[index] = payment_schedual.parent.id_payment_schedual;
                                amount += payment_schedual.parent.debit;
                            }
                            else
                            {
                                amount += payment_schedual.credit;
                            }
                        }
                    }
                }

                Detail += "Total de Ventas : " + Math.Round(amount, 2) + " \n";
            }
            Footer += "Cajero/a : " + UserName + " \n";
            Footer += "--------------------------------" + " \n";

            string Text = Header + Detail + Footer;

            Reciept Reciept = new Reciept();
            PrintDialog pd = new PrintDialog();

            FlowDocument document = new FlowDocument(new Paragraph(new Run(Text)));
            document.Name = "ItemMovement";
            document.FontFamily = new FontFamily("Courier New");
            document.FontSize = 11.0;
            document.FontStretch = FontStretches.Normal;
            document.FontWeight = FontWeights.Normal;

            document.PagePadding = new Thickness(20);

            document.PageHeight = double.NaN;
            document.PageWidth = double.NaN;
            //document.

            //Specify minimum page sizes. Origintally 283, but was too small.
            document.MinPageWidth = 283;
            //Specify maximum page sizes.
            document.MaxPageWidth = 300;

            IDocumentPaginatorSource idpSource = document;
            try
            {
                Nullable<bool> print = pd.ShowDialog();
                if (print == true)
                {
                    pd.PrintDocument(idpSource.DocumentPaginator, Text);
                }
            }
            catch
            { MessageBox.Show("Output (Reciept Printer) not Found Error", "Error 101"); }
        }
    }
}