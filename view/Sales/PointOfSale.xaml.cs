﻿using entity;
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
using System.Windows.Input;
using System.Windows.Threading;

namespace Cognitivo.Sales
{
    public partial class PointOfSale : Page
    {
        private CollectionViewSource sales_invoiceViewSource;
        private CollectionViewSource paymentViewSource;

        private entity.Controller.Sales.InvoiceController SalesDB;
        private entity.Controller.Finance.Payment PaymentDB;

        public PointOfSale()
        {
            InitializeComponent();

            SalesDB = FindResource("SalesDB") as entity.Controller.Sales.InvoiceController;
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                //Load Controller.
                SalesDB.Initialize();
                SalesDB.LoadPromotion();

                cbxLocation.IsEnabled = CurrentSession.UserRole.is_master ? true : false;
                cbxLocation.ItemsSource = CurrentSession.Locations.Where(x => x.id_branch == CurrentSession.Id_Branch).ToList();
                cbxDocument.ItemsSource = entity.Brillo.Logic.Range.List_Range(SalesDB.db, entity.App.Names.PointOfSale, CurrentSession.Id_Branch, CurrentSession.Id_Terminal);
            }

            PaymentDB = FindResource("PaymentDB") as entity.Controller.Finance.Payment;
            //Share DB to increase efficiency.
            PaymentDB.db = SalesDB.db;
        }

        #region ActionButtons

        /// <summary>
        /// Navigates to CLIENT Tab
        /// </summary>
        private void Client_Click(object sender, EventArgs e)
        {
            tabContact.IsSelected = true;
        }

        /// <summary>
        /// Navigates to ACCOUNT UTILITY Tab
        /// </summary>
        private void Account_Click(object sender, EventArgs e)
        {
            frmaccount.Navigate(new Configs.AccountActive(CurrentSession.Id_Account));
            tabAccount.IsSelected = true;
            tabAccount.Focus();
        }

        private void Sales_Click(object sender, EventArgs e)
        {
            tabSales.IsSelected = true;
        }

        private void Payment_Click(object sender, EventArgs e)
        {
            tabPayment.IsSelected = true;
            Promotion_Click(sender, e);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            CollectionViewSource paymentpayment_detailViewSource = FindResource("paymentpayment_detailViewSource") as CollectionViewSource;
            sales_invoice sales_invoice = (sales_invoice)sales_invoiceViewSource.View.CurrentItem as sales_invoice;
            payment payment = paymentViewSource.View.CurrentItem as payment;

            /// VALIDATIONS...
            ///
            /// Validates if Contact is not assigned, then it will take user to the Contact Tab.
            if (sales_invoice.contact == null)
            {
                tabContact.Focus();
                return;
            }

            /// Validates if Sales Detail has 0 rows, then take you to Sales Tab.
            if (sales_invoice.sales_invoice_detail.Count == 0)
            {
                tabSales.Focus();
                return;
            }

            /// Validate Payment <= Sales.GrandTotal
            if (payment.GrandTotalDetail < Math.Round(sales_invoice.GrandTotal, 2))
            {
                tabPayment.Focus();
                return;
            }

            //if (payment.GrandTotalDetail > Math.Round(sales_invoice.GrandTotal, 2))
            //{
            //    tabPayment.Focus();
            //    return;
            //}

            /// If all validation is met, then we can start Sales Process.
            if (sales_invoice.contact != null && sales_invoice.sales_invoice_detail.Count > 0)
            {
                sales_invoice.trans_date = DateTime.Now;
                sales_invoice.IsSelected = true;
                ///Approve Sales Invoice.
                ///Note> Approve includes Save Logic. No need to seperately Save.
                ///Plus we are passing True as default because in Point of Sale, we will always discount Stock.
                bool ApprovalStatus = SalesDB.Approve();

                if (ApprovalStatus)
                {

                    if (sales_invoice.TotalChanged > 0)
                    {
                        if (true)
                        {
                            decimal ChangeAmount = sales_invoice.TotalChanged;

                            List<payment_detail> SameCurrency_Payments = paymentpayment_detailViewSource.View.OfType<payment_detail>().ToList().Where(x => x.id_currencyfx == sales_invoice.id_currencyfx).ToList();
                            foreach (var detail in SameCurrency_Payments)
                            {
                                payment_detail payment_detail = paymentpayment_detailViewSource.View.OfType<payment_detail>().ToList().Where(x => x.id_currencyfx == sales_invoice.id_currencyfx).FirstOrDefault();
                                payment_detail.value = payment_detail.value - sales_invoice.TotalChanged;

                                //ChangeAmount -= ChangeAmount - 
                            }

                            //if (ChangeAmount > 0)
                            //{
                            //    List<payment_detail> DiffCurrency_Payments = paymentpayment_detailViewSource.View.OfType<payment_detail>().ToList().Where(x => x.id_currencyfx != sales_invoice.id_currencyfx).ToList();
                            //    foreach (var detail in SameCurrency_Payments)
                            //    {
                            //        payment_detail payment_detail = paymentpayment_detailViewSource.View.OfType<payment_detail>().ToList().Where(x => x.id_currencyfx == sales_invoice.id_currencyfx).FirstOrDefault();
                            //        payment_detail.value = payment_detail.value - sales_invoice.TotalChanged;
                            //    }
                            //}
                        }
                    }

                    //if (SalesDB.db.payment_schedual.Where(x => x.id_sales_invoice == sales_invoice.id_sales_invoice && x.debit > 0).ToList())
                    //{
                    //    payment_detail payment_detail = paymentpayment_detailViewSource.View.OfType<payment_detail>().ToList().Where(x => x.id_currencyfx == sales_invoice.id_currencyfx).FirstOrDefault();
                    //    payment_detail.value = payment_detail.value - sales_invoice.TotalChanged;
                    //}

                    List<payment_schedual> payment_schedualList = SalesDB.db.payment_schedual.Where(x => x.id_sales_invoice == sales_invoice.id_sales_invoice && x.debit > 0).ToList();
                    PaymentDB.Approve(payment_schedualList, true, (bool)chkreceipt.IsChecked);

                    //Start New Sale
                    New_Sale_Payment();
                }
                else
                {
                    if (SalesDB.Msg.Count() > 0)
                    {
                        MessageBox.Show(SalesDB.Msg.FirstOrDefault().ToString());
                    }
                }
            }
        }

        private void New_Sale_Payment()
        {
            ///Creating new SALES INVOICE for upcomming sale.
            ///TransDate = 0 because in Point of Sale we are assuming sale will always be done today.
            Settings SalesSettings = new Settings();

            sales_invoice sales_invoice = SalesDB.Create(SalesSettings.TransDate_Offset, false);
            sales_invoice.id_sales_rep = CurrentSession.SalesReps.FirstOrDefault().id_sales_rep;
            sales_invoice.Location = CurrentSession.Locations.Where(x => x.id_location == Settings.Default.Location).FirstOrDefault();
            app_document_range app_document_range = SalesDB.db.app_document_range.Where(x => x.id_company == CurrentSession.Id_Company && x.app_document.id_application == entity.App.Names.PointOfSale && x.is_active).FirstOrDefault();
            if (app_document_range != null)
            {
                sales_invoice.id_range = app_document_range.id_range;
                sales_invoice.RaisePropertyChanged("id_range");
                sales_invoice.app_document_range = app_document_range;
            }
            SalesDB.db.sales_invoice.Add(sales_invoice);

            Dispatcher.BeginInvoke((Action)(() =>
            {
                sales_invoiceViewSource = FindResource("sales_invoiceViewSource") as CollectionViewSource;
                sales_invoiceViewSource.Source = SalesDB.db.sales_invoice.Local;
                sales_invoiceViewSource.View.MoveCurrentTo(sales_invoice);
            }));


            ///Creating new PAYMENT for upcomming sale.
            payment payment = PaymentDB.New(true);
            payment.id_currencyfx = sales_invoice.id_currencyfx;
            PaymentDB.db.payments.Add(payment);

            Dispatcher.BeginInvoke((Action)(() =>
            {
                paymentViewSource = FindResource("paymentViewSource") as CollectionViewSource;
                paymentViewSource.Source = PaymentDB.db.payments.Local;
                paymentViewSource.View.MoveCurrentTo(payment);

                tabContact.Focus();
                sbxContact.Text = "";
            }));
        }

        #endregion ActionButtons

        #region SmartBox Selection

        private async void Contact_Select(object sender, RoutedEventArgs e)
        {
            if (sbxContact.ContactID > 0)
            {
                contact contact = await SalesDB.db.contacts.FindAsync(sbxContact.ContactID);
                if (contact != null)
                {
                    sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
                    payment payment = paymentViewSource.View.CurrentItem as payment;
                    sales_invoice.id_contact = contact.id_contact;
                    sales_invoice.contact = contact;
                  
                    if (contact.id_contract !=null)
                    {
                        sales_invoice.id_contract = (int)contact.id_contract;
                    }
                    if (contact.id_sales_rep != null)
                    {
                        sales_invoice.id_sales_rep = contact.id_sales_rep;
                    }
                    sales_invoiceViewSource.View.Refresh();

                    payment.id_contact = contact.id_contact;
                }
            }
        }

        private async void Item_Select(object sender, RoutedEventArgs e)
        {
            Settings SalesSettings = new Settings();
            if (sbxItem.ItemID > 0)
            {
                if (sales_invoiceViewSource.View.CurrentItem is sales_invoice sales_invoice)
                {
                    item item = await SalesDB.db.items.FindAsync(sbxItem.ItemID);
                    item_product item_product = item.item_product.FirstOrDefault();

                    if (item.id_item_type == item.item_type.ItemReceipe)
                    {
                        item_recepie ItemReceipe = item.item_recepie.FirstOrDefault();
                        if (ItemReceipe != null)
                        {
                            foreach (item_recepie_detail item_recepie_detail in ItemReceipe.item_recepie_detail)
                            {
                                OpenExpireModal(ref sales_invoice, item_recepie_detail.item, sales_invoice.id_branch, SalesSettings.AllowDuplicateItem);
                            }
                        }

                    }
                    else
                    {
                        OpenExpireModal(ref sales_invoice, item, sales_invoice.id_branch, SalesSettings.AllowDuplicateItem);
                    }
                }
            }
        }

        private void OpenExpireModal(ref sales_invoice sales_invoice, item item, int id_branch, bool AllowDuplicateItem)
        {


            item_product item_product = item.item_product.FirstOrDefault();
            if (item_product != null && item_product.can_expire)
            {
                System.Windows.Controls.Grid crudGrid = new Grid();
                crudGrid.Visibility = Visibility.Visible;
                cntrl.Panels.pnl_ItemMovementExpiry pnl_ItemMovementExpiry = new cntrl.Panels.pnl_ItemMovementExpiry(id_branch, null, item.item_product.FirstOrDefault().id_item_product);
                crudGrid.Children.Add(pnl_ItemMovementExpiry);
                crudGrid.SetValue(Grid.RowProperty, 3);
                crudGrid.SetValue(Grid.ColumnProperty, 2);
                crudGrid.IsVisibleChanged += new DependencyPropertyChangedEventHandler(Expire_IsVisibleChanged);
                MainGrid.Children.Add(crudGrid);
            }
            else
            {
                decimal QuantityInStock = sbxItem.QuantityInStock;
                sales_invoice_detail _sales_invoice_detail =
                      SalesDB.Create_Detail(ref sales_invoice, item, null,
                        new Settings().AllowDuplicateItem,
                        sbxItem.QuantityInStock,
                        sbxItem.Quantity);
                if (sales_invoice.Location != null)
                {
                    _sales_invoice_detail.id_location = sales_invoice.Location.id_location;
                }


            }
            sales_invoiceViewSource.View.Refresh();
            CollectionViewSource sales_invoicesales_invoice_detailViewSource = FindResource("sales_invoicesales_invoice_detailViewSource") as CollectionViewSource;
            sales_invoicesales_invoice_detailViewSource.View.Refresh();
            paymentViewSource.View.Refresh();
        }

        #endregion SmartBox Selection

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SalesDB.Initialize();

            New_Sale_Payment();

            //PAYMENT TYPE
            await SalesDB.db.payment_type
                .Where(a => a.is_active == true && a.id_company == CurrentSession.Id_Company && a.payment_behavior == payment_type.payment_behaviours.Normal)
                .LoadAsync();
            CollectionViewSource payment_typeViewSource = FindResource("payment_typeViewSource") as CollectionViewSource;
            payment_typeViewSource.Source = SalesDB.db.payment_type.Local;

            cbxSalesRep.ItemsSource = CurrentSession.SalesReps;
            

            CollectionViewSource app_currencyViewSource = FindResource("app_currencyViewSource") as CollectionViewSource;
            app_currencyViewSource.Source = CurrentSession.Currencies;

            int Id_Account = CurrentSession.Id_Account;
            app_account app_account = await SalesDB.db.app_account.FindAsync(CurrentSession.Id_Account);

            if (app_account != null)
            {
                //If Account Session has 1 cl_date as null, means Account is still open. If False, means account is closed.
                if (app_account.app_account_session.Where(x => x.cl_date == null).Any() == false)
                {
                    Account_Click(null, null);
                }
            }
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                tabContact.IsSelected = true;
            }
            else if (e.Key == Key.F2)
            {
                tabSales.IsSelected = true;
            }
            else if (e.Key == Key.F3)
            {
                tabPayment.IsSelected = true;
            }
            else if (e.Key == Key.F12)
            {
                Save_Click(sender, e);
            }
        }

        private void Cancel_MouseDown(object sender, EventArgs e)
        {
            SalesDB.CancelAllChanges();
            PaymentDB.CancelAllChanges();

            New_Sale_Payment();

            //Clean up Contact Data.
            sbxContact.Text = "";
            sbxContact.ContactID = 0;

            sbxItem.Text = "";
            sbxItem.ItemID = 0;

            tabContact.IsSelected = true;
        }

        #region Details

        private void PaymentDetail_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            payment payment = paymentViewSource.View.CurrentItem as payment;

            payment_detail payment_detail = e.NewItem as payment_detail;
            if (payment_detail != null && payment != null && sales_invoiceViewSource.View.CurrentItem as sales_invoice != null)
            {
                payment_detail.State = EntityState.Added;
                payment_detail.IsSelected = true;
                payment_detail.Default_id_currencyfx = CurrentSession.Get_Currency_Default_Rate().id_currencyfx;
                payment_detail.id_currencyfx = CurrentSession.Get_Currency_Default_Rate().id_currencyfx;
                payment_detail.id_currency = CurrentSession.Currency_Default.id_currency;

                payment_detail.id_payment = payment.id_payment;
                payment_detail.payment = payment;
            }
        }

        private void DeleteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter as sales_invoice_detail != null)
            {
                e.CanExecute = true;
            }
            else if (e.Parameter as payment_detail != null)
            {
                e.CanExecute = true;
            }
        }

        private void DeleteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(entity.Brillo.Localize.Question_Delete, "Cognitivo ERP", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                sales_invoice_detail sales_invoice_detail = e.Parameter as sales_invoice_detail;
                payment_detail payment_detail = e.Parameter as payment_detail;

                dgvSalesDetail.CommitEdit();
                dgvPaymentDetail.CommitEdit();

                if (sales_invoice_detail != null)
                {
                    if (sales_invoiceViewSource.View.CurrentItem is sales_invoice sales_invoice)
                    {
                        SalesDB.db.sales_invoice_detail.Remove(sales_invoice_detail);

                        if (FindResource("sales_invoicesales_invoice_detailViewSource") is CollectionViewSource sales_invoicesales_invoice_detailViewSource)
                        {
                            if (sales_invoicesales_invoice_detailViewSource.View != null)
                            {
                                sales_invoicesales_invoice_detailViewSource.View.Refresh();
                            }
                        }
                    }
                }
                else if (payment_detail != null)
                {
                    if (paymentViewSource.View.CurrentItem is payment payment)
                    {
                        PaymentDB.db.payment_detail.Remove(payment_detail);
                        paymentViewSource.View.Refresh();

                        if (FindResource("paymentpayment_detailViewSource") is CollectionViewSource paymentpayment_detailViewSource)
                        {
                            if (paymentpayment_detailViewSource.View != null)
                            {
                                paymentpayment_detailViewSource.View.Refresh();
                            }
                        }
                    }
                }
            }
        }

        #endregion Details

        private void GrandTotalsales_DataContextChanged(object sender, EventArgs e)
        {
            if (sales_invoiceViewSource != null && paymentViewSource != null)
            {
                if (sales_invoiceViewSource.View != null && paymentViewSource.View != null)
                {
                    if (sales_invoiceViewSource.View.CurrentItem != null && paymentViewSource.View.CurrentItem != null)
                    {
                        (paymentViewSource.View.CurrentItem as payment).GrandTotal = (sales_invoiceViewSource.View.CurrentItem as sales_invoice).GrandTotal;
                    }
                }
            }
        }

        private void Clear_MouseDown(object sender, EventArgs e)
        {
            if (sales_invoiceViewSource != null && paymentViewSource != null)
            {
                if (sales_invoiceViewSource.View != null && paymentViewSource.View != null)
                {
                    if (sales_invoiceViewSource.View.CurrentItem != null && paymentViewSource.View.CurrentItem != null)
                    {
                        sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
                        if (sales_invoice.GrandTotal > 0)
                        {
                            decimal TrailingDecimals = sales_invoice.GrandTotal - Math.Floor(sales_invoice.GrandTotal);
                            sales_invoice.DiscountWithoutPercentage += TrailingDecimals;
                        }
                    }
                }
            }
        }

        private void Promotion_Click(object sender, EventArgs e)
        {
            if (sales_invoiceViewSource.View.CurrentItem is sales_invoice Invoice)
            {
                SalesDB.Check_Promotions(Invoice);

                CollectionViewSource sales_invoicesales_invoice_detailViewSource = (CollectionViewSource)this.FindResource("sales_invoicesales_invoice_detailViewSource");
                sales_invoicesales_invoice_detailViewSource.View.Refresh();
                Invoice.RaisePropertyChanged("GrandTotal");
            }
        }

        private void Expire_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Grid crud_modal = sender as Grid;
            if (crud_modal.Visibility == Visibility.Collapsed || crud_modal.Visibility == Visibility.Hidden)
            {
                sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
                item item = SalesDB.db.items.Find(sbxItem.ItemID);

                cntrl.Panels.pnl_ItemMovementExpiry pnl_ItemMovementExpiry = crud_modal.Children.OfType<cntrl.Panels.pnl_ItemMovementExpiry>().FirstOrDefault();

                if (item != null && item.id_item > 0 && sales_invoice != null)
                {

                    if (pnl_ItemMovementExpiry.MovementID > 0)
                    {
                        Settings SalesSettings = new Settings();

                        item_movement item_movement = SalesDB.db.item_movement.Find(pnl_ItemMovementExpiry.MovementID);
                        if (item_movement != null)
                        {
                            if (item.id_item_type == item.item_type.ItemReceipe)
                            {
                                item_product item_product = SalesDB.db.item_product.Find(pnl_ItemMovementExpiry.ProductID);
                                if (item_product != null)
                                {
                                    item_recepie ItemReceipe = item.item_recepie.FirstOrDefault();
                                    if (ItemReceipe != null)
                                    {
                                        foreach (item_recepie_detail item_recepie_detail in ItemReceipe.item_recepie_detail)
                                        {
                                            if (item_recepie_detail.item.id_item == item_product.id_item)
                                            {
                                                decimal QuantityInStock = item_movement.avlquantity;

                                                sales_invoice_detail _sales_invoice_detail =
                                                      SalesDB.Create_Detail(ref sales_invoice, item_recepie_detail.item, null,
                                                        SalesSettings.AllowDuplicateItem,
                                                        sbxItem.QuantityInStock,
                                                        sbxItem.Quantity * item_recepie_detail.quantity);
                                                _sales_invoice_detail.Quantity_InStockLot = QuantityInStock;
                                                _sales_invoice_detail.batch_code = item_movement.code;
                                                _sales_invoice_detail.expire_date = item_movement.expire_date;
                                                //Update Grand Total
                                                (sales_invoiceViewSource.View.CurrentItem as sales_invoice).RaisePropertyChanged("GrandTotal");
                                            }

                                        }
                                    }
                                }
                            }
                            else
                            {
                                decimal QuantityInStock = item_movement.avlquantity;

                                sales_invoice_detail _sales_invoice_detail =
                                      SalesDB.Create_Detail(ref sales_invoice, item, null,
                                        SalesSettings.AllowDuplicateItem,
                                        sbxItem.QuantityInStock,
                                        sbxItem.Quantity);
                                _sales_invoice_detail.Quantity_InStockLot = QuantityInStock;
                                _sales_invoice_detail.batch_code = item_movement.code;
                                _sales_invoice_detail.expire_date = item_movement.expire_date;
                                //Update Grand Total
                                (sales_invoiceViewSource.View.CurrentItem as sales_invoice).RaisePropertyChanged("GrandTotal");
                            }
                        }
                    }
                }

                sales_invoiceViewSource.View.Refresh();
                CollectionViewSource sales_invoicesales_invoice_detailViewSource = FindResource("sales_invoicesales_invoice_detailViewSource") as CollectionViewSource;
                sales_invoicesales_invoice_detailViewSource.View.Refresh();
                paymentViewSource.View.Refresh();

                //Cleans for reuse.
                crud_modal.Children.Clear();
            }
        }

        private void dgvPaymentDetail_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            sales_invoice sales_invoice = (sales_invoice)sales_invoiceViewSource.View.CurrentItem as sales_invoice;
            payment payment = paymentViewSource.View.CurrentItem as payment;
            if (payment != null && sales_invoice != null)
            {
                sales_invoice.TotalChanged = Math.Round(payment.GrandTotalDetail - sales_invoice.GrandTotal) < 0 ? 0 : Math.Round(payment.GrandTotalDetail - sales_invoice.GrandTotal);
                sales_invoice.RaisePropertyChanged("TotalChanged");
            }

        }

        private void Expander_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Settings.Default.Save();

        }

        private void Qty_LostFocus(object sender, RoutedEventArgs e)
        {
            CollectionViewSource sales_invoicesales_invoice_detailViewSource = (CollectionViewSource)this.FindResource("sales_invoicesales_invoice_detailViewSource");
            sales_invoice_detail detail = sales_invoicesales_invoice_detailViewSource.View.CurrentItem as sales_invoice_detail;

            if (detail != null)
            {
                if (detail.item != null && detail.Quantity_InStock > 0)
                {
                    if (detail.quantity > detail.Quantity_InStock &&
                        (
                        detail.item.id_item_type == item.item_type.Product ||
                        detail.item.id_item_type == item.item_type.RawMaterial ||
                        detail.item.id_item_type == item.item_type.Supplies)
                        )
                    {
                        detail.quantity = (int)detail.Quantity_InStock;
                        detail.RaisePropertyChanged("quantity");
                        MessageBox.Show("Quantity Exceeded. Reverting back to " + detail.Quantity_InStock.ToString(), "Cognitivo ERP", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void dgvSalesDetail_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            (sales_invoiceViewSource.View.CurrentItem as sales_invoice).RaisePropertyChanged("GrandTotal");
        }
    }
}