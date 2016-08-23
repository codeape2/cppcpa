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
	var receiverUUIDSet = new HashSet<string>(GetUUIDsUsed(receiverXml));
	var senderUUIDSet = new HashSet<string>(GetUUIDsUsed(senderXml));
	if (receiverUUIDSet.SetEquals(senderUUIDSet))
	{
		Console.WriteLine("Receiver and sender contains exactly same UUIDs");
	}
	else
	{
		(from id in receiverUUIDSet where !senderUUIDSet.Contains(id) select id).Dump("In receiver, not in sender");
		(from id in senderUUIDSet where !receiverUUIDSet.Contains(id) select id).Dump("In sender, not in receiver");
	}

	FindDuplicates(GetUUIDsUsed(receiverXml, includeVersion: true)).Dump("Duplicate version/UUIDs in receiver.xml");
	FindDuplicates(GetUUIDsUsed(senderXml, includeVersion: true)).Dump("Duplicate version/UUIDs in sender.xml");

	FindSameUUIDButDifferentName(receiverXml, "receiver.xml");
	FindSameUUIDButDifferentName(senderXml, "sender.xml");
}

void FindSameUUIDButDifferentName(XDocument xDoc, string filename)
{
	var withUUID =
		from element in xDoc.Descendants()
		where element.Attribute(tns + "uuid") != null
		group element by element.Attribute(tns + "uuid").Value into grouping
		where grouping.Count() > 1 && (from ge in grouping select ge.Attribute(tns + "name").Value).Distinct().Count() > 1
		select new { uuid = grouping.Key, names = (from ge in grouping select ge.Attribute(tns + "name").Value) };
	withUUID.Dump($"Same UUID but different name ({filename})");

}

IEnumerable<string> GetUUIDsUsed(XDocument xdoc, bool includeVersion = false)
{
	return
		from element in xdoc.Descendants()
		where element.Attribute(tns + "uuid") != null
		select
			(includeVersion ? element.Attribute(tns + "version").Value : "") + element.Attribute(tns + "uuid").Value;
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