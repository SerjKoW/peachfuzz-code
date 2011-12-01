﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.IO;

namespace Peach.Core.Test.Mutators
{
    [TestFixture]
    class SizedVarianceMutatorTests
    {
        int? testValue = null;
        List<int?> listVals = new List<int?>();

        [Test]
        public void Test1()
        {
            // standard test ...

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"sizeRelation1\" length=\"2\">" +
                "           <Relation type=\"size\" of=\"string1\"/>" +
                "           <Hint name=\"SizedVarianceMutator-N\" value=\"5\"/>" +
                "       </String>" +
                "       <String name=\"string1\" value=\"Hello, World\"/>" +
                "   </DataModel>" +

                "   <StateModel name=\"TheState\" initialState=\"Initial\">" +
                "       <State name=\"Initial\">" +
                "           <Action type=\"output\">" +
                "               <DataModel ref=\"TheDataModel\"/>" +
                "           </Action>" +
                "       </State>" +
                "   </StateModel>" +

                "   <Test name=\"TheTest\">" +
                "       <StateModel ref=\"TheState\"/>" +
                "       <Publisher class=\"Stdout\"/>" +
                "   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // remove starting default value (100)
            //listVals.RemoveAt(0);

            // verify values
            //for (int i = 0; i <= 100; ++i)
            //    Assert.AreEqual(150 - i, listVals[i]);

            // reset
            //testValue = null;
            //listVals.Clear();
        }

        void Action_FinishedTest(Dom.Action action)
        {
            //Number n1 = (Number)action.dataModel[0];
            //Number n2 = (Number)action.dataModel[1];
            int x = 0;
        }
    }
}

// end
