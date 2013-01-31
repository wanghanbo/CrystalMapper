﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrystalMapper.UnitTest.Northwind;

namespace CrystalMapper.UnitTest.Linq
{
    /// <summary>
    /// Summary description for WhereTest
    /// </summary>
    [TestClass]    
    public class WhereTest
    {
        public WhereTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void WhereEqualTest()
        {
            var customers = Customer.Query().Where(c => c.CustomerID == "ALFKI").ToList();

            Assert.AreEqual(1, customers.Count);
        }

        [TestMethod]
        public void WhereNotEqualTest()
        {
            var customers = Customer.Query().Where(c => c.CustomerID != "ALFKI").ToList();

            Assert.AreNotEqual(0, customers.Count);
        }

        [TestMethod]
        public void WhereGreaterThen()
        {
            Assert.AreEqual(
                            TestHelper.ExecuteScalar("SELECT COUNT(*) FROM ORDERS WHERE FREIGHT > 100")
                            , Order.Query().Where(o => o.Freight > 100).Count());
        }

        [TestMethod]
        public void WhereLessThen()
        {
            Assert.AreEqual(
                          TestHelper.ExecuteScalar("SELECT COUNT(*) FROM ORDERS WHERE FREIGHT < 100")
                          , Order.Query().Where(o => o.Freight < 100).Count());
        }

        [TestMethod]
        public void WhereGreaterThenAndEqualTo()
        {
            Assert.AreEqual(
                            TestHelper.ExecuteScalar("SELECT COUNT(*) FROM ORDERS WHERE FREIGHT >= 100")
                            , Order.Query().Where(o => o.Freight > 100).Count());
        }

        [TestMethod]
        public void WhereLessThenAndEqualTo()
        {
            Assert.AreEqual(
                          TestHelper.ExecuteScalar("SELECT COUNT(*) FROM ORDERS WHERE FREIGHT <= 100")
                          , Order.Query().Where(o => o.Freight < 100).Count());
        }
    }
}