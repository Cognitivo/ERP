﻿using entity;
using System;
using System.Collections.Generic;
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
    public partial class PackingList : Page
    {
        private PackingListDB PackingListDB = new PackingListDB();

        private CollectionViewSource sales_packingViewSource, sales_packingsales_packinglist_detailViewSource, sales_packingsales_packing_detailVerifiedViewSource;
        private cntrl.PanelAdv.pnlSalesOrder pnlSalesOrder;
        private cntrl.Panels.pnl_ItemMovementExpiry pnl_ItemMovementExpiry;

        public PackingList()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            sales_packingViewSource = FindResource("sales_packingViewSource") as CollectionViewSource;
            await PackingListDB.sales_packing.Where(a => a.id_company == CurrentSession.Id_Company)
                .Include(x => x.contact)
                .OrderByDescending(x=>x.trans_date)
                .LoadAsync();
            sales_packingViewSource.Source = PackingListDB.sales_packing.Local;

            sales_packingsales_packinglist_detailViewSource = FindResource("sales_packingsales_packing_detailViewSource") as CollectionViewSource;
            sales_packingsales_packing_detailVerifiedViewSource = FindResource("sales_packingsales_packing_detailVerifiedViewSource") as CollectionViewSource;

            CollectionViewSource app_branchViewSource = FindResource("app_branchViewSource") as CollectionViewSource;
            app_branchViewSource.Source = await PackingListDB.app_branch.Where(a => a.is_active && a.id_company == CurrentSession.Id_Company).OrderBy(a => a.name).ToListAsync();

            CollectionViewSource item_assetViewSource = FindResource("item_assetViewSource") as CollectionViewSource;
            item_assetViewSource.Source = await PackingListDB.item_asset.Where(a => a.id_company == CurrentSession.Id_Company).Include(x => x.item).OrderBy(a => a.item.name).ToListAsync();

            CollectionViewSource app_terminalViewSource = FindResource("app_terminalViewSource") as CollectionViewSource;
            app_terminalViewSource.Source = await PackingListDB.app_terminal.Where(a => a.is_active && a.id_company == CurrentSession.Id_Company).OrderBy(a => a.name).ToListAsync();
            await PackingListDB.app_measurement.Where(a => a.is_active && a.id_company == CurrentSession.Id_Company).OrderBy(a => a.name).LoadAsync();

            CollectionViewSource app_measurevolume = FindResource("app_measurevolume") as CollectionViewSource;
            CollectionViewSource app_measureweight = FindResource("app_measureweight") as CollectionViewSource;
            app_measurevolume.Source = PackingListDB.app_measurement.Local;
            app_measureweight.Source = PackingListDB.app_measurement.Local;

            await Dispatcher.InvokeAsync(new Action(() =>
            {
                cbxDocument.ItemsSource = entity.Brillo.Logic.Range.List_Range(PackingListDB, entity.App.Names.PackingList, CurrentSession.Id_Branch, CurrentSession.Id_Terminal);
                cbxPackingType.ItemsSource = Enum.GetValues(typeof(Status.PackingTypes));

                if (sales_packingsales_packinglist_detailViewSource.View != null)
                {
                    sales_packingsales_packinglist_detailViewSource.View.Refresh();
                }
                if (sales_packingsales_packing_detailVerifiedViewSource.View != null)
                {
                    sales_packingsales_packing_detailVerifiedViewSource.View.Refresh();
                }
                filterDetail();
            }));
            Refresh_GroupByGrid();
        }

        private void filterDetail()
        {
            if (sales_packingsales_packinglist_detailViewSource != null)
            {
                if (sales_packingsales_packinglist_detailViewSource.View != null)
                {
                    sales_packingsales_packinglist_detailViewSource.View.Filter = i =>
                    {
                        sales_packing_detail sales_packing_detail = (sales_packing_detail)i;
                        if (sales_packing_detail.user_verified == false)
                            return true;
                        else
                            return false;
                    };
                }
            }
        }

        private void filterVerifiedDetail(int id_item)
        {
            if (sales_packingsales_packing_detailVerifiedViewSource != null)
            {
                if (sales_packingsales_packing_detailVerifiedViewSource.View != null)
                {
                    sales_packingsales_packing_detailVerifiedViewSource.View.Filter = i =>
                    {
                        sales_packing_detail sales_packing_detail = (sales_packing_detail)i;
                        if (id_item > 0)
                        {
                            if ((sales_packing_detail.user_verified == true || sales_packing_detail.parent != null) && sales_packing_detail.id_item == id_item)
                                return true;
                            else
                                return false;
                        }
                        else
                        {
                            if (sales_packing_detail.user_verified == true || sales_packing_detail.parent != null)
                                return true;
                            else
                                return false;
                        }
                    };
                }
            }
        }

        #region Toolbar Events

        private void toolBar_btnNew_Click(object sender)
        {
            Settings SalesSettings = new Settings();

            sales_packing sales_packing = PackingListDB.New();
            sales_packing.trans_date = DateTime.Now.AddDays(SalesSettings.TransDate_Offset);

            PackingListDB.sales_packing.Add(sales_packing);
            GridVerifiedList.ItemsSource = null;
            sales_packingViewSource.View.Refresh();
            sales_packingViewSource.View.MoveCurrentToLast();

        }

        private void toolBar_btnEdit_Click(object sender)
        {
            if (sales_packingDataGrid.SelectedItem != null)
            {
                sales_packing sales_packing = (sales_packing)sales_packingDataGrid.SelectedItem;
                sales_packing.IsSelected = true;
                sales_packing.State = EntityState.Modified;
                PackingListDB.Entry(sales_packing).State = EntityState.Modified;
                sales_packingViewSource.View.Refresh();
            }
            else
            {
                toolBar.msgWarning("Please Select an Item");
            }
        }

        private void toolBar_btnDelete_Click(object sender)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure want to Delete?", "Cognitivo", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                PackingListDB.sales_packing.Remove((sales_packing)sales_packingViewSource.View.CurrentItem);
                sales_packingViewSource.View.MoveCurrentToFirst();
                toolBar_btnSave_Click(sender);
            }
        }

        private void toolBar_btnSave_Click(object sender)
        {
            if (PackingListDB.SaveChanges() > 0)
            {
                sales_packingViewSource.View.Refresh();
                toolBar.msgSaved(PackingListDB.NumberOfRecords);
            }
        }

        private void toolBar_btnCancel_Click(object sender)
        {
            sales_packinglist_detailDataGrid.CancelEdit();
            sales_packingViewSource.View.MoveCurrentToFirst();
            PackingListDB.CancelAllChanges();
            if (sales_packingsales_packinglist_detailViewSource.View != null)
                sales_packingsales_packinglist_detailViewSource.View.Refresh();

            sbxContact.Text = "";
            filterDetail();
            filterVerifiedDetail(0);
        }

        #endregion Toolbar Events

        private void popupCustomize_Closed(object sender, EventArgs e)
        {
            popupCustomize.PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade;
            Settings.Default.Save();
            popupCustomize.IsOpen = false;
        }

        private void tbCustomize_MouseUp(object sender, MouseButtonEventArgs e)
        {
            popupCustomize.PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade;
            popupCustomize.StaysOpen = false;
            popupCustomize.IsOpen = true;
        }

        private void Hyperlink_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            crud_modal.Visibility = Visibility.Visible;
            cntrl.Curd.contact contact = new cntrl.Curd.contact();
            crud_modal.Children.Add(contact);
        }

        private void btnApprove_Click(object sender)
        {
            PackingListDB.Approve();
        }

        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter as sales_packing_detail != null)
            {
                e.CanExecute = true;
            }
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Are you sure want to Delete?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    //DeleteDetailGridRow
                    sales_packing_detail sales_packing_detail = e.Parameter as sales_packing_detail;
                    if (sales_packing_detail.Error == null)
                    {
                        PackingListDB.sales_packing_detail.Remove(sales_packing_detail);
                        Refresh_GroupByGrid();
                        GridVerifiedList.SelectedIndex = 0;
                        sales_packingViewSource.View.Refresh();
                        sales_packingsales_packinglist_detailViewSource.View.Refresh();
                    }
                    else
                    {
                        toolBar.msgWarning(sales_packing_detail.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
            }
        }

        private void toolBar_btnAnull_Click(object sender)
        {
            PackingListDB.Annull();
        }

        private void btnSalesOrder_Click(object sender, RoutedEventArgs e)
        {
            crud_modal.Visibility = Visibility.Visible;
            pnlSalesOrder = new cntrl.PanelAdv.pnlSalesOrder();
            pnlSalesOrder.db = PackingListDB;
            if (sbxContact.ContactID > 0)
            {
                contact contact = PackingListDB.contacts.Where(x => x.id_contact == sbxContact.ContactID).FirstOrDefault();
                pnlSalesOrder._contact = contact;
            }
            pnlSalesOrder.mode = cntrl.PanelAdv.pnlSalesOrder.module.packing_list;
            pnlSalesOrder.SalesOrder_Click += SalesOrder_Click;
            crud_modal.Children.Add(pnlSalesOrder);
        }

        public void SalesOrder_Click(object sender)
        {
            sales_packing sales_packing = (sales_packing)sales_packingViewSource.View.CurrentItem;

            if (sales_packing != null)
            {
                foreach (sales_order sales_order in pnlSalesOrder.selected_sales_order)
                {
                    foreach (sales_order_detail sales_order_detail in
                        sales_order.sales_order_detail.
                        Where(x => x.item != null && x.item.item_product.Count() > 0))
                    {
                        sales_packing_detail sales_packing_detail = new sales_packing_detail()
                        {
                            id_sales_order_detail = sales_order_detail.id_sales_order_detail,
                            id_item = sales_order_detail.id_item,
                            app_location = PackingListDB.app_location.Where(x => x.id_branch == sales_packing.id_branch && x.is_active && x.is_default).FirstOrDefault(),
                            id_location = PackingListDB.app_location.Where(x => x.id_branch == sales_packing.id_branch && x.is_active && x.is_default).FirstOrDefault().id_location,
                            item = sales_order_detail.item,
                            user_verified = false,
                            id_movement = sales_order_detail.movement_id,
                            quantity = sales_order_detail.quantity
                        };

                        if (sales_order_detail.batch_code != "")
                        {
                            sales_packing_detail.batch_code = sales_order_detail.batch_code;
                        }

                        if (sales_order_detail.expire_date != null)
                        {
                            sales_packing_detail.expire_date = sales_order_detail.expire_date;
                        }

                        sales_packing.sales_packing_detail.Add(sales_packing_detail);

                        sales_packingsales_packinglist_detailViewSource.View.Refresh();
                        PackingListDB.Entry(sales_packing).Entity.State = EntityState.Added;
                        crud_modal.Children.Clear();
                        crud_modal.Visibility = Visibility.Collapsed;
                        //filterVerifiedDetail();
                        filterDetail();
                    }
                    Refresh_GroupByGrid();
                    GridVerifiedList.SelectedIndex = 0;
                }
            }
        }

        private void Item_Select(object sender, EventArgs e)
        {
            AddItem(sbxItem.ItemID, sbxItem.Quantity);
        }

        public void AddItem(int ItemID, decimal Quantity)
        {
            app_branch app_branch = null;
            if (ItemID > 0)
            {
                sales_packing sales_packing = sales_packingViewSource.View.CurrentItem as sales_packing;

                item item = PackingListDB.items.Where(x => x.id_item == ItemID).FirstOrDefault();

                if (item != null && item.id_item > 0 && sales_packing != null)
                {
                    if (cbxBranch.SelectedItem != null)
                    { app_branch = cbxBranch.SelectedItem as app_branch; }

                    item_product item_product = item.item_product.FirstOrDefault();

                    if (item_product != null && item_product.can_expire)
                    {
                        crud_modalExpire.Visibility = Visibility.Visible;
                        pnl_ItemMovementExpiry = new cntrl.Panels.pnl_ItemMovementExpiry(sales_packing.id_branch, null, item.item_product.FirstOrDefault().id_item_product);

                        crud_modalExpire.Children.Add(pnl_ItemMovementExpiry);
                    }
                    else
                    {
                        Task Thread = Task.Factory.StartNew(() => select_Item(sales_packing, item, app_branch, null, Quantity));
                    }
                }
            }
        }
        private void select_Item(sales_packing sales_packing, item item, app_branch app_branch, item_movement item_movement, decimal quantity)
        {
            long id_movement = item_movement != null ? item_movement.id_movement : 0;

            if (sales_packing.sales_packing_detail.Where(a => a.id_item == item.id_item && a.id_movement == id_movement && a.user_verified).FirstOrDefault() == null)
            {
                sales_packing_detail _sales_packing_detail = new sales_packing_detail();
                _sales_packing_detail.sales_packing = sales_packing;
                _sales_packing_detail.item = item;
                _sales_packing_detail.verified_quantity = quantity;
                _sales_packing_detail.id_item = item.id_item;
                _sales_packing_detail.user_verified = true;




                sales_packing_detail sales_packing_detail = sales_packing.sales_packing_detail.Where(a => a.id_item == item.id_item && a.user_verified == false).FirstOrDefault();
                _sales_packing_detail.parent = sales_packing_detail;


                if (sales_packing_detail != null)
                {
                    if (sales_packing_detail.sales_packing_relation.Count() > 0)
                    {
                        sales_packing_relation PackingRelation = sales_packing_detail.sales_packing_relation.FirstOrDefault();
                        sales_packing_relation sales_packing_relation = new sales_packing_relation()
                        {
                            //id_sales_invoice_detail = PackingRelation.id_sales_invoice_detail,
                            sales_invoice_detail = PackingRelation.sales_invoice_detail,
                            id_sales_packing_detail = PackingRelation.id_sales_packing_detail,
                            sales_packing_detail = PackingRelation.sales_packing_detail
                        };
                        _sales_packing_detail.sales_packing_relation.Add(sales_packing_relation);
                    }
                    _sales_packing_detail.id_sales_order_detail = sales_packing_detail.id_sales_order_detail;
                    sales_packing_detail.verified_quantity = 1;
                    sales_packing.sales_packing_detail.Add(_sales_packing_detail);

                }

                if (item_movement != null)
                {
                    _sales_packing_detail.batch_code = item_movement.code;
                    _sales_packing_detail.expire_date = item_movement.expire_date;
                    _sales_packing_detail.id_movement = (int)item_movement.id_movement;
                    _sales_packing_detail.id_location = (int)item_movement.id_location;
                    _sales_packing_detail.app_location = PackingListDB.app_location.Where(x => x.id_location == (int)item_movement.id_location).FirstOrDefault();
                    _sales_packing_detail.Quantity_InStockLot = item_movement.avlquantity;

                }

                if (app_branch != null)
                {
                    if (_sales_packing_detail.id_location == null)
                    {
                        _sales_packing_detail.id_location = app_branch.app_location.Where(x => x.is_default).FirstOrDefault().id_location;
                        _sales_packing_detail.app_location = app_branch.app_location.Where(x => x.is_default).FirstOrDefault();
                    }

                }

            }
            else
            {
                sales_packing_detail sales_packing_detail = sales_packing.sales_packing_detail.Where(a => a.id_item == item.id_item).FirstOrDefault();
                sales_packing_detail.verified_quantity += 1;
            }

            Dispatcher.BeginInvoke((Action)(() =>
            {
                sales_packingsales_packinglist_detailViewSource.View.Refresh();
                sales_packingsales_packing_detailVerifiedViewSource.View.Refresh();
                //filterVerifiedDetail();
                filterDetail();
                Refresh_GroupByGrid();
            }));
        }

        private void Refresh_GroupByGrid()
        {
            sales_packing sales_packing = sales_packingViewSource.View.CurrentItem as sales_packing;
            if (sales_packing != null)
            {
                if (sales_packing.sales_packing_detail.Count() > 0)
                {
                    //This code should be in Selection Changed of DataGrid and after inserting new items.
                    var VerifiedItemList = sales_packing.sales_packing_detail
                    .Where(x => x.user_verified == false)
                    .GroupBy(x => x.id_item)
                    .Select(x => new
                    {

                        ItemName = x.Max(y => y.item.name),
                        ItemCode = x.Max(y => y.item.code),
                        VerifiedQuantity = sales_packing.sales_packing_detail.Where(y => y.user_verified && y.id_item == x.Max(z => z.id_item)).Sum(y => y.verified_quantity), //Only sum Verified Quantity if IsVerifiyed is True.
                        Quantity = x.Sum(y => y.quantity),
                        id_item = x.Max(y => y.id_item)
                    })
                    .ToList();
                    GridVerifiedList.ItemsSource = VerifiedItemList;
                }
            }
        }

        private void sales_packingDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filterDetail();
            filterVerifiedDetail(0);
            Refresh_GroupByGrid();
            GridVerifiedList.SelectedIndex = 0;
        }

        private void GridVerifiedList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic obj = GridVerifiedList.SelectedItem;
            if (obj != null)
            {
                filterVerifiedDetail(obj.id_item);
            }
        }

        #region Filter Data

        private void set_ContactPrefKeyStroke(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                set_ContactPref(sender, e);
            }
        }

        private void set_ContactPref(object sender, EventArgs e)
        {
            try
            {
                if (sbxContact.ContactID > 0)
                {
                    contact contact = PackingListDB.contacts.Where(x => x.id_contact == sbxContact.ContactID).FirstOrDefault();
                    sales_packing sales_packing = (sales_packing)sales_packingDataGrid.SelectedItem;
                    sales_packing.id_contact = contact.id_contact;
                    sales_packing.contact = contact;
                }
            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
            }
        }

        #endregion Filter Data

        private void salesorder_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Hyperlink Hyperlink = (Hyperlink)sender;
            sales_order sales_order = (sales_order)Hyperlink.Tag;
            if (sales_order != null)
            {
                entity.Brillo.Document.Start.Manual(sales_order, sales_order.app_document_range);
            }
        }

        private void toolBar_btnPrint_Click(object sender, MouseButtonEventArgs e)
        {
            sales_packing sales_packing = sales_packingDataGrid.SelectedItem as sales_packing;
            if (sales_packing != null)
            {
                entity.Brillo.Document.Start.Manual(sales_packing, sales_packing.app_document_range);
            }
            else
            {
                toolBar.msgWarning(entity.Brillo.Localize.PleaseSelect);
            }
        }

        private void cbxBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxBranch.SelectedItem != null)
            {
                app_branch app_branch = cbxBranch.SelectedItem as app_branch;
                if (app_branch != null)
                {
                    cbxLocation.ItemsSource = app_branch.app_location.ToList();
                }
            }
        }

        private void Search_Click(object sender, string query)
        {
            if (!string.IsNullOrEmpty(query))
            {
                sales_packingViewSource.View.Filter = i =>
                {
                    sales_packing Packing = i as sales_packing;
                    string Name = Packing.contact != null ? Packing.contact.name : "";
                    string Number = Packing.number != null ? Packing.number : "";

                    if (Name.ToLower().Contains(query.ToLower()) || Number.ToLower().Contains(query.ToLower()))
                    {
                        return true;
                    }

                    return false;
                };
            }
            else
            {
                sales_packingViewSource.View.Filter = null;
            }
        }

        private void toolBar_btnFocus_Click(object sender)
        {
            sales_packingViewSource = FindResource("sales_packingViewSource") as CollectionViewSource;
            sales_packingViewSource.Source = PackingListDB.sales_packing.Where(x => x.id_sales_packing == toolBar.ref_id).ToList();
        }
        private void toolBar_btnClear_Click(object sender)
        {

            sales_packingViewSource = FindResource("sales_packingViewSource") as CollectionViewSource;
            PackingListDB.sales_packing.Where(a => a.id_company == CurrentSession.Id_Company)
               .Include(x => x.contact)
               .LoadAsync();
            sales_packingViewSource.Source = PackingListDB.sales_packing.Local;
        }

        private void Item_Select(object sender, RoutedEventArgs e)
        {
            dynamic _selectedpacking = GridVerifiedList.SelectedItem;
            if (_selectedpacking != null)
            {
                AddItem(_selectedpacking.id_item, 1);
            }
        }

        private void btnSalesInvoice_Click(object sender, MouseButtonEventArgs e)
        {
            sales_packing packing = sales_packingViewSource.View.CurrentItem as sales_packing;
            if (packing != null)
            {
                if (packing.status == Status.Documents_General.Approved)
                {
                    List<sales_invoice_detail> DetailList = new List<sales_invoice_detail>();
                    //For now I only want to bring items not verified. Mainly because I want to prevent duplciating items in Purchase Invoice.
                    //I would like to some how check for inconsistancies or let user check for them before approving.

                    foreach (sales_packing_detail PackingDetail in packing.sales_packing_detail.Where(x => x.user_verified == true))
                    {
                        sales_invoice_detail detail = new sales_invoice_detail()
                        {
                            item = PackingDetail.item,
                            item_description = PackingDetail.sales_order_detail.item_description,
                            quantity = Convert.ToDecimal(PackingDetail.verified_quantity),
                            unit_cost = PackingDetail.sales_order_detail.unit_cost,
                            discount = PackingDetail.sales_order_detail.discount,
                            id_vat_group = PackingDetail.sales_order_detail.id_vat_group,
                            sales_order_detail = PackingDetail.sales_order_detail,
                            id_location = PackingDetail.sales_order_detail.id_location,
                            unit_price = PackingDetail.sales_order_detail.unit_price + PackingDetail.sales_order_detail.discount
                        };
                        if (PackingDetail.expire_date != null || !string.IsNullOrEmpty(PackingDetail.batch_code))
                        {
                            detail.expire_date = PackingDetail.expire_date;
                            detail.batch_code = PackingDetail.batch_code;
                        }
                        sales_packing_relation sales_packing_relation = new sales_packing_relation()
                        {
                            //id_sales_invoice_detail = detail.id_sales_invoice_detail,
                            sales_invoice_detail = detail,
                            id_sales_packing_detail = PackingDetail.id_sales_packing_detail,
                            sales_packing_detail = PackingDetail
                        };

                        PackingListDB.sales_packing_relation.Add(sales_packing_relation);
                        DetailList.Add(detail);
                    }

                    //Only if Details have been added, should we include the Purchase Invoice into Context.
                    if (DetailList.Count() > 0)
                    {
                        sales_order Order = PackingListDB.sales_order.Find(DetailList.FirstOrDefault().sales_order_detail.id_sales_order);
                        if (Order != null)
                        {
                            sales_invoice _sales_invoice = new sales_invoice()
                            {
                                contact = packing.contact,
                                id_contact = packing.id_contact,
                                app_branch = packing.app_branch,
                                app_contract = Order.app_contract,
                                app_condition = Order.app_condition,
                                id_contract = Order.id_contract,
                                id_condition = Order.id_condition,
                                id_currencyfx = Order.id_currencyfx,
                                trans_date = Order.trans_date,
                                timestamp = DateTime.Now
                            };

                            foreach (var item in DetailList)
                            {
                                _sales_invoice.sales_invoice_detail.Add(item);
                            }
                            PackingListDB.sales_invoice.Add(_sales_invoice);
                            crm_opportunity crm_opportunity = Order.crm_opportunity;
                            _sales_invoice.crm_opportunity = crm_opportunity;
                            //crm_opportunity.sales_invoice.Add(_sales_invoice);

                            try
                            {
                                PackingListDB.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                System.Windows.Forms.MessageBox.Show(ex.ToString());
                            }
                        }
                    }
                }
            }
        }

        private async void crud_modalExpire_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (crud_modalExpire.Visibility == Visibility.Collapsed || crud_modalExpire.Visibility == Visibility.Hidden)
            {
                if (sales_packingDataGrid.SelectedItem is sales_packing sales_packing)
                {
                    item item = await PackingListDB.items.FindAsync(sbxItem.ItemID);
                    app_branch app_branch = null;

                    if (item != null && item.id_item > 0 && sales_packing != null)
                    {
                        if (cbxBranch.SelectedItem != null)
                        { app_branch = cbxBranch.SelectedItem as app_branch; }

                        item_movement item_movement = PackingListDB.item_movement.Find(pnl_ItemMovementExpiry.MovementID);
                        Task Thread = Task.Factory.StartNew(() => select_Item(sales_packing, item, app_branch, item_movement, sbxItem.Quantity));
                    }
                }
            }
        }
    }
}