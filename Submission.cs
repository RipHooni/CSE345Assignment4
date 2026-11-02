using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Net;



/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/


namespace ConsoleApp1
{


    public class Submission
    {
        // load xml from github pages
        public static string xmlURL = "https://riphooni.github.io/CSE345Assignment4/NationalParks.xml";
        // load errorxml from github pages
        public static string xmlErrorURL = "https://riphooni.github.io/CSE345Assignment4/NationalParksErrors.xml";
        // load xsd from github pages
        public static string xsdURL = "https://riphooni.github.io/CSE345Assignment4/NationalParks.xsd";

        public static void Main(string[] args)
        {
            // validate against xsd then print
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            // validate error'd against xsd then print
            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            // convert xml to json then print
            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            //return "No Error" if XML is valid. Otherwise, return the desired exception message.
            // create bare error message
            StringBuilder err = new StringBuilder();
            
            try
            {
                // download xsd/xml from urls
                string xsdStr = DownloadContent(xsdUrl);
                string xmlStr = DownloadContent(xmlUrl);

                // create schema vairable
                XmlSchemaSet schema = new XmlSchemaSet();
                // read over xsd
                using (StringReader xsd = new StringReader(xsdStr))
                // read over xml and consume schema
                using (XmlReader xsdRead =  XmlReader.Create(xsd)) { schema.Add(null, xsdRead); }

                // obtain validation events and set reader to loaded schema
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                // create error message from handler
                settings.ValidationEventHandler += (sender, e) => { err.AppendLine(e.Severity + ": " + e.Message + " (Line " + e.Exception?.LineNumber + ", Pos " + e.Exception?.LinePosition + ")"); };

                try
                {
                    // read xml over and validate on node
                    using (StringReader xml = new StringReader(xmlStr))
                    using (XmlReader read = XmlReader.Create(xml, settings)) { while (read.Read()) {} }
                }
                // throw error message
                catch (XmlException exc) { err.AppendLine("XML Error: " + exc.Message + " (Line " + exc.LineNumber + ", Pos " + exc.LinePosition + ")"); }
            }
            // throw error message
            catch (Exception excptn) { err.AppendLine("Error: " + excptn.Message); }
            // return error or No errors are found if none
            return err.Length == 0 ? "No errors are found" : err.ToString().TrimEnd();
        }

        // download xml and convert it to json for JsonConvert.DeserializeXmlNode(jsonText) to read
        public static string Xml2Json(string xmlUrl)
        {
            // The returned jsonText needs to be de-serializable by Newtonsoft.Json package. (JsonConvert.DeserializeXmlNode(jsonText))
            // download xml contents
            string xml = DownloadContent(xmlUrl);
            // load xml into doc for configurations
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            // remove first line containing ?xml
            if (xmlDoc.FirstChild is XmlDeclaration) { xmlDoc.RemoveChild(xmlDoc.FirstChild); }
            
            // force elemnent as JSON array
            const string jsonArr = "http://james.newtonking.com/projects/json";
            // iterate over required tags
            foreach (string tag in new[] {"NationalPark", "Phone"})
            {
                // iterate over elements with tag
                foreach (XmlElement elem in xmlDoc.GetElementsByTagName(tag))
                {
                    // json array = true to each element
                    XmlAttribute attArr = xmlDoc.CreateAttribute("json", "Array", jsonArr);
                    attArr.Value = "true";
                    elem.SetAttributeNode(attArr);
                }
            }
            // convert doc to JSON and return
            return JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.Indented);
        }

        // Helper method to download content from URL
        private static string DownloadContent(string url)
        {
            // enable TLS12
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            // create web client  to download
            using (System.Net.WebClient cli = new System.Net.WebClient())
            {
                cli.Encoding = Encoding.UTF8;
                // byte-order in order to parse
                return cli.DownloadString(url).TrimStart('\uFEFF');
            }
        }
    }

}
