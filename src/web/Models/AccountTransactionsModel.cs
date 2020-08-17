using System;
using System.Collections.Generic;
using System.Linq;
using dto.Model;

namespace web.Models
{
    public class AccountTransactionsModel
    {
        public AccountDetails Details { get; set; }
        public IEnumerable<IGrouping<DateTime ,TransactionWithBalance>> Transactions { get; set; }
    }
}