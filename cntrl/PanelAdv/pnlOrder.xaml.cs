﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Data.Entity;
using entity;
using System.Data;
using System.ComponentModel;
using System.Data.Entity.Validation;

namespace cntrl.PanelAdv
{
    public partial class pnlOrder : UserControl
    {
        CollectionViewSource production_orderViewSource, production_lineViewSource; //, itemViewSource;
        public List<project_task> project_taskLIST { get; set; }
        public dbContext shared_dbContext { get; set; }
        public CollectionViewSource projectViewSource { get; set; }
        dbContext _dbContext = new dbContext();

        public pnlOrder()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            production_order production_order = new production_order();

            // Do not load your data at design time.
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                //Load your data here and assign the result to the CollectionViewSource.
                production_orderViewSource = (CollectionViewSource)this.Resources["production_orderViewSource"];

                shared_dbContext.db.production_order.Add(production_order);

                production_orderViewSource.Source = shared_dbContext.db.production_order.Local;

                production_lineViewSource = (CollectionViewSource)this.Resources["production_lineViewSource"];
                shared_dbContext.db.production_line.Load();
                production_lineViewSource.Source = shared_dbContext.db.production_line.Local;
            }


            if (project_taskLIST.Count() > 0)
            {
                //production_order production_order = new production_order();
                production_order.id_project = project_taskLIST.FirstOrDefault().id_project;
                production_order.name = project_taskLIST.FirstOrDefault().project.name;

                foreach (var item in project_taskLIST)
                {
                    project_task _project_task = (project_task)item;
                    production_order_detail production_order_detail = new production_order_detail();

                    production_order_detail.id_order_detail = _project_task.id_project_task;
                    production_order_detail.name = _project_task.item_description;
                    production_order_detail.item = _project_task.items;
                    production_order_detail.id_item = _project_task.id_item;

                    //If Item has Recepie
                    if (_project_task.items.item_recepie.Count > 0)
                    {
                        production_order_detail.is_input = false;
                    }
                    else
                    {
                        production_order_detail.is_input = true;
                    }

                    if (_project_task.parent != null)
                    {
                        production_order_detail _production_order_detail = production_order.production_order_detail.Where(x => x.id_project_task == _project_task.parent.id_project_task).FirstOrDefault();
                        if (_production_order_detail != null)
                        {
                            production_order_detail.parent = _production_order_detail;
                        }
                    }

                    production_order_detail.id_project_task = _project_task.id_project_task;
                    if (_project_task.quantity_est > 0)
                    {
                        production_order_detail.quantity = (decimal)_project_task.quantity_est;
                    }

                    production_order.status = entity.Status.Production.Pending;
                    production_order.name = _project_task.project.name;
                    production_order.production_order_detail.Add(production_order_detail);
                }

                shared_dbContext.db.production_order.Add(production_order);


                production_orderViewSource.View.MoveCurrentToLast();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            foreach (var item in project_taskLIST)
            {
                project_task _project_task = (project_task)item;

                if (_project_task.status == entity.Status.Project.Approved)
                {
                    _project_task.status = entity.Status.Project.InProcess;
                    _project_task.IsSelected = false;
                }
            }

            IEnumerable<DbEntityValidationResult> validationresult = _dbContext.db.GetValidationErrors();

            if (validationresult.Count() == 0)
            {
                shared_dbContext.SaveChanges();
                Grid parentGrid = (Grid)this.Parent;
                parentGrid.Children.Clear();
                parentGrid.Visibility = Visibility.Hidden;
            }
            filter_task();
        }

        public void filter_task()
        {
            if (projectViewSource != null)
            {
                if (projectViewSource.View != null)
                {

                    projectViewSource.View.Filter = i =>
                    {
                        project_task _project_task = (project_task)i;
                        if (_project_task.parent == null)
                            return true;
                        else
                        {
                            return false;
                        }
                    };
                }

            }

        }
        private void lblCancel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var item in project_taskLIST)
            {
                project_task _project_task = (project_task)item;
                _project_task.IsSelected = false;

            }

            projectViewSource.View.Refresh();

            Grid parentGrid = (Grid)this.Parent;
            parentGrid.Children.Clear();
            parentGrid.Visibility = Visibility.Hidden;
        }

    }
}
