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
	DumpProcSpecsAndRoles(xdoc);
	DumpSimpleParts(xdoc);
}

void DumpSimpleParts(XDocument xdoc)
{
	var simpleParts =
		from element in xdoc.Descendants(tns + "SimplePart")
		select
			new
			{
				Id = element.Attribute(tns + "id").Value,
				MimeType = element.Attribute(tns + "mimetype").Value,
				Namespaces = from ns in element.Descendants(tns + "NamespaceSupported")
							 select new
							 {
							 	Location = ns.Attribute(tns + "location").Value,
								Version = ns.Attribute(tns + "version").Value,
							 	Namespace = ns.Value
							 }
			};
	simpleParts.Dump();
}

void DumpProcSpecsAndRoles(XDocument xdoc)
{
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