<Query Kind="Program">
  <Namespace>System.Xml.Schema</Namespace>
</Query>

/*
Checks if sender.xml and receiver.xml has same ids.
	
To use this script, download and install LINQPad from https://www.linqpad.net/
*/

string directory = Path.GetDirectoryName(LINQPad.Util.CurrentQueryPath);
XNamespace tns = "http://www.oasis-open.org/committees/ebxml-cppa/schema/cpp-cpa-2_0.xsd";

void Main()
{
	var receiverXml = XDocument.Load(Path.Combine(directory, "AMQP-mal-reciever.xml"));
	var senderXml = XDocument.Load(Path.Combine(directory, "AMQP-mal-sender.xml"));
	
	CompareIDsUsed(receiverXml, senderXml);
	CompareUUIDsUsed(receiverXml, senderXml);
}

void CompareIDsUsed(XDocument receiverXml, XDocument senderXml)
{
	var receiverIds = GetIDsUsed(receiverXml, true);
	var senderIds = GetIDsUsed(senderXml, true);

	var receiverIdSet = new HashSet<string>(receiverIds);
	var senderIdSet = new HashSet<string>(senderIds);

	Console.WriteLine($"IDs in receiver.xml: {receiverIds.Count()}");
	Console.WriteLine($"IDs in sender.xml: {senderIds.Count()}");

	if (senderIdSet.SetEquals(receiverIdSet))
	{
		Console.WriteLine("receiver and sender contain exactly the same id elements");
	}
	else
	{
		(from id in receiverIdSet where !senderIdSet.Contains(id) select id).Dump("In receiver, not in sender");
		(from id in senderIdSet where !receiverIdSet.Contains(id) select id).Dump("In sender, not in receiver");
	}

	FindDuplicates(receiverIds).Dump("Duplicate IDs in receiver.xml");
	FindDuplicates(GetIDsUsed(receiverXml, false)).Dump("Duplicate IDs in receiver.xml, disregarding element name prefix");

	FindDuplicates(senderIds).Dump("Duplicate IDs in sender.xml");
	FindDuplicates(GetIDsUsed(senderXml, false)).Dump("Duplicate IDs in sender.xml, disregarding element name prefix");
}


void CompareUUIDsUsed(XDocument receiverXml, XDocument senderXml)
{
	var receiverUUIDs = GetUUIDsUsed(receiverXml);
	var senderUUIDs = GetUUIDsUsed(senderXml);
	
	var receiverUUIDSet = new HashSet<string>(receiverUUIDs);
	var senderUUIDSet = new HashSet<string>(senderUUIDs);
	if (receiverUUIDSet.SetEquals(senderUUIDSet))
	{
		Console.WriteLine("Receiver and sender contains exactly same UUIDs");
	}
	else
	{
		(from id in receiverUUIDSet where !senderUUIDSet.Contains(id) select id).Dump("In receiver, not in sender");
		(from id in senderUUIDSet where !receiverUUIDSet.Contains(id) select id).Dump("In sender, not in receiver");
	}

	FindDuplicates(receiverUUIDs).Dump("Duplicate UUIDs in receiver.xml");
	FindDuplicates(senderUUIDs).Dump("Duplicate UUIDs in sender.xml");
}

IEnumerable<string> GetUUIDsUsed(XDocument xdoc)
{
	return
		from element in xdoc.Descendants()
		where element.Attribute(tns + "uuid") != null
		select
			element.Attribute(tns + "uuid").Value;
}


IEnumerable<string> GetIDsUsed(XDocument xdoc, bool withPrefix)
{
	return 
		from element in xdoc.Descendants() 
		where element.Attribute(tns + "id") != null 
		select 
			(withPrefix ? element.Name.LocalName + "/" : "") + element.Attribute(tns + "id").Value;
}

IEnumerable<T> FindDuplicates<T>(IEnumerable<T> input)
{
	return input.GroupBy(item => item).Where(group => group.Count() > 1).Select(group => group.Key);
}