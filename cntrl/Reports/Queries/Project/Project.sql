﻿ select proj.name as ProjectName,
  task.id_project_task,
   task.parent_id_project_task as ParentTask,
    item.name as  Item,
	 item.code as ItemCode,
 task.code as TaskCode,
  task.item_description as Task,
   task.status,contacts.name as Contact,
   contacts.code as ContactCode,
   Contacts.gov_code as GovermentId,
sum(task.quantity_est) as QuantityEst, 
sum(exe.Quantity) as QuantityReal, 
task.unit_cost_est as CostEst,
 exe.unit_cost as CostReal, 
 task.start_date_est as StartDate,
  task.end_date_est as EndDate ,
   sum(sbd.quantity * sbd.unit_price) as TotalBudgeted,
                            sum(sid.quantity * sid.unit_price) as TotalInvoiced,
                            sum(ps.debit) as TotalPaid,
                            sum(sbd.quantity * sbd.unit_price)-sum(ps.debit) as Balance
 
from project_task as task 

 inner join projects  as proj on proj.id_project = task.id_project 
  inner join contacts   on proj.id_contact = contacts.id_contact 

 inner join items as item on task.id_item = item.id_item
 left join  production_execution_detail as exe on task.id_project_task = exe.id_project_task 
  left join sales_budget as sb on p.id_project = sb.id_project
                            left join sales_budget_detail as sbd on sb.id_sales_budget = sbd.id_sales_budget
                            left join sales_invoice as si  on p.id_project = si.id_project
                            left join sales_invoice_detail as sid on si.id_sales_invoice = sid.id_sales_invoice
                            left join payment_schedual as ps on ps.id_sales_invoice = si.id_sales_invoice

 where proj.id_project = @IDProject
 
 group by task.id_project_task 