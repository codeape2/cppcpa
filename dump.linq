<Query Kind="Program">
  <Namespace>System.Xml.Schema</Namespace>
</Query>

/* Dumps Process specifications and roles */

string directory = Path.GetDirectoryName(LINQPad.Util.CurrentQueryPath);
XNamespace tns = "http://www.oasis-open.org/committees/ebxml-cppa/schema/cpp-cpa-2_0.xsd";

void Main()
{
	var receiverXml = XDocument.Load(Path.Combine(directory, "AMQP-mal-reciever.xml"));
	var senderXml = XDocument.Load(Path.Combine(directory, "AMQP-mal-sender.xml"));
	
	DumpCPP(receiverXml, "receiver.xml");
	DumpCPP(senderXml, "sender.xml");
}

void DumpCPP(XDocument xdoc, string title)
{
	title.Dump();
	var collaborationRoles = 
		from element in xdoc.Descendants(tns + "CollaborationRole")
		let ps = element.Descendants(tns + "ProcessSpecification").Single()
		select 
			new
			{
				ProcessSpecificationName = ps.Attribute(tns + "name").Value,
				ProcessSpecificationVersion = ps.Attribute(tns + "version").Value,
				RoleName = element.Descendants(tns + "Role").Single().Attribute(tns + "name").Value
			};
	collaborationRoles.Dump();
}