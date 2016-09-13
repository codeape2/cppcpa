<Query Kind="Program">
  <Namespace>System.Xml.Schema</Namespace>
</Query>

/*
Checks for the following errors:

- XSD Schema validation
  XSD Schema validation checks conformance to cpa-cpp schema, including the following:
  	- duplicate ids
	- wrong idref references
	

To use this script, download and install LINQPad from https://www.linqpad.net/
*/

string directory = Path.GetDirectoryName(LINQPad.Util.CurrentQueryPath);

Dictionary<string, string> namespaces = new Dictionary<string, string>
{
	["http://www.oasis-open.org/committees/ebxml-cppa/schema/cpp-cpa-2_0.xsd"] = "cpp-cpa-2_0.xsd",
	["http://www.w3.org/XML/1998/namespace"] = "xml.xsd",
	["http://www.w3.org/2000/09/xmldsig#"] = "xmldsig-core-schema.xsd",
	["http://www.w3.org/1999/xlink"] = "xlink.xsd"
};

void Main()
{
	CheckFile(Path.Combine(directory, "AMQP-mal-reciever.xml"));
	CheckFile(Path.Combine(directory, "AMQP-mal-sender.xml"));
}

void CheckFile(string filename)
{
	Console.WriteLine($"Checking {filename}");
	if (ValidateUsingXsd(filename))
	{
		Console.WriteLine("XSD validation OK!");
	}
	else
	{
		Console.WriteLine("ERROR in XSD validation!");
	}
}

bool ValidateUsingXsd(string filename)
{
	var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
	foreach (var targetNamespace in namespaces.Keys)
	{
		settings.Schemas.Add(
			targetNamespace, 
			XmlReader.Create(
				Path.Combine(directory, "schemas", namespaces[targetNamespace]), 
				new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse }
			)
		);
	}

	using (var reader = XmlReader.Create(filename, settings))
	{
		try
		{
			while (reader.Read()) ;
		}
		catch (XmlSchemaValidationException validationException)
		{
			Console.WriteLine($"{validationException.SourceUri} ({validationException.LineNumber}:{validationException.LinePosition}): {validationException.Message}");
			return false;
		}
	}
	return true;
}