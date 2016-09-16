<Query Kind="Program">
  <Namespace>System.Xml.Schema</Namespace>
  <Namespace>System.Xml.Resolvers</Namespace>
</Query>

/*
Checks for the following errors:

- XSD Schema validation
  XSD Schema validation checks conformance to cpa-cpp schema, including the following:
  	- duplicate ids
	- wrong idref references
	

To use this script, download and install LINQPad from https://www.linqpad.net/
*/

static string directory = Path.GetDirectoryName(LINQPad.Util.CurrentQueryPath);


static List<Tuple<string, string>> namespaces = new List<System.Tuple<string, string>>
{
	Tuple.Create("http://www.w3.org/XML/1998/namespace", "xml.xsd"),
	Tuple.Create("http://www.w3.org/1999/xlink", "xlink.xsd"),
	Tuple.Create("http://www.oasis-open.org/committees/ebxml-cppa/schema/cpp-cpa-2_0.xsd", "cpp-cpa-2_0.xsd"),
	Tuple.Create("http://www.w3.org/2000/09/xmldsig#", "xmldsig-core-schema.xsd")
};

void Main()
{
	var schemaSet = CreateSchemaSet();
	CheckFile(Path.Combine(directory, "AMQP-mal-reciever.xml"), schemaSet);
	CheckFile(Path.Combine(directory, "AMQP-mal-sender.xml"), schemaSet);
}

void CheckFile(string filename, XmlSchemaSet schemaSet)
{
	Console.WriteLine($"Checking {filename}");
	
	// check well-formedness
	XDocument.Load(filename);
	
	if (ValidateUsingXsd(filename, schemaSet))
	{
		Console.WriteLine("XSD validation OK!");
	}
	else
	{
		Console.WriteLine("ERROR in XSD validation!");
	}
}

class MyResolver : XmlResolver
{
	public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
	{
		Debug.Assert(role == null);
		Debug.Assert(ofObjectToReturn == null);
		//$"GetEntity({absoluteUri}, {role}, {ofObjectToReturn})".Dump();
		
		return GetStream(absoluteUri.ToString());
	}

	private Stream GetStream(string uri)
	{
		foreach (var nstuple in namespaces)
		{
			if (nstuple.Item1 == uri)
			{
				return new FileStream(Path.Combine(directory, "schemas", nstuple.Item2), FileMode.Open);
			}
		}
		throw new ArgumentException($"Unknown: {uri}");
	}
}

XmlSchemaSet CreateSchemaSet()
{
	var schemas = new XmlSchemaSet();
	var preloadedResolver = new MyResolver();
	schemas.XmlResolver = preloadedResolver;
	
	foreach (var targetNamespace in namespaces)
	{
		schemas.Add(targetNamespace.Item1, XDocument.Load(Path.Combine(directory, "schemas", targetNamespace.Item2)).CreateReader());
	}
	
	return schemas;
}

bool ValidateUsingXsd(string filename, XmlSchemaSet schemaSet)
{
	using (var reader = XmlReader.Create(filename, new XmlReaderSettings { ValidationType = ValidationType.Schema, Schemas = schemaSet }))
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
