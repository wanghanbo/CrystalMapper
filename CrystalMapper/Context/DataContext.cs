﻿/***********************************************************************************
 * Author: Faraz Masood Khan 
 * Description: This class represents a live database connection 
 *              and provides lot of CrystalMapper functions to interactive database
 * Project: http://www.fanaticlab.com/projects/crystalmapper/
 * Copyright (c) 2013 FanaticLab
 ***********************************************************************************/

using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;

using CoreSystem.Data;
using CoreSystem.Dynamic;
using System.Linq;
using CrystalMapper.Linq;
using CoreSystem.Util;

namespace CrystalMapper.Context
{
    /// <summary>
    /// Database connection wrapper class; primary class to interact with database in CrystalMapper
    /// </summary>
    public class DataContext : IDisposable
    {
        private bool disposeConnection = false;
        private Database database;
        private DbConnection connection;
        private DbTransaction transaction;

        /// <summary>
        /// True if object is disposed
        /// </summary>
        internal bool IsDisposed { get; private set; }

        /// <summary>
        /// Open database connection from "Default-Db" connection string
        /// </summary>
        public DataContext()
            : this(DbContext.DefaultDb)
        { }

        /// <summary>
        ///Open database connection from specified connection string
        /// </summary>
        /// <param name="name">Name of the connection string define in configuration</param>
        public DataContext(string name)
            : this(DbFactory.GetDatabase(name))
        { }

        /// <summary>
        /// Open database connection from specified database
        /// </summary>
        /// <param name="database">Database to open connection from</param>
        public DataContext(Database database)
            : this(database, database.CreateConnection())
        {
            disposeConnection = true;
        }

        /// <summary>
        /// Create DataContext from database and its connection, connection should be open
        /// </summary>
        /// <param name="database">Database to which connection belongs</param>
        /// <param name="connection">Database connection</param>
        private DataContext(Database database, DbConnection connection)
        {
            this.database = database;
            this.connection = connection;
        }

        /// <summary>
        /// Database object of connected database
        /// </summary>
        public Database Database
        {
            get { return this.database; }
        }

        /// <summary>
        /// Current opened database connection
        /// </summary>
        public DbConnection Connection
        {
            get { return this.connection; }
        }

        /// <summary>
        /// Current transaction, if transaction was begin
        /// </summary>
        public DbTransaction Transaction
        {
            get { return this.transaction; }
        }

        /// <summary>
        /// Create a new SQL command object in current database connection and/or transaction
        /// </summary>
        /// <param name="cmdText">SQL command text</param>
        /// <returns>SQL command object</returns>
        public DbCommand CreateCommand(string cmdText)
        {
            return (this.transaction != null ? this.database.CreateCommand(cmdText, transaction) : this.database.CreateCommand(cmdText, this.connection));
        }

        /// <summary>
        /// Create a new SQL parameter for current database
        /// </summary>
        /// <returns>SQL parameter</returns>
        public DbParameter CreateParameter()
        {
            return this.database.CreateParameter();
        }

        /// <summary>
        /// Create a new SQL parameter for current database
        /// </summary>
        /// <param name="paramName">SQL parameter name</param>
        /// <param name="paramValue">SQL parameter value</param>
        /// <returns>SQL parameter object</returns>
        public DbParameter CreateParameter(string paramName, object paramValue)
        {
            return this.database.CreateParameter(paramValue, paramName);
        }

        /// <summary>
        /// Create a new query object and associate it with current database connection.
        /// </summary>
        /// <typeparam name="T">Type of record entity</typeparam>
        /// <returns>Query object for T entity</returns>
        public IQueryable<T> Query<T>()
            where T : IRecord, new()
        {
            return new Query<T>(this);
        }

        /// <summary>
        /// Create a new record in this connection context
        /// </summary>
        /// <param name="record">New record</param>
        public void Create(IRecord record)
        {
            if (!record.Create(this))
                throw new InvalidOperationException(string.Format("Failed to a create record '{0}' in database", record));
        }

        /// <summary>
        /// Create new records in current database connection
        /// </summary>
        /// <param name="records">New records</param>
        public void Create(IEnumerable<IRecord> records)
        {
            foreach (var record in records)
                this.Create(record);
        }

        /// <summary>
        /// Update a record in current database connection
        /// </summary>
        /// <param name="record">Record to update</param>
        public void Update(IRecord record)
        {
            if (!record.Update(this))
                throw new InvalidOperationException(string.Format("Failed to update a record '{0}' in database", record));
        }

        /// <summary>
        /// Update records in current database connection
        /// </summary>
        /// <param name="records">Records to update</param>
        public void Update(IEnumerable<IRecord> records)
        {
            foreach (var record in records)
                this.Update(record);
        }

        /// <summary>
        /// Delete a record in current database connection
        /// </summary>
        /// <param name="record">Record to delete</param>
        public void Delete(IRecord record)
        {
            if (!record.Delete(this))
                throw new InvalidOperationException(string.Format("Failed to delete a record '{0}' in database", record));
        }

        /// <summary>
        /// Delete records in current database connection
        /// </summary>
        /// <param name="records">Records to delete</param>
        public void Delete(IEnumerable<IRecord> records)
        {
            foreach (var record in records)
                this.Delete(record);
        }

        /// <summary>
        /// Returns all records of T entity in array from current database connection
        /// </summary>
        /// <typeparam name="T">Record entity type</typeparam>
        /// <returns>All records</returns>
        /// <remarks>ToList is faster than to ToArray because result is constructed in List first</remarks>
        public T[] ToArray<T>() where T : IRecord, new()
        {
            return this.Query<T>().ToArray();
        }

        /// <summary>
        /// Execute specified query and return result in array from current database connection
        /// </summary>
        /// <typeparam name="T">Record entity type</typeparam>
        /// <param name="cmdText">SQL query to execute in database</param>
        /// <returns>Result of query from current database connection</returns>
        /// <remarks>ToList is faster than to ToArray because result is constructed in List first</remarks>
        public T[] ToArray<T>(string cmdText) where T : IRecord, new()
        {
            return this.ToList<T>(cmdText).ToArray();
        }

        /// <summary>
        /// Execute specified query and return result in array from current database connection
        /// </summary>
        /// <typeparam name="T">Record entity type</typeparam>
        /// <param name="cmdText">SQL query to execute in database</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns>Result of query from current database connection</returns>
        /// <remarks>ToList is faster than to ToArray because result is constructed in List first</remarks>
        public T[] ToArray<T>(string cmdText, Dictionary<string, object> parameters) where T : IRecord, new()
        {
            return this.ToList<T>(cmdText, parameters).ToArray();
        }

        /// <summary>
        /// Returns all records of T entity in list from current database connection
        /// </summary>
        /// <typeparam name="T">Record entity type</typeparam>
        /// <returns>All records</returns>
        public List<T> ToList<T>(string cmdText) where T : IRecord, new()
        {
            return this.ToList<T>(cmdText, null);
        }

        /// <summary>
        /// Execute specified query and return result in list from current database connection
        /// </summary>
        /// <typeparam name="T">Record entity type</typeparam>
        /// <param name="cmdText">SQL query to execute in database</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns>Result of query from current database connection</returns>
        public List<T> ToList<T>(string cmdText, Dictionary<string, object> parameters) where T : IRecord, new()
        {
            using (DbCommand command = this.CreateCommand(cmdText))
            {
                if (parameters != null)
                    foreach (string param in parameters.Keys)
                        command.Parameters.Add(this.CreateParameter(param, DbConvert.DbValue(parameters[param])));

                return DataContext.ToList<T>(command);
            }
        }

        /// <summary>
        /// Execute specified query and return result in list from current database connection
        /// </summary>
        /// <typeparam name="T">Record entity type</typeparam>
        /// <param name="command">Execute reader command</param>
        /// <returns>Result of query from current database connection</returns>
        public static List<T> ToList<T>(DbCommand command) where T : IRecord, new()
        {
            List<T> records = new List<T>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var record = new T();
                    record.Read(reader);
                    records.Add(record);
                }
            }

            return records;
        }

        /// <summary>
        /// Execute specified query in current connection and return list of dynamic objects
        /// </summary>
        /// <param name="cmdText">SQL query to execute in current connection</param>
        /// <returns>List of Donymous objects</returns>
        public List<dynamic> ToDynamic(string cmdText)
        {
            return this.ToDynamic(cmdText, null);
        }

        /// <summary>
        /// Execute specified query in current connection and return list of dynamic objects
        /// </summary>
        /// <param name="cmdText">SQL query to execute in current connection</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns>List of Donymous objects</returns>
        public List<dynamic> ToDynamic(string cmdText, Dictionary<string, object> parameters)
        {            
            using (var command = this.CreateCommand(cmdText))
            {
                if (parameters != null)
                    foreach (string param in parameters.Keys)
                        command.Parameters.Add(this.CreateParameter(param, DbConvert.DbValue(parameters[param])));

                return DataContext.ToDynamic(command);
            }
        }

        /// <summary>
        /// Execute specified query in current connection and return list of dynamic objects
        /// </summary>
        /// <param name="command">SQL command</param>
        /// <returns>List of Donymous objects</returns>
        public static List<dynamic> ToDynamic(DbCommand command)
        {
            var records = new List<dynamic>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    records.Add(new Donymous(reader));
            }

            return records;
        }

        /// <summary>
        /// Execute SQL query and returns scalar value
        /// </summary>
        /// <param name="cmdText">SQL command to execute</param>
        /// <returns>Scalar query result</returns>
        public object ToScalar(string cmdText)
        {
            return this.ToScalar(cmdText, null);
        }

        /// <summary>
        /// Execute SQL query and returns scalar value
        /// </summary>
        /// <param name="cmdText">SQL command to execute</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns>Scalar query result</returns>
        public object ToScalar(string cmdText, Dictionary<string, object> parameters)
        {            
            using (var command = this.CreateCommand(cmdText))
            {
                if (parameters != null)
                    foreach (string param in parameters.Keys)
                        command.Parameters.Add(this.CreateParameter(param, DbConvert.DbValue(parameters[param])));

                return DbConvert.CLRValue(command.ExecuteScalar());
            }
        }

        /// <summary>
        /// Execute SQL query and returns scalar value
        /// </summary>
        /// <typeparam name="T">Cast scalar value to type</typeparam>
        /// <param name="cmdText">SQL command to execute</param>
        /// <returns>Scalar query result</returns>
        public T ToScalar<T>(string cmdText)
        {
            return this.ToScalar<T>(cmdText, null);
        }

        /// <summary>
        /// Execute SQL query and returns scalar value
        /// </summary>
        /// <typeparam name="T">Cast scalar value to type</typeparam>
        /// <param name="cmdText">SQL command to execute</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns>Scalar query result</returns>
        public T ToScalar<T>(string cmdText, Dictionary<string, object> parameters)
        {
            Type typeOfT = typeof(T);
            if (typeOfT.IsGenericType && typeOfT.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                object value = this.ToScalar(cmdText, parameters);
                if (value != null)
                    return (T)Convert.ChangeType(value, typeOfT.GetGenericArguments()[0]);

                return (T)Activator.CreateInstance(typeOfT);
            }

            return (T)Convert.ChangeType(this.ToScalar(cmdText, parameters), typeof(T));
        }

        #region Transaction functions

        /// <summary>
        /// Starts a database transaction
        /// </summary>
        public void BeginTransaction()
        {
            this.transaction = this.connection.BeginTransaction();
        }

        /// <summary>
        /// Starts a database transaction
        /// </summary>
        /// <param name="isolationLevel">Transaction isolation level, refer RDBMS provider for isolation level details</param>
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            this.transaction = this.connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Commits current transaction
        /// </summary>
        public void CommitTransaction()
        {
            if (this.transaction == null)
                throw new InvalidOperationException("Transaction was never started, please use BeginTransaction to start a transaction on current context");

            transaction.Commit();
            transaction = null;
        }

        /// <summary>
        /// Rollback current transaction
        /// </summary>
        public void RollbackTransaction()
        {
            if (this.transaction == null)
                throw new InvalidOperationException("Transaction was never started, please use BeginTransaction to start a transaction on current context");

            transaction.Rollback();
            transaction = null;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Disposes associated database connection and transaction
        /// </summary>
        /// <param name="disposing">Do disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.transaction != null)
                    this.transaction.Dispose();

                if (this.disposeConnection)
                    this.connection.Dispose();

                this.IsDisposed = true;
            }
        }
    }
}
