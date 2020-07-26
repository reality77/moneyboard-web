using System;
using System.Collections.Generic;
using dto.Model;

namespace web.Models
{
    public class AccountTransactionsModel
    {
        public AccountDetails Details { get; set; }
        public IEnumerable<TransactionWithBalance> Transactions { get; set; }
    }
}