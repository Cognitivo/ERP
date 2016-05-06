﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Entity;
using System.Data;
using entity;
using System.Collections.Generic;

namespace Cognitivo.Product
{
    public partial class Inventory : Page
    {
        InventoryDB InventoryDB = new InventoryDB();
        CollectionViewSource item_inventoryViewSource, item_inventoryitem_inventory_detailViewSource, app_branchapp_locationViewSource, app_branchViewSource;
       
        cntrl.Panels.pnl_ItemMovement objpnl_ItemMovement;

        public Inventory()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            item_inventoryitem_inventory_detailViewSource = (CollectionViewSource)(FindResource("item_inventoryitem_inventory_detailViewSource"));
            app_branchapp_locationViewSource = (CollectionViewSource)(FindResource("app_branchapp_locationViewSource"));
            item_inventoryViewSource = ((CollectionViewSource)(FindResource("item_inventoryViewSource")));
            InventoryDB.item_inventory.Where(a => a.id_company == CurrentSession.Id_Company).Load();
            item_inventoryViewSource.Source = InventoryDB.item_inventory.Local;

            CollectionViewSource app_currencyfxViewSource = ((CollectionViewSource)(FindResource("app_currencyfxViewSource")));
            InventoryDB.app_currencyfx.Where(a => a.id_company == CurrentSession.Id_Company && a.is_active).Load();
            app_currencyfxViewSource.Source = InventoryDB.app_currencyfx.Local;

            app_branchViewSource = (CollectionViewSource)(FindResource("app_branchViewSource"));
            InventoryDB.app_branch.Include(b => b.app_location)
                .Where(a => a.is_active == true
                    && a.can_stock == true
                    && a.id_company == CurrentSession.Id_Company)
                .OrderBy(a => a.name).Load();
            app_branchViewSource.Source = InventoryDB.app_branch.Local;

            app_branchViewSource.View.MoveCurrentToFirst();


            if (app_branchapp_locationViewSource.View != null)
            {
                app_branchapp_locationViewSource.View.MoveCurrentToFirst();
            }
            filetr_detail();
        }

        private void filetr_detail()
        {
            app_location app_location = app_branchapp_locationViewSource.View.CurrentItem as app_location;
            if (item_inventoryitem_inventory_detailViewSource != null)
            {
                if (item_inventoryitem_inventory_detailViewSource.View != null)
                {
                    if (item_inventoryitem_inventory_detailViewSource.View.OfType<item_inventory_detail>().Count() > 0)
                    {
                        item_inventoryitem_inventory_detailViewSource.View.Filter = i =>
                        {
                            item_inventory_detail item_inventory_detail = (item_inventory_detail)i;
                            if (item_inventory_detail.id_location == app_location.id_location)
                                return true;
                            else
                                return false;
                        };
                    }
                }
            }
        }
        private void BindItemMovement()
        {
            item_inventory item_inventory = (item_inventory)item_inventoryViewSource.View.CurrentItem;
            app_location app_location = app_branchapp_locationViewSource.View.CurrentItem as app_location;

            if (item_inventory.item_inventory_detail.Where(x => x.id_location == app_location.id_location).Count() == 0)
            {
         
                List<item_product> item_productLIST = InventoryDB.item_product.Where(x => x.id_company == CurrentSession.Id_Company && x.item.is_active).ToList();

                foreach (item_product i in item_productLIST)
                {
                    item_inventory_detail item_inventory_detail = new item_inventory_detail();
                    item_inventory_detail.State = EntityState.Added;
                    item_inventory_detail.item_product = i;
                    item_inventory_detail.id_item_product = i.id_item_product;
                   


                    if (app_branchapp_locationViewSource != null)
                    {
                       
                        item_inventory_detail.id_location = app_location.id_location;
                        item_inventory_detail.timestamp = DateTime.Now;
                 
                    if (InventoryDB.app_currencyfx.Where(x => x.app_currency.is_priority && x.is_active).FirstOrDefault() != null)
                    {
                        item_inventory_detail.id_currencyfx = InventoryDB.app_currencyfx.Where(x => x.app_currency.is_priority && x.is_active).FirstOrDefault().id_currencyfx;
                    }


                    item_inventory.item_inventory_detail.Add(item_inventory_detail);

                }

            }
   
            item_inventoryViewSource.View.Refresh();
            item_inventoryitem_inventory_detailViewSource.View.Refresh();
            item_inventoryitem_inventory_detailViewSource.View.MoveCurrentToFirst();
           app_branchViewSource.View.Refresh();
           cbxBranch.SelectedItem = app_location.app_branch;
          
           
        }

        private void toolBar_btnNew_Click(object sender)
        {
            try
            {
                item_inventory item_inventory = new item_inventory();
                item_inventory.IsSelected = true;
                item_inventory.trans_date = DateTime.Now;
                InventoryDB.Entry(item_inventory).State = EntityState.Added;
                item_inventory.State = EntityState.Added;
                item_inventoryViewSource.View.Refresh();
                item_inventoryViewSource.View.MoveCurrentToLast();


            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
            }
        }

        private void toolBar_btnEdit_Click(object sender)
        {
            if (item_inventoryDataGrid.SelectedItem != null)
            {
                item_inventory item_inventory_old = (item_inventory)item_inventoryDataGrid.SelectedItem;
                item_inventory_old.IsSelected = true;
                item_inventory_old.State = EntityState.Modified;
                InventoryDB.Entry(item_inventory_old).State = EntityState.Modified;
            }
            else
            {
                toolBar.msgWarning("Please Select an Item");
            }
        }

        private void toolBar_btnSave_Click(object sender)
        {
            try
            {
                if (InventoryDB.SaveChanges() == 0)
                {
                    toolBar.msgSaved();
                    item_inventoryViewSource.View.Refresh();
                }
                else
                {
                    toolBar.msgWarning("Unable to Save");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void toolBar_btnCancel_Click(object sender)
        {
            InventoryDB.CancelAllChanges();
        }

        private void toolBar_btnDelete_Click(object sender)
        {
            MessageBox.Show("Function Not Available");
        }

        private void toolBar_btnApprove_Click(object sender)
        {
            item_inventory item_inventory = (item_inventory)item_inventoryDataGrid.SelectedItem;
            item_inventory.id_branch = (int)cbxBranch.SelectedValue;
            InventoryDB.Approve();

            toolBar.msgDone();
        }

        private void CbxBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


        }

      

        private void location_ListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (item_inventoryDataGrid.SelectedItem != null)
            {
                if (app_branchapp_locationViewSource != null)
                {
                    if (app_branchapp_locationViewSource.View != null)
                    {
                        BindItemMovement();
                    }
                }
            }
        }

        private void crud_modal_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
          
            if (crud_modal.IsVisible == false)
            {
               
              
                item_inventory item_inventory = (item_inventory)item_inventoryViewSource.View.CurrentItem;
             
                foreach (item_inventory_detail _item_inventory_detail in objpnl_ItemMovement.item_inventoryList.Skip(1))
                {
                    item_inventory.item_inventory_detail.Add(_item_inventory_detail);
                }
               
                item_inventoryViewSource.View.Refresh();
                item_inventoryitem_inventory_detailViewSource.View.Refresh();
                item_inventoryitem_inventory_detailViewSource.View.MoveCurrentToFirst();
                app_branchViewSource.View.Refresh();
                cbxBranch.SelectedItem = objpnl_ItemMovement.item_inventoryList.FirstOrDefault().app_location.app_branch;
            }
        }

        private void EditCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter as item_inventory_detail != null)
            {
                e.CanExecute = true;
            }
        }

        private void EditCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            crud_modal.Children.Clear();
            crud_modal.Visibility = Visibility.Hidden;
            item_inventory_detail item_inventory_detail = e.Parameter as item_inventory_detail;
               item_inventory item_inventory = (item_inventory)item_inventoryDataGrid.SelectedItem;
            if (item_inventory_detail != null)
            {
                crud_modal.Visibility = System.Windows.Visibility.Visible;
                objpnl_ItemMovement = new cntrl.Panels.pnl_ItemMovement();
           
                foreach (item_inventory_detail _item_inventory_detail in item_inventoryitem_inventory_detailViewSource.View.OfType<item_inventory_detail>().Where(x=>x.id_item_product==item_inventory_detail.id_item_product).ToList())
                {
                    if (_item_inventory_detail.item_inventory_dimension.Count() == 0)
                    {


                        if (InventoryDB.item_dimension.Where(x => x.id_item == _item_inventory_detail.item_product.id_item).ToList() != null)
                        {
                            List<item_dimension> item_dimensionList = InventoryDB.item_dimension.Where(x => x.id_item == _item_inventory_detail.item_product.id_item).ToList();
                            foreach (item_dimension item_dimension in item_dimensionList)
                            {
                                item_inventory_dimension item_inventory_dimension = new item_inventory_dimension();
                                item_inventory_dimension.id_dimension = item_dimension.id_app_dimension;
                                item_inventory_dimension.value = item_dimension.value;
                                item_inventory_detail.item_inventory_dimension.Add(item_inventory_dimension);
                            }


                        }
                    }
                    _item_inventory_detail.IsSelected = true;
                }

                objpnl_ItemMovement.item_inventoryList = item_inventoryitem_inventory_detailViewSource.View.OfType<item_inventory_detail>().Where(x => x.id_item_product == item_inventory_detail.id_item_product).ToList();
                objpnl_ItemMovement.InventoryDB = InventoryDB;
                crud_modal.Children.Add(objpnl_ItemMovement);
            }
        }
    }
}
