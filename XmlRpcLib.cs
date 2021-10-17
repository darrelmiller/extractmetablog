using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;

namespace ExtractMetablog {
    public static class XmlRpcLib {
        
        public static HttpContent CreateRPCCall(string methodName, params object[] parameters) {
            
            var xParams =  new System.Collections.Generic.List<XElement>();
            foreach(var parm in parameters) {

                XElement xValue;
                switch(parm) {
                    case int iParm:
                        xValue = new XElement("int", iParm);
                        break;
                    default:
                        xValue = new XElement("string", parm);
                        break;
                }
                
                xParams.Add(new XElement("param", new XElement("value", xValue)));
            }
            XElement call = new XElement("methodCall",
                    new XElement("methodName",methodName),
                    new XElement("params",xParams)
            );

            return new StringContent(call.ToString(), UTF8Encoding.UTF8,"text/xml");
        }

        public static void ParseStruct<T>(XElement xStruct,IDictionary<string, Action<T,string>> handlers, T target) {

            foreach(var member in xStruct.Elements("member")) {
                Action<T,string> handler = null;
                handlers.TryGetValue(member.Element("name").Value, out handler);
                if (handler != null) {
                    handler(target, member.Element("value").Value);
                }
            }
        }
    }
}