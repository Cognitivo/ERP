﻿using cntrl.Class;
using cntrl.Controls;
using entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Cognitivo.Sales
{
    public partial class Invoice : INotifyPropertyChanged
    {
        //Global Variables
        private CollectionViewSource sales_invoiceViewSource;

        private CollectionViewSource sales_invoicesales_invoice_detailViewSource;
        private CollectionViewSource sales_invoicesales_invoice_detailsales_packinglist_relationViewSource;
        private SalesInvoiceDB SalesInvoiceDB = new SalesInvoiceDB();

        private cntrl.PanelAdv.pnlPacking pnlPacking;
        private cntrl.PanelAdv.pnlSalesOrder pnlSalesOrder;

        public DateTime start_Range
        {
            get { return _start_Range; }
            set
            {
                if (_start_Range != value)
                {
                    _start_Range = value;
                }
            }
        }

        private DateTime _start_Range = DateTime.Now.AddDays(-7);

        public DateTime end_Range
        {
            get { return _end_Range; }
            set
            {
                if (_end_Range != value)
                {
                    _end_Range = value;
                }
            }
        }

        private DateTime _end_Range = DateTime.Now.AddDays(+1);

        public Invoice()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region DataLoad

        private void SalesInvoice_Loaded(object sender, EventArgs e)
        {
            //SalesInvoiceDB = new SalesInvoiceDB();

            sales_invoicesales_invoice_detailsales_packinglist_relationViewSource = (CollectionViewSource)FindResource("sales_invoicesales_invoice_detailsales_packinglist_relationViewSource");
            sales_invoicesales_invoice_detailViewSource = FindResource("sales_invoicesales_invoice_detailViewSource") as CollectionViewSource;

            load_PrimaryDataThread(null, null);
            load_SecondaryDataThread();
        }

        private async void load_PrimaryDataThread(object sender, EventArgs e)
        {
            Settings SalesSettings = new Settings();
            var predicate = PredicateBuilder.True<sales_invoice>();
            predicate = predicate.And(x => x.id_company == CurrentSession.Id_Company);
            predicate = predicate.And(x => x.is_head == true);
            predicate = predicate.And(x => x.is_archived == false);

            if (start_Range != Convert.ToDateTime("1/1/0001"))
            {
                predicate = predicate.And(x => x.trans_date >= start_Range.Date);
            }
            if (end_Range != Convert.ToDateTime("1/1/0001"))
            {
                predicate = predicate.And(x => x.trans_date <= end_Range.Date);
            }

            sales_invoiceViewSource = ((CollectionViewSource)(FindResource("sales_invoiceViewSource")));

            if (SalesSettings.FilterByBranch)
            {
                await SalesInvoiceDB.sales_invoice.Where(predicate).OrderByDescending(x => x.trans_date).ThenBy(x => x.number).LoadAsync();
            }
            else
            {
                await SalesInvoiceDB.sales_invoice.Where(predicate).OrderByDescending(x => x.trans_date).ThenBy(x => x.number).LoadAsync();
            }

            sales_invoiceViewSource.Source = SalesInvoiceDB.sales_invoice.Local;
            if (SalesInvoiceDB.sales_invoice.Local.Count() > 0)
            {
                if (sales_invoicesales_invoice_detailViewSource.View != null)
                {
                    sales_invoicesales_invoice_detailViewSource.View.Refresh();
                }
            }
        }

        private async void load_SecondaryDataThread()
        {
            await Dispatcher.InvokeAsync(new Action(() =>
            {
                cbxDocument.ItemsSource = entity.Brillo.Logic.Range.List_Range(SalesInvoiceDB, entity.App.Names.SalesInvoice, CurrentSession.Id_Branch, CurrentSession.Id_Terminal);
            }));

            cbxTransType.ItemsSource = Enum.GetValues(typeof(Status.TransactionTypes));
        }

        #endregion DataLoad

        #region "Action Events"

        private void btnNew_Click(object sender)
        {
            Settings SalesSettings = new Settings();
            sales_invoice sales_invoice = SalesInvoiceDB.New(SalesSettings.TransDate_Offset, false);
            cbxCurrency.get_DefaultCurrencyActiveRate();

            SalesInvoiceDB.sales_invoice.Add(sales_invoice);

            sales_invoiceViewSource.View.MoveCurrentToLast();
            sbxContact.Text = "";
            sbxItem.Text = "";
        }

        private void toolBar_btnEdit_Click(object sender)
        {
            if (sales_invoiceDataGrid.SelectedItem != null)
            {
                sales_invoice sales_invoice_old = (sales_invoice)sales_invoiceDataGrid.SelectedItem;
                sales_invoice_old.IsSelected = true;
                sales_invoice_old.State = EntityState.Modified;
                SalesInvoiceDB.Entry(sales_invoice_old).State = EntityState.Modified;
            }
            else
            {
                toolBar.msgWarning("Please Select an Item");
            }
        }

        private void btnDelete_Click(object sender)
        {
            sales_invoice sales_invoice = (sales_invoice)sales_invoiceDataGrid.SelectedItem;
            if (sales_invoice != null && sales_invoice.State != EntityState.Added)
            {
                sales_invoice.is_archived = true;
                sales_invoice.State = EntityState.Modified;
                SalesInvoiceDB.SaveChanges();

                load_PrimaryDataThread(null, null);
            }
            else if (sales_invoice != null && sales_invoice.State == EntityState.Added)
            {
                toolBar_btnCancel_Click(sender);
            }
        }

        private void btnSave_Click(object sender)
        {
            sales_invoice sales_invoice = (sales_invoice)sales_invoiceDataGrid.SelectedItem;

            if (sales_invoice.status == Status.Documents_General.Approved)
            {
                if (sales_invoice != null)
                {
                    UpdateMovementReApprove UpdateMovementReApprove = new UpdateMovementReApprove();
                    CheckMovementReApprove CheckMovementReApprove = new CheckMovementReApprove();
                    UpdatePaymentReApprove UpdatePaymentReApprove = new UpdatePaymentReApprove();
                    CheckPaymentReApprove CheckPaymentReApprove = new CheckPaymentReApprove();

                    string Message = CheckPaymentReApprove.Check_ContractChanges(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                    if (Message != "")
                    {
                        Message += "\n" + "Are You Sure Want To Change The Data..";
                        if (MessageBox.Show(Message, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            UpdatePaymentReApprove.Update_ContractChanges(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                        }
                    }

                    Message = CheckPaymentReApprove.Check_ValueUP(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                    if (Message != "")
                    {
                        Message += "\n" + "Are You Sure Want To Change The Data..";
                        if (MessageBox.Show(Message, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            UpdatePaymentReApprove.Update_ValueUP(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                        }
                    }

                    Message = CheckPaymentReApprove.Check_ValueDown(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                    if (Message != "")
                    {
                        Message += "\n" + "Are You Sure Want To Change The Data..";
                        if (MessageBox.Show(Message, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            UpdatePaymentReApprove.Update_ValueDown(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                        }
                    }

                    Message += CheckPaymentReApprove.Check_CurrencyChange(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                    if (Message != "")
                    {
                        Message += "\n" + "Are You Sure Want To Change The Data..";
                        if (MessageBox.Show(Message, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            UpdatePaymentReApprove.Update_CurrencyChange(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                        }
                    }

                    Message = CheckPaymentReApprove.Check_DateChange(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                    if (Message != "")
                    {
                        Message += "\n" + "Are You Sure Want To Change The Data..";
                        if (MessageBox.Show(Message, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            UpdatePaymentReApprove.Update_DateChange(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                        }
                    }

                    Message = "";
                    Message = CheckMovementReApprove.CheckQuantityUP(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);

                    if (Message != "")
                    {
                        Message += "\n" + "Are You Sure Want To Change The Data..";
                        if (MessageBox.Show(Message, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            UpdateMovementReApprove.QuantityUP(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                        }
                    }
                    Message = CheckMovementReApprove.CheckQuantityDown(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);

                    if (Message != "")
                    {
                        Message += "\n" + "Are You Sure Want To Change The Data..";
                        if (MessageBox.Show(Message, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            UpdateMovementReApprove.QuantityDown(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                        }
                    }
                    Message = CheckMovementReApprove.CheckDateChange(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);

                    if (Message != "")
                    {
                        Message += "\n" + "Are You Sure Want To Change The Data..";
                        if (MessageBox.Show(Message, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            UpdateMovementReApprove.DateChange(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                        }
                    }

                    Message = CheckMovementReApprove.CheckNewMovement(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);

                    if (Message != "")
                    {
                        Message += "\n" + "Are You Sure Want To Change The Data..";
                        if (MessageBox.Show(Message, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            UpdateMovementReApprove.NewMovement(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                        }
                    }
                    Message = CheckMovementReApprove.CheckDeleteMovement(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);

                    if (Message != "")
                    {
                        Message += "\n" + "Are You Sure Want To Change The Data..";
                        if (MessageBox.Show(Message, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            UpdateMovementReApprove.DeleteMovement(SalesInvoiceDB, sales_invoice.id_sales_invoice, entity.App.Names.SalesInvoice);
                        }
                    }
                }
                //SalesInvoiceDB.ReApprove(sales_invoice);
                sales_invoiceViewSource.View.Refresh();
                SalesInvoiceDB.SaveChanges();
            }
            else
            {
                app_document_range app_document_range = cbxDocument.SelectedItem as app_document_range;
                if (app_document_range != null)
                {
                    if (app_document_range.range_current > app_document_range.range_end)
                    {
                        toolBar.msgWarning("Document Range is finished.");
                        return;
                    }
                }

                if (SalesInvoiceDB.SaveChanges() > 0)
                {
                    sales_invoiceViewSource.View.Refresh();
                    toolBar.msgSaved(SalesInvoiceDB.NumberOfRecords);
                }
            }
        }

        private void toolBar_btnCancel_Click(object sender)
        {
            sales_invoiceViewSource.View.MoveCurrentToFirst();
            SalesInvoiceDB.CancelAllChanges();

            if (sales_invoiceViewSource.View != null)
                sales_invoiceViewSource.View.Refresh();
        }

        private void btnApprove_Click(object sender)
        {
            Settings SalesSettings = new Settings();

            Class.CreditLimit Limit = new Class.CreditLimit();
            foreach (sales_invoice sales_invoice in SalesInvoiceDB.sales_invoice.Local.Where(x => x.IsSelected))
            {
                sales_invoice.app_currencyfx = SalesInvoiceDB.app_currencyfx.Find(sales_invoice.id_currencyfx);
                Limit.Check_CreditAvailability(sales_invoice);
            }

            if (SalesInvoiceDB.Approve(SalesSettings.DiscountStock))
            {
                cbxDocument.ItemsSource = entity.Brillo.Logic.Range.List_Range(SalesInvoiceDB, entity.App.Names.SalesInvoice, CurrentSession.Id_Branch, CurrentSession.Id_Terminal);
                cbxDocument.SelectedIndex = 0;
            }
            else
            {
                toolBar.msgWarning("Please check Customer's Credit");
            }

            load_PrimaryDataThread(null, null);
        }

        private void toolBar_btnAnull_Click(object sender)
        {
            sales_invoice sales_invoice = sales_invoiceDataGrid.SelectedItem as sales_invoice;
            if (sales_invoice != null)
            {
                sales_invoice.status = Status.Documents_General.Annulled;
                crud_modalAnull.Visibility = Visibility.Visible;
                cntrl.PanelAdv.ActionPanelAnull ActionPanelAnull = new cntrl.PanelAdv.ActionPanelAnull();
                ActionPanelAnull.ID = sales_invoice.id_sales_invoice;
                ActionPanelAnull.Application = entity.App.Names.SalesInvoice;
                ActionPanelAnull.db = SalesInvoiceDB;
                crud_modalAnull.Children.Add(ActionPanelAnull);
            }
        }

        #endregion "Action Events"

        #region Filter Data

        private async void set_ContactPref(object sender, EventArgs e)
        {
            if (sbxContact.ContactID > 0)
            {
                contact contact = await SalesInvoiceDB.contacts.Where(x => x.id_contact == sbxContact.ContactID).FirstOrDefaultAsync();

                //Empty so that memory does not bring incorrect currency calculation
                contact.Check_CreditAvailability();
                sales_invoice sales_invoice = (sales_invoice)sales_invoiceDataGrid.SelectedItem;
                sales_invoice.contact = contact;
                sales_invoice.id_contact = contact.id_contact;

                if (sales_invoice.sales_order == null)
                {
                    ///Start Thread to get Data.
                    Task thread_SecondaryData = Task.Factory.StartNew(() => set_ContactPref_Thread(contact));
                }
            }
        }

        private async void set_ContactPref_Thread(contact objContact)
        {
            if (objContact != null)
            {
                await Dispatcher.InvokeAsync(new Action(() =>
                {
                    cbxContactRelation.ItemsSource = SalesInvoiceDB.contacts.Where(x => x.parent.id_contact == objContact.id_contact).ToList();

                    if (objContact.id_sales_rep != null)
                        cbxSalesRep.SelectedValue = Convert.ToInt32(objContact.id_sales_rep);
                    //Condition
                    if (objContact.app_contract != null)
                        cbxCondition.SelectedValue = objContact.app_contract.id_condition;
                    //Contract
                    if (objContact.id_contract != null)
                        cbxContract.SelectedValue = Convert.ToInt32(objContact.id_contract);
                    //Currency

                    cbxCurrency.get_ActiveRateXContact(ref objContact);

                    if (cbxCondition.SelectedItem != null && cbxContract.SelectedItem != null && cbxCurrency.SelectedValue > 0)
                    {
                        sbxItem.Focus();
                    }
                }));
            }
        }

        private void cbxCondition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxCondition.SelectedItem != null)
            {
                app_condition app_condition = cbxCondition.SelectedItem as app_condition;
                cbxContract.ItemsSource = CurrentSession.Contracts.Where(x => x.id_condition == app_condition.id_condition).ToList();

                sales_invoice sales_invoice = (sales_invoice)sales_invoiceDataGrid.SelectedItem;

                if (sales_invoice != null)
                {
                    if (sales_invoice.id_contract == 0)
                    {
                        cbxContract.SelectedIndex = 0;
                    }
                }
            }
        }

        #endregion Filter Data

        private void calculate_vat(object sender, EventArgs e)
        {
            sales_invoice sales_invoice = (sales_invoice)sales_invoiceDataGrid.SelectedItem;

            if (sales_invoice != null)
            {
                sales_invoice.RaisePropertyChanged("GrandTotal");
                List<sales_invoice_detail> sales_invoice_detail = sales_invoice.sales_invoice_detail.ToList();
                if (sales_invoice_detail.Count > 0)
                {
                    var listvat = sales_invoice_detail
                           .Join(SalesInvoiceDB.app_vat_group_details, ad => ad.id_vat_group, cfx => cfx.id_vat_group
                               , (ad, cfx) => new { name = cfx.app_vat.name, value = ad.unit_price * (cfx.app_vat.coefficient * cfx.percentage), id_vat = cfx.app_vat.id_vat, ad })
                               .GroupBy(a => new { a.name, a.id_vat, a.ad })
                       .Select(g => new
                       {
                           id_vat = g.Key.id_vat,
                           name = g.Key.name,
                           value = g.Sum(a => a.value * a.ad.quantity)
                       }).ToList();

                    dgvVAT.ItemsSource = listvat.GroupBy(x => x.id_vat).Select(g => new
                    {
                        id_vat = g.Max(y => y.id_vat),
                        name = g.Max(y => y.name),
                        value = g.Sum(a => a.value)
                    }).ToList();
                }
            }
        }

        private void sales_invoice_detailDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            calculate_vat(sender, e);
        }

        #region PrefSettings

        private void tbCustomize_MouseUp(object sender, MouseButtonEventArgs e)
        {
            popupCustomize.PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade;
            popupCustomize.StaysOpen = false;
            popupCustomize.IsOpen = true;
        }

        private void popupCustomize_Closed(object sender, EventArgs e)
        {
            Settings SalesSettings = new Settings();

            popupCustomize.PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade;

            Settings.Default.Save();
            SalesSettings = Settings.Default;
            popupCustomize.IsOpen = false;
        }

        #endregion PrefSettings

        private void sales_invoice_detailDataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            sales_invoice_detail sales_invoice_detail = (sales_invoice_detail)e.NewItem;
            sales_invoice sales_invoice = (sales_invoice)sales_invoiceDataGrid.SelectedItem;

            sales_invoice_detail.sales_invoice = sales_invoice;
        }

        private void item_Select(object sender, EventArgs e)
        {
            if (sbxItem.ItemID > 0)
            {
                Settings SalesSettings = new Settings();
                sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;

                if (sales_invoice != null)
                {
                    int LineLimit = 0;

                    if (sales_invoice.id_range > 0)
                    {
                        app_document_range app_document_range = SalesInvoiceDB.app_document_range.Find(sales_invoice.id_range);
                        if (app_document_range.app_document.line_limit != null)
                        {
                            LineLimit = (int)app_document_range.app_document.line_limit;
                        }
                    }

                    if (SalesSettings.BlockExcessItem == true && LineLimit > 0 && sales_invoice.sales_invoice_detail.Count + 1 > LineLimit)
                    {
                        toolBar.msgWarning("Your Item Limit is Exceed");
                    }
                    else
                    {
                        int BranchID = (int)cbxBranch.SelectedValue;

                        item item = SalesInvoiceDB.items.Find(sbxItem.ItemID);
                        item_product item_product = item.item_product.FirstOrDefault();

                        if (item_product != null && item_product.can_expire)
                        {
                            crud_modalExpire.Visibility = Visibility.Visible;
                            cntrl.Panels.pnl_ItemMovementExpiry pnl_ItemMovementExpiry = new cntrl.Panels.pnl_ItemMovementExpiry(sales_invoice.id_branch, null, item.item_product.FirstOrDefault().id_item_product);
                            crud_modalExpire.Children.Add(pnl_ItemMovementExpiry);
                        }
                        else
                        {
                            sales_invoice_detail _sales_invoice_detail = SalesInvoiceDB.Select_Item(ref sales_invoice, item, sbxItem.QuantityInStock, SalesSettings.AllowDuplicateItem, null, sbxItem.Quantity);
                            sales_invoicesales_invoice_detailViewSource.View.Refresh();
                            sales_invoice.RaisePropertyChanged("GrandTotal");
                        }
                    }
                }
            }
        }

        private void toolBar_btnSearch_Click(object sender, string query)
        {
            if (!string.IsNullOrEmpty(query) && sales_invoiceViewSource != null)
            {
                sales_invoiceViewSource.View.Filter = i =>
                {
                    sales_invoice sales_invoice = i as sales_invoice;
                    contact contact = sales_invoice.contact != null ? sales_invoice.contact : null;

                    if (sales_invoice != null)
                    {
                        //Protect the code against null values.
                        string number = sales_invoice.number != null ? sales_invoice.number : "";
                        string customer = "";
                        string cust_code = "";
                        string cust_gov_code = "";

                        if (contact != null)
                        {
                            if (contact.name != null)
                            {
                                customer = contact.name.ToLower();
                            }
                            if (contact.code != null)
                            {
                                cust_code = contact.code.ToLower();
                            }
                            if (contact.gov_code != null)
                            {
                                cust_gov_code = contact.gov_code.ToLower();
                            }
                        }

                        if (customer.Contains(query.ToLower())
                            ||
                            cust_code.Contains(query.ToLower())
                            ||
                            cust_gov_code.Contains(query.ToLower())
                            ||
                            number.Contains(query))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                };
            }
            else
            {
                sales_invoiceViewSource.View.Filter = null;
            }
        }

        private void DeleteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter as sales_invoice_detail != null)
            {
                e.CanExecute = true;
            }
        }

        private void DeleteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Are you sure want to Delete?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
                    //DeleteDetailGridRow
                    dgvSalesDetail.CancelEdit();
                    SalesInvoiceDB.sales_invoice_detail.Remove(e.Parameter as sales_invoice_detail);
                    sales_invoicesales_invoice_detailViewSource.View.Refresh();
                }
            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
            }
        }

        private void cbxCurrency_LostFocus(object sender, RoutedEventArgs e)
        {
            sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
            if (sales_invoice != null)
            {
                if (sales_invoice.id_currencyfx > 0)
                {
                    app_currencyfx app_currencyfx = SalesInvoiceDB.app_currencyfx.Where(x => x.id_currencyfx == sales_invoice.id_currencyfx).FirstOrDefault();
                    if (app_currencyfx != null)
                    {
                        sales_invoice.app_currencyfx = app_currencyfx;
                    }
                }
            }
            calculate_vat(sender, e);
        }

        private void sales_invoice_detailDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            calculate_vat(sender, e);
        }

        private void btnPackingList_Click(object sender, RoutedEventArgs e)
        {
            sales_invoice sales_invoice = (sales_invoice)sales_invoiceDataGrid.SelectedItem;

            if (sales_invoice != null)
            {
                crud_modal.Visibility = Visibility.Visible;

                pnlPacking = new cntrl.PanelAdv.pnlPacking();
                pnlPacking._entity = SalesInvoiceDB;
                pnlPacking._contact = SalesInvoiceDB.contacts.Where(x => x.id_contact == sbxContact.ContactID).FirstOrDefault(); //sbxContact.Contact as contact;
                pnlPacking.Link_Click += Link_Click;
                crud_modal.Children.Add(pnlPacking);
                CollectionViewSource sales_invoicesales_invoice_detailViewSource = FindResource("sales_invoicesales_invoice_detailViewSource") as CollectionViewSource;
                sales_invoicesales_invoice_detailViewSource.View.Refresh();
            }
            else
            {
                toolBar.msgWarning("Check Sales Invoice");
            }
        }

        public void Link_Click(object sender)
        {
            sales_invoice _sales_invoice = (sales_invoice)sales_invoiceViewSource.View.CurrentItem;
            foreach (sales_packing item in pnlPacking.selected_sales_packing)
            {
                sales_packing sales_packing = SalesInvoiceDB.sales_packing.Where(x => x.id_sales_packing == item.id_sales_packing).FirstOrDefault();

                foreach (sales_packing_detail _sales_packing_detail in sales_packing.sales_packing_detail.Where(x=>x.user_verified))
                {
                    sales_order_detail sales_order_detail = _sales_packing_detail.sales_order_detail;
                    sales_invoice_detail sales_invoice_detail = new sales_invoice_detail();

                    sales_packing_relation sales_packing_relation = new sales_packing_relation();
                    sales_packing_relation.id_sales_packing_detail = _sales_packing_detail.id_sales_packing_detail;
                    sales_packing_relation.sales_packing_detail = _sales_packing_detail;
                    sales_invoice_detail.sales_packing_relation.Add(sales_packing_relation);

                    //if SalesOrder Exists, use it for Price and VAT.
                    if (sales_order_detail != null)
                    {
                        sales_invoice_detail.sales_invoice = _sales_invoice;
                        sales_invoice_detail.Contact = SalesInvoiceDB.contacts.Find(sbxContact.ContactID);// sbxContact.Contact;
                        sales_invoice_detail.item = _sales_packing_detail.item;
                        sales_invoice_detail.id_item = _sales_packing_detail.id_item;
                        sales_invoice_detail.quantity = _sales_packing_detail.quantity;
                        sales_invoice_detail.id_vat_group = sales_order_detail.id_vat_group;
                        sales_invoice_detail.State = EntityState.Added;
                        sales_invoice_detail.unit_price = sales_order_detail.unit_price + sales_order_detail.discount;
                        sales_invoice_detail.discount = sales_order_detail.discount;

                        _sales_invoice.sales_invoice_detail.Add(sales_invoice_detail);
                    }
                    else
                    {
                        //If Sales Order does not exist, use Price and VAT From standard of the company.
                        SalesInvoiceDB.AddDetail(ref _sales_invoice, _sales_packing_detail.item, 0, null, (decimal)_sales_packing_detail.verified_quantity);
                    }
                }

                CollectionViewSource sales_invoicesales_invoice_detailViewSource = FindResource("sales_invoicesales_invoice_detailViewSource") as CollectionViewSource;
                sales_invoicesales_invoice_detailViewSource.View.Refresh();
                sales_invoicesales_invoice_detailViewSource.View.MoveCurrentToFirst();
            }

            CollectionViewSource sales_invoicesales_invoice_detailsales_packinglist_relationViewSource = FindResource("sales_invoicesales_invoice_detailsales_packinglist_relationViewSource") as CollectionViewSource;

            if (sales_invoicesales_invoice_detailsales_packinglist_relationViewSource != null)
            {
                sales_invoicesales_invoice_detailsales_packinglist_relationViewSource.Source = SalesInvoiceDB.sales_packing_relation.Local.Where(x => x.sales_invoice_detail.id_sales_invoice == _sales_invoice.id_sales_invoice).ToList();
            }
            else
            {
                sales_invoicesales_invoice_detailsales_packinglist_relationViewSource.Source = null;
            }

            crud_modal.Children.Clear();
            crud_modal.Visibility = Visibility.Collapsed;
            _sales_invoice.RaisePropertyChanged("GrandTotal");
        }

        private void btnSalesOrder_Click(object sender, RoutedEventArgs e)
        {
            crud_modal.Visibility = Visibility.Visible;
            pnlSalesOrder = new cntrl.PanelAdv.pnlSalesOrder();
            pnlSalesOrder._entity = SalesInvoiceDB;
            pnlSalesOrder._contact = SalesInvoiceDB.contacts.Where(x => x.id_contact == sbxContact.ContactID).FirstOrDefault(); // sbxContact.Contact;
            pnlSalesOrder.mode = cntrl.PanelAdv.pnlSalesOrder.module.sales_invoice;
            pnlSalesOrder.SalesOrder_Click += SalesOrder_Click;
            crud_modal.Children.Add(pnlSalesOrder);
        }

        public void SalesOrder_Click(object sender)
        {
            sales_invoice _sales_invoice = (sales_invoice)sales_invoiceViewSource.View.CurrentItem;

            sbxContact.Text = pnlSalesOrder.selected_sales_order.FirstOrDefault().contact.name;
            foreach (sales_order sales_order in pnlSalesOrder.selected_sales_order)
            {
                _sales_invoice.State = EntityState.Modified;
                _sales_invoice.contact = sales_order.contact;

                cbxContactRelation.ItemsSource = SalesInvoiceDB.contacts.Where(x => x.parent.id_contact == sales_order.contact.id_contact).ToList();

                _sales_invoice.id_contact = sales_order.contact.id_contact;
                _sales_invoice.id_condition = sales_order.id_condition;
                _sales_invoice.id_contract = sales_order.id_contract;
                _sales_invoice.id_currencyfx = sales_order.id_currencyfx;
                _sales_invoice.app_currencyfx = sales_order.app_currencyfx;
                _sales_invoice.id_sales_order = sales_order.id_sales_order;
                _sales_invoice.id_project = sales_order.id_project;
                _sales_invoice.id_sales_rep = sales_order.id_sales_rep;

                foreach (sales_order_detail _sales_order_detail in sales_order.sales_order_detail)
                {
                    sales_invoice_detail sales_invoice_detail = new sales_invoice_detail();

                    //There is an issue that the detail does not know of the currency previously selected. Maybe this can help.
                    sales_invoice_detail.CurrencyFX_ID = sales_order.app_currencyfx.id_currencyfx;

                    sales_invoice_detail.id_sales_order_detail = _sales_order_detail.id_sales_order_detail;
                    sales_invoice_detail.sales_order_detail = _sales_order_detail;
                    sales_invoice_detail.id_project_task = _sales_order_detail.id_project_task;
                    sales_invoice_detail.Contact = _sales_invoice.contact;
                    sales_invoice_detail.sales_invoice = _sales_invoice;
                    sales_invoice_detail.item = _sales_order_detail.item;
                    sales_invoice_detail.id_item = _sales_order_detail.id_item;
                    sales_invoice_detail.quantity = _sales_order_detail.quantity - SalesInvoiceDB.sales_invoice_detail
                                                                                 .Where(x => x.id_sales_order_detail == _sales_order_detail.id_sales_order_detail)
                                                                                 .GroupBy(x => x.id_sales_order_detail)
                                                                                 .Select(x => x.Sum(y => y.quantity))
                                                                                 .FirstOrDefault();
                    sales_invoice_detail.id_vat_group = _sales_order_detail.id_vat_group;
                    sales_invoice_detail.unit_price = _sales_order_detail.unit_price;
                    sales_invoice_detail.movement_id = _sales_order_detail.movement_id;
                    if (_sales_order_detail.expire_date != null || !string.IsNullOrEmpty(_sales_order_detail.batch_code))
                    {
                        sales_invoice_detail.expire_date = _sales_order_detail.expire_date;
                        sales_invoice_detail.batch_code = _sales_order_detail.batch_code;
                    }
                    if (sales_invoice_detail.quantity > 0)
                    {
                        _sales_invoice.sales_invoice_detail.Add(sales_invoice_detail);
                    }
                }

                SalesInvoiceDB.Entry(_sales_invoice).Entity.State = EntityState.Added;
                crud_modal.Children.Clear();
                crud_modal.Visibility = Visibility.Collapsed;
                sales_invoiceViewSource.View.Refresh();
                sales_invoicesales_invoice_detailViewSource.View.Refresh();
            }
            _sales_invoice.RaisePropertyChanged("GrandTotal");
        }

        private void salesorder_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Hyperlink Hyperlink = (Hyperlink)sender;
            sales_order sales_order = (sales_order)Hyperlink.Tag;
            if (sales_order != null)
            {
                entity.Brillo.Document.Start.Automatic(sales_order, sales_order.app_document_range);
            }
        }

        private void salespackinglist_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Hyperlink Hyperlink = (Hyperlink)sender;
            sales_packing sales_packing = (sales_packing)Hyperlink.Tag;
            if (sales_packing != null)
            {
                entity.Brillo.Document.Start.Automatic(sales_packing, sales_packing.app_document_range);
            }
        }

        private void toolBar_btnPrint_Click(object sender, MouseButtonEventArgs e)
        {
            sales_invoice sales_invoice = sales_invoiceDataGrid.SelectedItem as sales_invoice;
            if (sales_invoice != null && sales_invoice.status == Status.Documents_General.Approved)
            {
                entity.Brillo.Document.Start.Manual(sales_invoice, sales_invoice.app_document_range);
            }
            else
            {
                toolBar.msgWarning("Please a valid select record");
            }
        }

        private void sales_invoiceDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sales_invoice sales_invoice = sales_invoiceDataGrid.SelectedItem as sales_invoice;

            CollectionViewSource sales_invoicesales_invoice_detailsales_packinglist_relationViewSource = FindResource("sales_invoicesales_invoice_detailsales_packinglist_relationViewSource") as CollectionViewSource;

            if (sales_invoicesales_invoice_detailsales_packinglist_relationViewSource != null)
            {
                if (sales_invoice != null)
                {
                    sales_invoice.RaisePropertyChanged("GrandTotal");
                    int id_sales_invoice = sales_invoice.id_sales_invoice;
                    sales_invoicesales_invoice_detailsales_packinglist_relationViewSource.Source = SalesInvoiceDB.sales_packing_relation.Where(x => x.sales_invoice_detail.id_sales_invoice == id_sales_invoice).ToList();
                }
            }
            else
            {
                sales_invoicesales_invoice_detailsales_packinglist_relationViewSource.Source = null;
            }

            //calculate_vat(sender, e);
        }

        private void btnRecivePayment_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            sales_invoice sales_invoice = sales_invoiceDataGrid.SelectedItem as sales_invoice;
            if (sales_invoice != null)
            {
                entity.Brillo.Security Security = new entity.Brillo.Security(entity.App.Names.AccountsReceivable);
                if (Security.create)
                {
                    crud_modal.Visibility = Visibility.Visible;
                    cntrl.Curd.receive_payment recive_payment = new cntrl.Curd.receive_payment();
                    recive_payment.sales_invoice = sales_invoice;
                    crud_modal.Children.Add(recive_payment);
                }
                else
                {
                    toolBar.msgWarning("Access Denied. Please contact your Administrator.");
                }
            }
        }

        private async void btnAccountsRecievable_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
            List<payment_schedual> payment_schedualList = await SalesInvoiceDB.payment_schedual
                     .Where(x => x.id_payment_detail == null && x.id_company == CurrentSession.Id_Company && x.id_contact == sales_invoice.id_contact
                         && (x.id_sales_invoice > 0 || x.id_sales_order > 0) && x.id_note == null
                         && (x.debit - (x.child.Count() > 0 ? x.child.Sum(y => y.credit) : 0)) > 0)
                         .OrderBy(x => x.expire_date).ToListAsync();

            payment_schedualDataGrid.ItemsSource = payment_schedualList.GroupBy(x => x.id_currencyfx).Select(x => new { CustName = x.Max(s => s.contact.name), AccountReceivableBalance = x.Sum(y => y.AccountReceivableBalance), Currency = x.Max(z => z.app_currencyfx.app_currency.name) });
            crud_modalDuePaymnet.Visibility = Visibility.Visible;
        }

        private void btnGridSearch(object sender, RoutedEventArgs e)
        {
            load_PrimaryDataThread(null, null);
        }

        private void cbxBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxBranch.SelectedItem != null)
            {
                app_branch app_branch = cbxBranch.SelectedItem as app_branch;
                cbxLocation.ItemsSource = CurrentSession.Locations.Where(x => x.id_branch == app_branch.id_branch).ToList();
            }
        }

        private void lblCancel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            crud_modalDuePaymnet.Visibility = Visibility.Collapsed;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (SalesInvoiceDB != null)
            {
                if (disposing)
                {
                    SalesInvoiceDB.Dispose();
                    // Dispose other managed resources.
                }
                //release unmanaged resources.
            }
        }

        private void lblCheckCredit(object sender, RoutedEventArgs e)
        {
            if (sales_invoiceViewSource != null)
            {
                sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
                sales_invoice.app_currencyfx = SalesInvoiceDB.app_currencyfx.Find(sales_invoice.id_currencyfx);
                Class.CreditLimit Limit = new Class.CreditLimit();
                Limit.Check_CreditAvailability(sales_invoice);
            }
        }

        private void toolBar_btnReturn_Click(object sender, MouseButtonEventArgs e)
        {
            sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
            if (sales_invoice != null && sales_invoice.status == Status.Documents_General.Approved && sales_invoice.sales_return.Count() == 0)
            {
                sales_return sales_return = new sales_return();
                sales_return.barcode = sales_invoice.barcode;
                sales_return.code = sales_invoice.code;
                sales_return.trans_date = DateTime.Now;
                sales_return.comment = sales_invoice.comment;
                sales_return.id_condition = sales_invoice.id_condition;
                sales_return.id_contact = sales_invoice.id_contact;
                sales_return.contact = sales_invoice.contact;
                sales_return.id_contract = sales_invoice.id_contract;
                sales_return.id_currencyfx = sales_invoice.id_currencyfx;
                sales_return.id_project = sales_invoice.id_project;
                sales_return.id_sales_rep = sales_invoice.id_sales_rep;
                sales_return.id_weather = sales_invoice.id_weather;
                sales_return.is_impex = sales_invoice.is_impex;
                sales_return.sales_invoice = sales_invoice;
                foreach (sales_invoice_detail sales_invoice_detail in sales_invoice.sales_invoice_detail)
                {
                    sales_return_detail sales_return_detail = new sales_return_detail();
                    sales_return_detail.comment = sales_invoice_detail.comment;
                    sales_return_detail.discount = sales_invoice_detail.discount;
                    sales_return_detail.id_item = sales_invoice_detail.id_item;
                    sales_return_detail.item_description = sales_invoice_detail.item_description;
                    sales_return_detail.id_location = sales_invoice_detail.id_location;
                    sales_return_detail.id_project_task = sales_invoice_detail.id_project_task;
                    sales_return_detail.sales_invoice_detail = sales_invoice_detail;
                    sales_return_detail.id_vat_group = sales_invoice_detail.id_vat_group;
                    sales_return_detail.quantity = sales_invoice_detail.quantity - sales_invoice_detail.sales_return_detail.Sum(x => x.quantity);
                    sales_return_detail.unit_cost = sales_invoice_detail.unit_cost;
                    sales_return_detail.unit_price = sales_invoice_detail.unit_price;
                    sales_return_detail.movement_id = sales_invoice_detail.movement_id;
                    if (sales_invoice_detail.expire_date != null || !string.IsNullOrEmpty(sales_invoice_detail.batch_code))
                    {
                        sales_return_detail.expire_date = sales_invoice_detail.expire_date;
                        sales_return_detail.batch_code = sales_invoice_detail.batch_code;
                    }
                    sales_return.sales_return_detail.Add(sales_return_detail);
                }

                SalesInvoiceDB.sales_return.Add(sales_return);
                crm_opportunity crm_opportunity = sales_invoice.crm_opportunity;
                crm_opportunity.sales_return.Add(sales_return);
                SalesInvoiceDB.SaveChanges();
                MessageBox.Show("Return Created Successfully ..");
            }
            else
            {
                MessageBox.Show("Return Already Created Or Status is Not Approved..");
            }
        }

        private void Refinance_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            sales_invoice sales_invoice = sales_invoiceDataGrid.SelectedItem as sales_invoice;
            if (sales_invoice != null)
            {
                entity.Brillo.Security Security = new entity.Brillo.Security(entity.App.Names.AccountsReceivable);
                if (Security.create)
                {
                    crud_modal.Visibility = Visibility.Visible;
                    cntrl.Curd.RefinanceSales RefinanceSales = new cntrl.Curd.RefinanceSales();
                    RefinanceSales.sales_invoice = sales_invoice;
                    crud_modal.Children.Add(RefinanceSales);
                }
                else
                {
                    toolBar.msgWarning("Access Denied. Please contact your Administrator.");
                }
            }
        }

        private void crud_modalExpire_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (crud_modalExpire.Visibility == Visibility.Collapsed || crud_modalExpire.Visibility == Visibility.Hidden)
            {
                sales_invoice sales_invoice = sales_invoiceDataGrid.SelectedItem as sales_invoice;
                item item = SalesInvoiceDB.items.Find(sbxItem.ItemID);

                cntrl.Panels.pnl_ItemMovementExpiry pnl_ItemMovementExpiry = crud_modalExpire.Children.OfType<cntrl.Panels.pnl_ItemMovementExpiry>().FirstOrDefault();

                if (item != null && item.id_item > 0 && sales_invoice != null)
                {
                    Settings SalesSettings = new Settings();

                    if (pnl_ItemMovementExpiry.MovementID > 0)
                    {
                        item_movement item_movement = SalesInvoiceDB.item_movement.Find(pnl_ItemMovementExpiry.MovementID);

                        SalesInvoiceDB.Select_Item(ref sales_invoice, item, sbxItem.QuantityInStock, SalesSettings.AllowDuplicateItem, item_movement, sbxItem.Quantity);
                        sales_invoicesales_invoice_detailViewSource.View.Refresh();
                        sales_invoice.RaisePropertyChanged("GrandTotal");
                    }
                    else
                    {
                        toolBar.msgWarning("Batch not selected correctly.");
                    }
                }

                //Cleans for reuse.
                crud_modalExpire.Children.Clear();
            }
        }

        private void chbxRowDetail_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chbx = sender as CheckBox;
            if ((bool)chbx.IsChecked)
            {
                dgvSalesDetail.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            }
            else
            {
                dgvSalesDetail.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            }
        }

    
    }
}