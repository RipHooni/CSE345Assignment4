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
        // load files
        public static string xmlURL = "https://riphooni.github.io/CSE345Assignment4/NationalParks.xml";
        public static string xmlErrorURL = "https://riphooni.github.io/CSE345Assignment4/NationalParksErrors.xml";
        public static string xsdURL = "https://riphooni.github.io/CSE345Assignment4/NationalParks.xsd";

        public static void Main(string[] args)
        {
            // validate against xsd
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            // vakudate error'd against xsd
            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            // convert to json
            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            //return "No Error" if XML is valid. Otherwise, return the desired exception message.
            StringBuilder err = new StringBuilder();
            
            try
            {
                // download xsd/xml from urls
                string xsdStr = DownloadContent(xsdUrl);
                string xmlStr = DownloadContent(xmlUrl);

                // load schema from downnloads
                XmlSchemaSet schema = new XmlSchemaSet();
                using (StringReader xsd = new StringReader(xsdStr))
                using (XmlReader xsdRead =  XmlReader.Create(xsd)) { schema.Add(null, xsdRead); }

                // obtain validation events and set reader
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                // create error message from handler
                settings.ValidationEventHandler += (sender, e) => { err.AppendLine(e.Severity + ": " + e.Message + " (Line " + e.Exception?.LineNumber + ", Pos " + e.Exception?.LinePosition + ")"); };

                try
                {
                    using (StringReader xml = new StringReader(xmlStr))
                    using (XmlReader read = XmlReader.Create(xml, settings)) { while (read.Read()) {} }
                }
                catch (XmlException exc) { err.AppendLine("XML Error: " + exc.Message + " (Line " + exc.LineNumber + ", Pos " + exc.LinePosition + ")"); }
            }
            catch (Exception excptn) { err.AppendLine("Error: " + excptn.Message); }
            return err.Length == 0 ? "No errors are found" : err.ToString().TrimEnd();
        }

        public static string Xml2Json(string xmlUrl)
        {
            // The returned jsonText needs to be de-serializable by Newtonsoft.Json package. (JsonConvert.DeserializeXmlNode(jsonText))
            string xml = DownloadContent(xmlUrl);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            if (xmlDoc.FirstChild is XmlDeclaration) { xmlDoc.RemoveChild(xmlDoc.FirstChild); }
            
            const string jsonArr = "http://james.newtonking.com/projects/json";
            foreach (string tag in new[] {"NationalPark", "Phone"})
            {
                foreach (XmlElement elem in xmlDoc.GetElementsByTagName(tag))
                {
                    XmlAttribute attArr = xmlDoc.CreateAttribute("json", "Array", jsonArr);
                    attArr.Value = "true";
                    elem.SetAttributeNode(attArr);
                }
            }
            return JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.Indented);
        }

        // Helper method to download content from URL
        private static string DownloadContent(string url)
        {
            // obtain content and encode
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            using (System.Net.WebClient cli = new System.Net.WebClient())
            {
                cli.Encoding = Encoding.UTF8;
                return cli.DownloadString(url).TrimStart('\uFEFF');
            }
        }
    }

}
