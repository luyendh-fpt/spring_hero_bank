using System;
using ConsoleApp3.model;
using MySql.Data.MySqlClient;
using SpringHeroBank.entity;
using SpringHeroBank.utility;

namespace SpringHeroBank.model
{
    public class AccountModel
    {
        public Boolean Save(Account account)
        {
            DbConnection.Instance().OpenConnection(); // đảm bảo rằng đã kết nối đến db thành công.
            var salt = Hash.RandomString(7); // sinh ra chuỗi muối random.
            account.Salt = salt; // đưa muối vào thuộc tính của account để lưu vào database.
            // mã hoá password của người dùng kèm theo muối, set thuộc tính password mới.
            account.Password = Hash.GenerateSaltedSHA1(account.Password, account.Salt);
            var sqlQuery = "insert into `accounts` " +
                           "(`username`, `password`, `accountNumber`, `identityCard`, `balance`, `phone`, `email`, `fullName`, `salt`, `status`) values" +
                           "(@username, @password, @accountNumber, @identityCard, @balance, @phone, @email, @fullName, @salt, @status)";
            var cmd = new MySqlCommand(sqlQuery, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@username", account.Username);
            cmd.Parameters.AddWithValue("@password", account.Password);
            cmd.Parameters.AddWithValue("@accountNumber", account.AccountNumber);
            cmd.Parameters.AddWithValue("@identityCard", account.IdentityCard);
            cmd.Parameters.AddWithValue("@balance", account.Balance);
            cmd.Parameters.AddWithValue("@phone", account.Phone);
            cmd.Parameters.AddWithValue("@email", account.Email);
            cmd.Parameters.AddWithValue("@fullName", account.FullName);
            cmd.Parameters.AddWithValue("@salt", account.Salt);
            cmd.Parameters.AddWithValue("@status", account.Status);
            var result = cmd.ExecuteNonQuery();
            DbConnection.Instance().CloseConnection();
            return result == 1;
        }

        public bool UpdateBalance(Account account, decimal amount, Transaction.TransactionType transactionType)
        {
            DbConnection.Instance().OpenConnection(); // đảm bảo rằng đã kết nối đến db thành công.
            var transaction = DbConnection.Instance().Connection.BeginTransaction(); // Khởi tạo transaction.

            // Lấy thông tin số dư mới nhất của tài khoản.
            var queryBalance = "select balance from `accounts` where username = @username and status = @status";
            MySqlCommand queryBalanceCommand = new MySqlCommand(queryBalance, DbConnection.Instance().Connection);
            queryBalanceCommand.Parameters.AddWithValue("@username", account.Username);
            queryBalanceCommand.Parameters.AddWithValue("@status", account.Status);
            var balanceReader = queryBalanceCommand.ExecuteReader();

            if (!balanceReader.Read())
            {
                // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                // Hàm dừng tại đây.
                transaction.Rollback();
                return false;
            }

            // Đảm bảo sẽ có bản ghi.
            var currentBalance = balanceReader.GetDecimal("balance");
            if (transactionType == Transaction.TransactionType.DEPOSIT)
            {
                currentBalance += amount;
            }
            else if (transactionType == Transaction.TransactionType.WITHDRAW)
            {
                if (amount > currentBalance)
                {
                    transaction.Rollback();
                    return false;
                }
                currentBalance -= amount;
            }
            else
            {
                transaction.Rollback();
                return false;
            }

            var result = 0;
            try
            {
                var sqlQuery = "update `accounts` set balance = @balance where username = @username and status = 1";
                var cmd = new MySqlCommand(sqlQuery, DbConnection.Instance().Connection);
                cmd.Parameters.AddWithValue("@username", account.Username);
                cmd.Parameters.AddWithValue("@balance", currentBalance);
                result = cmd.ExecuteNonQuery();
                // lưu transaction.
                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback(); // Kết thúc transaction.
                throw;
            }

            DbConnection.Instance().CloseConnection();
            return result == 1;
        }

        public Boolean CheckExistUserName(string username)
        {
            return false;
        }

        public Account GetAccountByUserName(string username)
        {
            DbConnection.Instance().OpenConnection();
            var queryString = "select * from  `accounts` where username = @username and status = 1";
            var cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@username", username);
            var reader = cmd.ExecuteReader();
            Account account = null;
            if (reader.Read())
            {
                var _username = reader.GetString("username");
                var password = reader.GetString("password");
                var salt = reader.GetString("salt");
                var accountNumber = reader.GetString("accountNumber");
                var identityCard = reader.GetString("identityCard");
                var balance = reader.GetDecimal("balance");
                var phone = reader.GetString("phone");
                var email = reader.GetString("email");
                var fullName = reader.GetString("fullName");
                var createdAt = reader.GetString("createdAt");
                var updatedAt = reader.GetString("updatedAt");
                var status = reader.GetInt32("status");
                account = new Account(_username, password, salt, accountNumber, identityCard, balance, phone, email,
                    fullName, createdAt, updatedAt, (Account.ActiveStatus) status);
            }

            DbConnection.Instance().CloseConnection();
            return account;
        }
    }
}