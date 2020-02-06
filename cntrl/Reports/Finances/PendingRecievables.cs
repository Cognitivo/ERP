﻿namespace cntrl.Reports.Finance
{
    public static class PendingReceivables
    {
        public static string query = @"
  set global sql_mode='STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';
                                set session sql_mode='STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';
select
								contact.code as Code,
								contact.gov_code as GovID,
								contact.name as Contact,
								si.number as Number,
								si.comment as Comment,
								cond.name as Conditions,
								contract.name as Contract,
								DATE_FORMAT(schedual.expire_date,'%d %b %y') as ExpiryDate,
								curr.name as CurrencyName,
								fx.buy_value as Rate,
								schedual.debit as Value,
								schedual.CreditChild as Paid,
								(schedual.debit - schedual.CreditChild) as Balance,
								schedual.trans_date as TransDate
								from (
								select
									parent.*,
									( select if(sum(credit) is null, 0, sum(credit))
									from payment_schedual as child where child.parent_id_payment_schedual = parent.id_payment_schedual
									) as CreditChild
								from payment_schedual as parent
								where parent.id_company = @CompanyID and parent.trans_date between '@startDate' and '@EndDate'
								group by parent.id_payment_schedual
								) as schedual

								inner join contacts as contact on schedual.id_contact = contact.id_contact
								inner join app_currencyfx as fx on schedual.id_currencyfx = fx.id_currencyfx
								inner join app_currency as curr on fx.id_currency = curr.id_currency
								inner join sales_invoice as si on schedual.id_sales_invoice = si.id_sales_invoice
								left join app_contract as contract on si.id_contract = contract.id_contract
								left join app_condition as cond on contract.id_condition = cond.id_condition
								where (schedual.debit - schedual.CreditChild) > 0
								group by schedual.id_payment_schedual
								order by schedual.expire_date";
    }
}