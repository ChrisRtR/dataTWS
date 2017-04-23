using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Forms;
using IBApi;
using System.Xml.Linq;
using System.Globalization;
using System.Threading;

namespace dataTWS
{
    public partial class Form1 : Form
    {
        EWrapperImpl ibClient;
        int taskcounter = 0;

        delegate void SetTextCallback(XmlDocument xDoc);

        delegate void SetXDocumentCallback(string data);

        //public void AddListBoxItem(XmlDocument xDoc)   //Callback for data as XMLDocument (SLOW!)
        //{
        //    if (lstData.InvokeRequired)
        //    {
        //        SetTextCallback d = new SetTextCallback(AddListBoxItem);
        //        this.Invoke(d, new object[] { xDoc });
        //    }
        //    else
        //    {
        //        XmlNode nodeCompanyTicker = xDoc.DocumentElement.ChildNodes[0].ChildNodes[2].ChildNodes[0].ChildNodes[2];
        //        this.lstData.Items.Insert(0, nodeCompanyTicker.ChildNodes[2].InnerText);
        //        this.lstData.Items.Insert(0, nodeCompanyTicker.ChildNodes[1].InnerText);

        //        this.lstData.Items.Add(xDoc.DocumentElement.ChildNodes[0].ChildNodes[0].InnerText);
        //        this.lstData.Items.Add(nodeCompanyTicker.ChildNodes[0].InnerXml);
        //    }
        //}

        public void AddXDocument(string data)  //Callback for fundamentaldata (XML) as a string
        {
            selectCultureUS();
            if (lstData.InvokeRequired)
            {
                SetXDocumentCallback d = new SetXDocumentCallback(AddXDocument);
                this.Invoke(d, new object[] { data });
            }
            else
            {
                XElement doc = XElement.Parse(data);    //string 'converted' XML-doc for parsing with LINQ

                // Query for retrieving Company Ticker
                var query = from a in doc.Elements("Company").Elements("SecurityInfo").Elements("Security")
                            where (string)a.Attribute("code") == "1"
                            from b in a.Elements("SecIds").Elements("SecId")
                            where (string)b.Attribute("type") == "TICKER"
                            select b;

                // Query for retreiving EPS Estimates
                var estimateBase = from esta in doc.Elements("ConsEstimates").Elements("FYEstimates").Elements("FYEstimate")
                                   where (string)esta.Attribute("type") == "EPS"
                                   from estb in esta.Elements("FYPeriod")
                                   where (string)estb.Attribute("endCalYear") == "2017" && ((string)estb.Attribute("endMonth") == "03" ||
                                                                                                (string)estb.Attribute("endMonth") == "04" || (string)estb.Attribute("endMonth") == "05")
                                   select estb;

                var estimateMean = from esta in estimateBase.Elements("ConsEstimate")
                                   where (string)esta.Attribute("type") == "Mean"
                                   from estb in esta.Elements("ConsValue")
                                   where (string)estb.Attribute("dateType") == "CURR"
                                   select estb;

                var estimateLow = from esta in estimateBase.Elements("ConsEstimate")
                                  where (string)esta.Attribute("type") == "Low"
                                  from estb in esta.Elements("ConsValue")
                                  where (string)estb.Attribute("dateType") == "CURR"
                                  select estb;

                var estimateHigh = from esta in estimateBase.Elements("ConsEstimate")
                                   where (string)esta.Attribute("type") == "High"
                                   from estb in esta.Elements("ConsValue")
                                   where (string)estb.Attribute("dateType") == "CURR"
                                   select estb;

                // Query to retrieve Revenue Estimates
                var estRevenueBase = from esta in doc.Elements("ConsEstimates").Elements("FYEstimates").Elements("FYEstimate")
                                     where (string)esta.Attribute("type") == "REVENUE"
                                     from estb in esta.Elements("FYPeriod")
                                     where (string)estb.Attribute("endCalYear") == "2017" && ((string)estb.Attribute("endMonth") == "03" ||
                                                                                                (string)estb.Attribute("endMonth") == "04" || (string)estb.Attribute("endMonth") == "05")
                                     select estb;

                var estRevenueMean = from esta in estRevenueBase.Elements("ConsEstimate")
                                     where (string)esta.Attribute("type") == "Mean"
                                     from estb in esta.Elements("ConsValue")
                                     where (string)estb.Attribute("dateType") == "CURR"
                                     select estb;

                var estRevenueLow = from esta in estRevenueBase.Elements("ConsEstimate")
                                    where (string)esta.Attribute("type") == "Low"
                                    from estb in esta.Elements("ConsValue")
                                    where (string)estb.Attribute("dateType") == "CURR"
                                    select estb;

                var estRevenueHigh = from esta in estRevenueBase.Elements("ConsEstimate")
                                     where (string)esta.Attribute("type") == "High"
                                     from estb in esta.Elements("ConsValue")
                                     where (string)estb.Attribute("dateType") == "CURR"
                                     select estb;

                // Add items to list, opted for Insert instead of Add to put newest data on top.
                this.lstData.Items.Insert(0, "Ticker: " + query.FirstOrDefault().Value + "  ~  " + "Mean EPS est.: " + estimateMean.LastOrDefault().Value + "  ~  " +
                    "Low EPS est.: " + estimateLow.LastOrDefault().Value + "  ~  " + "High EPS est.: " + estimateHigh.LastOrDefault().Value + " ~  " +
                    "Mean Rev est.: " + Convert.ToDecimal(estRevenueMean.LastOrDefault().Value).ToString("0.0") + "  ~  " +
                    "Low Rev est.: " + Convert.ToDecimal(estRevenueLow.LastOrDefault().Value).ToString("0.0") + " ~  " +
                    "High Rev est.: " + Convert.ToDecimal(estRevenueHigh.LastOrDefault().Value).ToString("0.0")
                    );

                //Query to retrieve company name
                var coName = from p in doc.Elements("Company").Elements("CoName").Elements("Name")
                             where (string)p.Attribute("type") == "Long"
                             select p;

                #region External DB
                //Methods and queries to select and add company data to the EXTERNAL DB
                using (RewardTheRiskEntities dbExt = new RewardTheRiskEntities())
                {
                    var existingData = from nd in dbExt.CompanyData
                                       select nd;

                    var existingTickerList = from ticker in dbExt.CompanyData
                                             select ticker.Ticker;

                    if (!existingTickerList.Contains(query.FirstOrDefault().Value)) // Check if company/ticker already exists in DB.
                    {
                        CompanyData dataNewEntry = new CompanyData
                        {
                            Ticker = query.FirstOrDefault().Value,
                            CompanyName = coName.FirstOrDefault().Value,
                            EPS_Q12017_Mean = estimateMean.FirstOrDefault().Value,
                            EPS_Q12017_Low = estimateLow.FirstOrDefault().Value,
                            EPS_Q12017_High = estimateHigh.FirstOrDefault().Value,
                            Revenue_Q12017_Mean = Convert.ToDecimal(estRevenueMean.FirstOrDefault().Value).ToString("0,000.0"),
                            Revenue_Q12017_Low = Convert.ToDecimal(estRevenueLow.FirstOrDefault().Value).ToString("0,000.0"),
                            Revenue_Q12017_High = Convert.ToDecimal(estRevenueHigh.FirstOrDefault().Value).ToString("0,000.0")
                        };
                        dbExt.CompanyData.Add(dataNewEntry);
                        dbExt.SaveChanges();
                    }
                    else
                    {
                        foreach (var item in existingData)
                        {
                            if (item.Ticker.Trim().Equals(query.FirstOrDefault().Value))
                            {
                                item.EPS_Q12017_Mean = estimateMean.FirstOrDefault().Value;
                                item.EPS_Q12017_Low = estimateLow.FirstOrDefault().Value;
                                item.EPS_Q12017_High = estimateHigh.FirstOrDefault().Value;
                                item.Revenue_Q12017_Mean = Convert.ToDecimal(estRevenueMean.FirstOrDefault().Value).ToString("0,000.0");
                                item.Revenue_Q12017_Low = Convert.ToDecimal(estRevenueLow.FirstOrDefault().Value).ToString("0,000.0");
                                item.Revenue_Q12017_High = Convert.ToDecimal(estRevenueHigh.FirstOrDefault().Value).ToString("0,000.0");
                            }
                        }
                        dbExt.SaveChanges();
                    }
                }
                #endregion

                #region RtRTest DB
                //// Add Ticker and estimates to database table Quarterly                
                //using (RtRTestEntities dbRTR = new RtRTestEntities())
                //{
                //    var newData = from nd in dbRTR.Quarterlies
                //                  select nd;

                //    var existingTickerList = from ticker in dbRTR.Quarterlies          // Collection of existing ticker-entries in table                                  
                //                             select ticker.Ticker;                     // Specifically selecting Ticker!

                //    if (!existingTickerList.Contains(query.FirstOrDefault().Value))      // First, check if ticker doesn't exist.
                //    {
                //        Quarterly firstQuarter = new Quarterly  // If ticker not in db, new instance should of table should be created.
                //        {
                //            Ticker = query.FirstOrDefault().Value,
                //            EPS_Estimate_Mean = estimateMean.FirstOrDefault().Value,
                //            EPS_Estimate_Low = estimateLow.FirstOrDefault().Value,
                //            EPS_Estimate_High = estimateHigh.FirstOrDefault().Value,
                //            Revenue_Estimate_Mean = Convert.ToDecimal(estRevenueMean.FirstOrDefault().Value).ToString("0,000.0"),
                //            Revenue_Estimate_Low = Convert.ToDecimal(estRevenueLow.FirstOrDefault().Value).ToString("0,000.0"),
                //            Revenue_Estimate_High = Convert.ToDecimal(estRevenueHigh.FirstOrDefault().Value).ToString("0,000.0")
                //        };
                //        dbRTR.Quarterlies.Add(firstQuarter);        // adding row to table.
                //        dbRTR.SaveChanges();
                //    }
                //    else            // Update rows if ticker already in db, no new instance required.
                //    {
                //        foreach (var item in newData)
                //        {
                //            if (item.Ticker.Trim().Equals(query.FirstOrDefault().Value))
                //            {
                //                item.EPS_Estimate_Mean = estimateMean.FirstOrDefault().Value;
                //                item.EPS_Estimate_Low = estimateLow.FirstOrDefault().Value;
                //                item.EPS_Estimate_High = estimateHigh.FirstOrDefault().Value;
                //                item.Revenue_Estimate_Mean = Convert.ToDecimal(estRevenueMean.FirstOrDefault().Value).ToString("0,000.0");
                //                item.Revenue_Estimate_Low = Convert.ToDecimal(estRevenueLow.FirstOrDefault().Value).ToString("0,000.0");
                //                item.Revenue_Estimate_High = Convert.ToDecimal(estRevenueHigh.FirstOrDefault().Value).ToString("0,000.0");

                //                item.EPS_FY_2018 = query.FirstOrDefault().Value;
                //            }
                //        }
                //        dbRTR.SaveChanges();
                //    }

                //    // Query to retrieve company name and adding to database
                //    //var query = from p in doc.Elements("Company").Elements("CoName").Elements("Name")
                //    //            where (string)p.Attribute("type") == "Long"
                //    //            select p.Value;

                //    //foreach (var item in query)
                //    //{
                //    //    this.lstData.Items.Add(item);
                //    //}
                //    //using (RtRTestEntities dbRtR = new RtRTestEntities())
                //    //{
                //    //    foreach (var item in query)
                //    //    {
                //    //        TickerTest ticker = new TickerTest
                //    //        {
                //    //            Ticker = "KO",
                //    //            CompanyName = item
                //    //        };
                //    //        dbRtR.TickerTests.Add(ticker);
                //    //        dbRtR.SaveChanges();
                //    //    }                    
                //    //}

                //}
                #endregion
                taskcounter++;    // counting number of tickers loaded in app.             

                if (taskcounter > 0)
                {
                    lblCounting.Text = "Downloading Data...";
                    label1.Text = "Data of " + taskcounter + " companies loaded...";
                    label1.Font = new Font(label1.Font, FontStyle.Bold);
                }

                if (taskcounter >= 30)
                {
                    lblCounting.Text = "";
                    label1.Text = "All Data Downloaded and Added to Database!";
                    label1.ForeColor = Color.DarkGreen;
                    label1.Font = new Font(label1.Font, FontStyle.Bold);

                    if (chkComplete.Checked == true)
                    {
                        Thread.Sleep(1000);
                        lblCounting.Text = "Application will close in a second...";
                        Thread.Sleep(3000);
                        ibClient.ClientSocket.eDisconnect();
                        Close();
                    }
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
            ibClient = new EWrapperImpl();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ibClient.myform = (Form1)Application.OpenForms[0];

            EClientSocket clientSocket = ibClient.ClientSocket;

            clientSocket.eConnect("127.0.0.1", 7496, 4);

            #region Contracts
            List<Contract> dow30 = new List<Contract>();

            Contract contract_1 = new Contract();
            contract_1.Symbol = "AAPL";
            contract_1.SecType = "STK";
            contract_1.Currency = "USD";
            contract_1.Exchange = "ISLAND";
            dow30.Add(contract_1);

            Contract contract_2 = new Contract();
            contract_2.Symbol = "AXP";
            contract_2.SecType = "STK";
            contract_2.Currency = "USD";
            contract_2.Exchange = "SMART";
            dow30.Add(contract_2);

            Contract contract_3 = new Contract();
            contract_3.Symbol = "BA";
            contract_3.SecType = "STK";
            contract_3.Currency = "USD";
            contract_3.Exchange = "SMART";
            dow30.Add(contract_3);

            Contract contract_4 = new Contract();
            contract_4.Symbol = "CAT";
            contract_4.SecType = "STK";
            contract_4.Currency = "USD";
            contract_4.Exchange = "SMART";
            dow30.Add(contract_4);

            Contract contract_5 = new Contract();
            contract_5.Symbol = "CSCO";
            contract_5.SecType = "STK";
            contract_5.Currency = "USD";
            contract_5.Exchange = "ISLAND";
            dow30.Add(contract_5);

            Contract contract_6 = new Contract();
            contract_6.Symbol = "CVX";
            contract_6.SecType = "STK";
            contract_6.Currency = "USD";
            contract_6.Exchange = "SMART";
            dow30.Add(contract_6);

            Contract contract_7 = new Contract();
            contract_7.Symbol = "DD";
            contract_7.SecType = "STK";
            contract_7.Currency = "USD";
            contract_7.Exchange = "SMART";
            dow30.Add(contract_7);

            Contract contract_8 = new Contract();
            contract_8.Symbol = "DIS";
            contract_8.SecType = "STK";
            contract_8.Currency = "USD";
            contract_8.Exchange = "SMART";
            dow30.Add(contract_8);

            Contract contract_9 = new Contract();
            contract_9.Symbol = "GE";
            contract_9.SecType = "STK";
            contract_9.Currency = "USD";
            contract_9.Exchange = "SMART";
            dow30.Add(contract_9);

            Contract contract_10 = new Contract();
            contract_10.Symbol = "GS";
            contract_10.SecType = "STK";
            contract_10.Currency = "USD";
            contract_10.Exchange = "SMART";
            dow30.Add(contract_10);

            Contract contract_11 = new Contract();
            contract_11.Symbol = "HD";
            contract_11.SecType = "STK";
            contract_11.Currency = "USD";
            contract_11.Exchange = "SMART";
            dow30.Add(contract_11);

            Contract contract_12 = new Contract();
            contract_12.Symbol = "IBM";
            contract_12.SecType = "STK";
            contract_12.Currency = "USD";
            contract_12.Exchange = "SMART";
            dow30.Add(contract_12);

            Contract contract_13 = new Contract();
            contract_13.Symbol = "INTC";
            contract_13.SecType = "STK";
            contract_13.Currency = "USD";
            contract_13.Exchange = "ISLAND";
            dow30.Add(contract_13);

            Contract contract_14 = new Contract();
            contract_14.Symbol = "JNJ";
            contract_14.SecType = "STK";
            contract_14.Currency = "USD";
            contract_14.Exchange = "SMART";
            dow30.Add(contract_14);

            Contract contract_15 = new Contract();
            contract_15.Symbol = "JPM";
            contract_15.SecType = "STK";
            contract_15.Currency = "USD";
            contract_15.Exchange = "SMART";
            dow30.Add(contract_15);

            Contract contract_16 = new Contract();
            contract_16.Symbol = "KO";
            contract_16.SecType = "STK";
            contract_16.Currency = "USD";
            contract_16.Exchange = "SMART";
            dow30.Add(contract_16);

            Contract contract_17 = new Contract();
            contract_17.Symbol = "MCD";
            contract_17.SecType = "STK";
            contract_17.Currency = "USD";
            contract_17.Exchange = "SMART";
            dow30.Add(contract_17);

            Contract contract_18 = new Contract();
            contract_18.Symbol = "MMM";
            contract_18.SecType = "STK";
            contract_18.Currency = "USD";
            contract_18.Exchange = "SMART";
            dow30.Add(contract_18);

            Contract contract_19 = new Contract();
            contract_19.Symbol = "MRK";
            contract_19.SecType = "STK";
            contract_19.Currency = "USD";
            contract_19.Exchange = "SMART";
            dow30.Add(contract_19);

            Contract contract_20 = new Contract();
            contract_20.Symbol = "MSFT";
            contract_20.SecType = "STK";
            contract_20.Currency = "USD";
            contract_20.Exchange = "ISLAND";
            dow30.Add(contract_20);

            Contract contract_21 = new Contract();
            contract_21.Symbol = "NKE";
            contract_21.SecType = "STK";
            contract_21.Currency = "USD";
            contract_21.Exchange = "SMART";
            dow30.Add(contract_21);

            Contract contract_22 = new Contract();
            contract_22.Symbol = "PFE";
            contract_22.SecType = "STK";
            contract_22.Currency = "USD";
            contract_22.Exchange = "SMART";
            dow30.Add(contract_22);

            Contract contract_23 = new Contract();
            contract_23.Symbol = "PG";
            contract_23.SecType = "STK";
            contract_23.Currency = "USD";
            contract_23.Exchange = "SMART";
            dow30.Add(contract_23);

            Contract contract_24 = new Contract();
            contract_24.Symbol = "TRV";
            contract_24.SecType = "STK";
            contract_24.Currency = "USD";
            contract_24.Exchange = "SMART";
            dow30.Add(contract_24);

            Contract contract_25 = new Contract();
            contract_25.Symbol = "UNH";
            contract_25.SecType = "STK";
            contract_25.Currency = "USD";
            contract_25.Exchange = "SMART";
            dow30.Add(contract_25);

            Contract contract_26 = new Contract();
            contract_26.Symbol = "UTX";
            contract_26.SecType = "STK";
            contract_26.Currency = "USD";
            contract_26.Exchange = "SMART";
            dow30.Add(contract_26);

            Contract contract_27 = new Contract();
            contract_27.Symbol = "V";
            contract_27.SecType = "STK";
            contract_27.Currency = "USD";
            contract_27.Exchange = "SMART";
            dow30.Add(contract_27);

            Contract contract_28 = new Contract();
            contract_28.Symbol = "VZ";
            contract_28.SecType = "STK";
            contract_28.Currency = "USD";
            contract_28.Exchange = "SMART";
            dow30.Add(contract_28);

            Contract contract_29 = new Contract();
            contract_29.Symbol = "WMT";
            contract_29.SecType = "STK";
            contract_29.Currency = "USD";
            contract_29.Exchange = "SMART";
            dow30.Add(contract_29);

            Contract contract_30 = new Contract();
            contract_30.Symbol = "XOM";
            contract_30.SecType = "STK";
            contract_30.Currency = "USD";
            contract_30.Exchange = "SMART";
            dow30.Add(contract_30);

            #endregion
            int reqId = 1;

            // Request to list of contracts
            foreach (var item in dow30)
            {
                clientSocket.reqFundamentalData(reqId, item, "RESC", null);
                reqId++;
            }

            // Request to individual contract
            //clientSocket.reqFundamentalData(1, contract_10, "RESC", null);            
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            //Exiting application needs disconnect and then close, otherwise no complete shutdown of app.
            ibClient.ClientSocket.eDisconnect();
            Close();
        }

        private void btnCopyListbox_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lstData.Items.Count; i++)
            {
                lstData.SetSelected(i, true);
            }
            string s = "";
            foreach (object o in lstData.SelectedItems)
            {
                s += o.ToString() + "\r\n";
            }
            Clipboard.SetText(s);
        }

        static void selectCultureUS()
        {
            CultureInfo culture;
            culture = CultureInfo.CreateSpecificCulture("en-US");

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
