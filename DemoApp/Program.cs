﻿using System;
using System.Runtime.InteropServices;
using PCSC;
using SmartCardApi.DataGroups;
using SmartCardApi.Infrastructure;
using SmartCardApi.MRZ;
using SmartCardApi.SmartCard;
using SmartCardApi.SmartCard.Reader;

namespace DemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var contextFactory2 = ContextFactory.Instance;
            var context = contextFactory2.Establish(SCardScope.System);


            var readerNames = context.GetReaders();
            foreach (var readerName in readerNames)
            {
                Console.WriteLine(readerName);
            }


            var contextFactory = ContextFactory.Instance;
            SCardMonitor monitor = new SCardMonitor(contextFactory, SCardScope.System);
            monitor.CardInserted += new CardInsertedEvent(CardInsertEventHandler);

            monitor.Start("ACS CCID USB Reader 0");
            //monitor.Start("OMNIKEY CardMan 5x21-CL 0");

            Console.ReadKey();
        }


        static void CardInsertEventHandler(object sender, CardStatusEventArgs e)
        {

            var cardContext = ContextFactory.Instance.Establish(SCardScope.System);
            var readerName = e.ReaderName;
            var readerNames = cardContext.GetReaders();

            using (var reader = new SCardReader(cardContext))
            {
                var cardError = reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                if (cardError == SCardError.Success)
                {
                    SCardProtocol proto;
                    SCardState state;
                    byte[] atr;

                    var sc = reader.Status(
                                out readerNames,
                                out state,
                                out proto,
                                out atr);

                    sc = reader.BeginTransaction();
                    if (sc != SCardError.Success)
                    {
                        Console.WriteLine("Could not begin transaction.");
                        Console.ReadKey();
                        return;
                    }
                    Console.WriteLine("Connected with protocol {0} in state {1}", proto, state);
                    Console.WriteLine("Card ATR: {0}", BitConverter.ToString(atr));

                    var mrzInfo = new MRZInfo(
                                        "12IB34415", 
                                        new DateTime(1992, 06, 16), 
                                        new DateTime(2022, 10, 08)
                                  ); //"12IB34415792061602210089" K

                    //var mrzInfo = new MRZInfo("15IC69034", new DateTime(1996,11,26), new DateTime(2026, 06, 11)); //"496112612606118" Bagdavadze
                    //var mrzInfo = "13ID37063295110732402055";     // + Shako
                    //var mrzInfo = "13IB90080296040761709252";   // + guka 
                    //var mrzInfo = "13ID40308689022472402103";     // + Giorgio
 
                    var smartCard = new SmartCard(
                                        new BacReader(
                                            new SecuredReader(
                                                    mrzInfo,
                                                    new WrReader(
                                                        new LogedReader(
                                                            reader
                                                        )
                                                    )
                                                )
                                        )
                                    );
    
                    var dg1Content = smartCard.DG1().Content();
                    var dg2Content = smartCard.DG2().Content();
                    var dg7Content = smartCard.DG7().Content();
                    var dg11Content = smartCard.DG11().Content();
                    var dg12Content = smartCard.DG12().Content();

                    reader.EndTransaction(SCardReaderDisposition.Leave);
                    reader.Disconnect(SCardReaderDisposition.Reset);

                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Error message: {0}\n", SCardHelper.StringifyError(cardError));
                }
            }
        }
    }
}